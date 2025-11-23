using System;
using System.Threading.Tasks;
using UIFramework.Animation;
using UIFramework.DI;
using UIFramework.MVVM;
using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Non-generic base class for all UI Views.
    /// Allows editor scripts to reference views without knowing the generic type parameter.
    /// </summary>
    public abstract class UIViewBase : MonoBehaviour, IUIView
    {
        /// <summary>
        /// The CanvasGroup component (added automatically if not present).
        /// </summary>
        [SerializeField] protected CanvasGroup canvasGroup;

        #region IUIView Implementation

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public RectTransform RectTransform => transform as RectTransform;

        #endregion

        #region Animation Hooks

        /// <summary>
        /// Override to define the show animation. Return null for instant show.
        /// </summary>
        protected virtual UITransition GetShowTransition() => null;

        /// <summary>
        /// Override to define the hide animation. Return null for instant hide.
        /// </summary>
        protected virtual UITransition GetHideTransition() => null;

        #endregion

        /// <summary>
        /// Shows the View to the user with optional animation.
        /// </summary>
        public virtual async Task Show()
        {
            gameObject.SetActive(true);

            // Disable interaction during animation
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            // Play show animation if defined
            var transition = GetShowTransition();
            if (transition != null)
            {
                var animator = ServiceLocator.Get<IUIAnimator>();
                if (animator != null)
                {
                    transition.Target = transform;
                    try
                    {
                        await animator.AnimateAsync(transition);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[UIView] Show animation failed: {ex.Message}");
                    }
                }
            }

            // Enable interaction after animation completes
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Hides the View from the user with optional animation.
        /// </summary>
        public virtual async Task Hide()
        {
            // Disable interaction during hide animation
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            // Play hide animation if defined
            var transition = GetHideTransition();
            if (transition != null)
            {
                var animator = ServiceLocator.Get<IUIAnimator>();
                if (animator != null)
                {
                    transition.Target = transform;
                    try
                    {
                        await animator.AnimateAsync(transition);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[UIView] Hide animation failed: {ex.Message}");
                    }
                }
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Destroys the View.
        /// </summary>
        public virtual void Destroy()
        {
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Generic base class for all UI Views implementing the MVVM pattern.
    /// Views are responsible for visual presentation only - no business logic.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type for this View.</typeparam>
    public abstract class UIView<TViewModel> : UIViewBase where TViewModel : IViewModel
    {
        /// <summary>
        /// The ViewModel bound to this View.
        /// </summary>
        protected TViewModel ViewModel { get; private set; }

        /// <summary>
        /// PropertyBinder for managing bindings (optional, can be added at runtime).
        /// </summary>
        protected PropertyBinder Binder { get; private set; }

        /// <summary>
        /// Whether this View has been initialized.
        /// </summary>
        protected bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the View with its ViewModel.
        /// </summary>
        /// <param name="viewModel">The ViewModel to bind to this View.</param>
        public void Initialize(TViewModel viewModel)
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"[UIView] {GetType().Name} already initialized.");
                return;
            }

            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            // Ensure CanvasGroup exists
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Ensure PropertyBinder exists
            Binder = GetComponent<PropertyBinder>();
            if (Binder == null)
                Binder = gameObject.AddComponent<PropertyBinder>();

            // Initialize ViewModel
            ViewModel.Initialize();

            // Allow derived classes to perform custom initialization
            OnViewModelSet();

            // Bind properties to UI
            BindViewModel();

            IsInitialized = true;
        }

        /// <summary>
        /// Called after the ViewModel is set but before BindViewModel().
        /// Override to perform custom initialization.
        /// </summary>
        protected virtual void OnViewModelSet()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Override this to bind ViewModel properties to UI elements.
        /// Use Bind() helper methods for automatic subscription management.
        /// </summary>
        protected abstract void BindViewModel();

        /// <summary>
        /// Shows the View to the user.
        /// </summary>
        public override async Task Show()
        {
            await base.Show();
            ViewModel?.OnViewShown();
        }

        /// <summary>
        /// Hides the View from the user.
        /// </summary>
        public override async Task Hide()
        {
            ViewModel?.OnViewHidden();
            await base.Hide();
        }

        /// <summary>
        /// Destroys the View and disposes the ViewModel.
        /// </summary>
        public override void Destroy()
        {
            ViewModel?.Dispose();
            base.Destroy();
        }

        #region Binding Helper Methods

        /// <summary>
        /// Binds a reactive property to an update action.
        /// Automatically managed - will unsubscribe when the View is destroyed.
        /// </summary>
        /// <typeparam name="T">The property value type.</typeparam>
        /// <param name="property">The reactive property.</param>
        /// <param name="updateAction">The action to invoke on value changes.</param>
        /// <returns>The subscription handle.</returns>
        protected IDisposable Bind<T>(IReadOnlyReactiveProperty<T> property, Action<T> updateAction)
        {
            return Binder.Bind(property, updateAction);
        }

        /// <summary>
        /// Binds a reactive command to a button.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="button">The button.</param>
        /// <returns>The subscription handle.</returns>
        protected IDisposable Bind(ReactiveCommand command, UnityEngine.UI.Button button)
        {
            return Binder.Bind(command, button);
        }

        /// <summary>
        /// Binds a reactive command with parameter to a button.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="command">The command.</param>
        /// <param name="button">The button.</param>
        /// <param name="parameter">The parameter value.</param>
        /// <returns>The subscription handle.</returns>
        protected IDisposable Bind<T>(ReactiveCommand<T> command, UnityEngine.UI.Button button, T parameter)
        {
            return Binder.Bind(command, button, parameter);
        }

        #endregion

        protected virtual void OnDestroy()
        {
            if (ViewModel != null && !ViewModel.Equals(null))
            {
                ViewModel.Dispose();
            }
        }
    }
}
