using VContainer;
using VContainer.Unity;
using UIFramework.Core;

namespace UIFramework.DI
{
    /// <summary>
    /// Optional: Lifetime scope for ViewModels with specific dependencies.
    /// Use this if you need to inject services that are specific to a UI context.
    /// </summary>
    public class UIViewModelScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Register ViewModels as transient (new instance each time)
            // Users can register their ViewModels here or use RegisterViewModel<T>()

            // Example:
            // builder.Register<MainMenuViewModel>(Lifetime.Transient);
            // builder.Register<SettingsViewModel>(Lifetime.Transient);
        }

        /// <summary>
        /// Helper method to register a ViewModel with transient lifetime.
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type.</typeparam>
        public void RegisterViewModel<TViewModel>() where TViewModel : IViewModel
        {
            var builder = Container as IContainerBuilder;
            builder?.Register<TViewModel>(Lifetime.Transient);
        }
    }
}
