using System;
using System.Threading;
using System.Threading.Tasks;
using UIFramework.Animation;
using UIFramework.Events;
using UIFramework.Loading;
using UIFramework.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace UIFramework.Core
{
    /// <summary>
    /// Static facade for UIFramework.
    /// External modules communicate with the framework through this API.
    /// Provides unified access to navigation, loading screens, events, and more.
    ///
    /// Usage:
    ///   await UIFramework.PushAsync&lt;MainMenuView, MainMenuViewModel&gt;();
    ///   await UIFramework.ShowLoadingAsync("Loading...");
    ///   UIFramework.PublishEvent(new GameStartEvent());
    /// </summary>
    public static class UIFramework
    {
        #region Internal Services

        private static IObjectResolver _container;
        private static UINavigator _navigator;
        private static UIEventBus _eventBus;
        private static bool _isInitialized;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the UIFramework with required services.
        /// Called automatically by UIFrameworkInstaller.
        /// </summary>
        internal static void Initialize(IObjectResolver container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;

            if (_isInitialized)
            {
                Debug.LogWarning("[UIFramework] Already initialized. Re-initializing...");
            }

            _navigator = container.Resolve<UINavigator>();
            _eventBus =  container.Resolve<UIEventBus>();

            _isInitialized = true;
        }

        /// <summary>
        /// Resets the UIFramework.
        /// Called automatically when UIFrameworkInstaller is destroyed.
        /// </summary>
        internal static void Reset()
        {
            _container = null;
            _navigator = null;
            _eventBus = null;
            _isInitialized = false;
            Debug.Log("[UIFramework] Reset.");
        }

        /// <summary>
        /// Ensures UIFramework is initialized before use.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    "UIFramework not initialized. " +
                    "Ensure UIFrameworkInstaller is in the scene and has been initialized.");
            }
        }

        /// <summary>
        /// Whether UIFramework is initialized and ready to use.
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// Gets a service from the container.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The resolved service instance.</returns>
        public static T Get<T>()
        {
            if (_container == null)
            {
                throw new InvalidOperationException(
                    "ServiceLocator has not been initialized. " +
                    "Ensure UIFrameworkInstaller is in the scene and has been initialized.");
            }

            try
            {
                return _container.Resolve<T>();
            }
            catch (VContainerException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to resolve service of type '{typeof(T).Name}'. " +
                    "Make sure the service is registered in UIFrameworkInstaller.",
                    ex);
            }
        }

        /// <summary>
        /// Tries to get a service, returning default if not registered.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <returns>The service instance, or default if not registered.</returns>
        public static T TryGet<T>() where T : class
        {
            if (_container == null)
            {
                return null;
            }

            try
            {
                return _container.Resolve<T>();
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Navigation API

        /// <summary>
        /// Pushes a new UI view onto the navigation stack.
        /// </summary>
        /// <typeparam name="TView">The view type to create.</typeparam>
        /// <typeparam name="TViewModel">The view model type for the view.</typeparam>
        /// <param name="viewModel">Optional view model instance (created via DI if null).</param>
        /// <param name="transition">Optional custom transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created view instance.</returns>
        public static Task<TView> PushAsync<TView, TViewModel>(
            TViewModel viewModel = null,
            UITransition transition = null,
            CancellationToken cancellationToken = default)
            where TView : UIView<TViewModel>
            where TViewModel : class, IViewModel
        {
            EnsureInitialized();
            return _navigator.PushAsync<TView, TViewModel>(viewModel, cancellationToken);
        }

        /// <summary>
        /// Pops the current view from the navigation stack.
        /// </summary>
        /// <param name="transition">Optional custom transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The view now at the top of the stack.</returns>
        public static Task<IUIView> PopAsync(
            UITransition transition = null,
            CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            return _navigator.PopAsync(cancellationToken);
        }

        /// <summary>
        /// Pops all views except the root.
        /// </summary>
        /// <param name="transition">Optional custom transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static Task PopToRootAsync(
            UITransition transition = null,
            CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            return _navigator.PopToRootAsync(cancellationToken);
        }

        /// <summary>
        /// Clears the entire navigation stack.
        /// </summary>
        /// <param name="includeRoot">Whether to clear the root view as well.</param>
        public static Task ClearStackAsync(bool includeRoot = true)
        {
            EnsureInitialized();
            return _navigator.ClearStack(includeRoot);
        }

        /// <summary>
        /// Gets the currently visible view from the navigation stack.
        /// </summary>
        public static IUIView CurrentView
        {
            get
            {
                EnsureInitialized();
                return _navigator.CurrentView;
            }
        }

        /// <summary>
        /// Gets the current navigation stack depth.
        /// </summary>
        public static int StackDepth
        {
            get
            {
                EnsureInitialized();
                return _navigator.StackCount;
            }
        }

        #endregion

        #region State Management API

        /// <summary>
        /// Registers a UI state with the state machine.
        /// </summary>
        /// <param name="state">The state to register.</param>
        public static void RegisterState(IUIState state)
        {
            EnsureInitialized();
            _navigator.RegisterState(state);
        }

        /// <summary>
        /// Transitions to a new UI state.
        /// </summary>
        /// <param name="stateId">The state ID to transition to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static Task ChangeStateAsync(string stateId, CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            return _navigator.ChangeStateAsync(stateId, cancellationToken);
        }

        /// <summary>
        /// Gets the current UI state.
        /// </summary>
        public static IUIState CurrentState
        {
            get
            {
                EnsureInitialized();
                return _navigator.CurrentState;
            }
        }

        /// <summary>
        /// Gets the current state ID.
        /// </summary>
        public static string CurrentStateId
        {
            get
            {
                EnsureInitialized();
                return _navigator.CurrentStateId;
            }
        }

        #endregion

        #region Event Bus API

        /// <summary>
        /// Publishes an event to all subscribers.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="event">The event instance.</param>
        public static void PublishEvent<TEvent>(TEvent @event) where TEvent : IUIEvent
        {
            EnsureInitialized();
            _eventBus.Publish(@event);
        }

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="handler">The event handler callback.</param>
        /// <returns>Disposable subscription handle.</returns>
        public static IDisposable SubscribeToEvent<TEvent>(Action<TEvent> handler) where TEvent : IUIEvent
        {
            EnsureInitialized();
            return _eventBus.Subscribe(handler);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Updates the state machine (call every frame if states use OnUpdate).
        /// </summary>
        public static void Update()
        {
            if (_isInitialized)
            {
                _navigator.Update();
            }
        }

        #endregion
    }
}
