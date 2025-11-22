using UnityEngine;
using VContainer;
using VContainer.Unity;
using UIFramework.Animation;
using UIFramework.Core;
using UIFramework.Events;
using UIFramework.Navigation;

namespace UIFramework.DependencyInjection
{
    /// <summary>
    /// VContainer installer for the UIFramework.
    /// Registers all framework services with proper lifetimes.
    /// </summary>
    public class UIFrameworkInstaller : LifetimeScope
    {
        [SerializeField] private UIFrameworkConfig config;

        protected override void Configure(IContainerBuilder builder)
        {
            if (config == null)
            {
                Debug.LogError("[UIFrameworkInstaller] UIFrameworkConfig is not assigned!");
                return;
            }

            Debug.Log("[UIFrameworkInstaller] Configuring UIFramework services...");

            // Register configuration
            builder.RegisterInstance(config);

            // Register core services
            RegisterCoreServices(builder);

            // Register navigation
            RegisterNavigationServices(builder);

            // Register animation
            RegisterAnimationServices(builder);

            // Register loading
            RegisterLoadingServices(builder);

            // Register pooling (optional)
            if (config.EnablePooling)
            {
                RegisterPoolingServices(builder);
            }

            Debug.Log("[UIFrameworkInstaller] UIFramework services configured.");
        }

        private void RegisterCoreServices(IContainerBuilder builder)
        {
            // Event bus - singleton
            builder.Register<UIEventBus>(Lifetime.Singleton);

            // View factory - singleton
            builder.Register<IUIViewFactory, UIViewFactory>(Lifetime.Singleton);
        }

        private void RegisterNavigationServices(IContainerBuilder builder)
        {
            // State machine - singleton
            builder.Register<UIStateMachine>(Lifetime.Singleton);

            // Navigation stack - singleton
            builder.Register<INavigationStack, NavigationStack>(Lifetime.Singleton)
                .WithParameter(config.MaxNavigationStackDepth);

            // Navigator facade - singleton
            builder.Register<UINavigator>(Lifetime.Singleton);
        }

        private void RegisterAnimationServices(IContainerBuilder builder)
        {
            // Animator - singleton
            builder.Register<IUIAnimator, DOTweenUIAnimator>(Lifetime.Singleton);
        }

        private void RegisterLoadingServices(IContainerBuilder builder)
        {
            if (config.UseAddressables)
            {
                #if UIFRAMEWORK_ADDRESSABLES
                builder.Register<IUILoader, UIFramework.Loading.AddressablesUILoader>(Lifetime.Singleton);
                Debug.Log("[UIFrameworkInstaller] Registered AddressablesUILoader");
                #else
                Debug.LogWarning("[UIFrameworkInstaller] Addressables is enabled in config but not installed. Falling back to Resources.");
                builder.Register<IUILoader, UIFramework.Loading.ResourcesUILoader>(Lifetime.Singleton);
                #endif
            }
            else
            {
                builder.Register<IUILoader, UIFramework.Loading.ResourcesUILoader>(Lifetime.Singleton);
                Debug.Log("[UIFrameworkInstaller] Registered ResourcesUILoader");
            }
        }

        private void RegisterPoolingServices(IContainerBuilder builder)
        {
            //builder.Register<IUIObjectPool, UIObjectPool>(Lifetime.Singleton);
        }

        /// <summary>
        /// Called after the container is built.
        /// Use this to perform initialization that requires resolved services.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // Initialize global ServiceLocator for legacy/convenience access
            ServiceLocator.Initialize(Container);
        }

        protected override void OnDestroy()
        {
            ServiceLocator.Reset();
            base.OnDestroy();
        }
    }
}
