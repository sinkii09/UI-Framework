using System;
using UnityEngine;
using UIFramework.MVVM;

namespace UIFramework.Core
{
    /// <summary>
    /// Base class for all UI Views implementing the MVVM pattern.
    /// Views are responsible for visual presentation only - no business logic.
    /// </summary>
    /// <typeparam name="TViewModel">The ViewModel type for this View.</typeparam>
    public abstract class UIView<TViewModel> : MonoBehaviour, IUIView where TViewModel : IViewModel
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
        /// The CanvasGroup component (added automatically if not present).
        /// </summary>
        [SerializeField] protected CanvasGroup canvasGroup;

        /// <summary>
        /// Whether this View has been initialized.
        /// </summary>
        protected bool IsInitialized { get; private set; }

        #region IUIView Implementation

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public RectTransform RectTransform => transform as RectTransform;

        #endregion

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
        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            ViewModel?.OnViewShown();
        }

        /// <summary>
        /// Hides the View from the user.
        /// </summary>
        public virtual void Hide()
        {
            ViewModel?.OnViewHidden();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Destroys the View and disposes the ViewModel.
        /// </summary>
        public virtual void Destroy()
        {
            // Dispose ViewModel
            ViewModel?.Dispose();

            // Destroy GameObject
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
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
