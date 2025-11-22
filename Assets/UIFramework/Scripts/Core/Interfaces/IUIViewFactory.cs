using System.Threading;
using System.Threading.Tasks;

namespace UIFramework.Core
{
    /// <summary>
    /// Factory interface for creating UI views with their ViewModels.
    /// Handles instantiation, dependency injection, and initialization.
    /// </summary>
    public interface IUIViewFactory
    {
        /// <summary>
        /// Creates a view with its ViewModel asynchronously.
        /// </summary>
        /// <typeparam name="TView">The view type.</typeparam>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="viewModel">Optional view model instance (will be created via DI if null).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created and initialized view.</returns>
        Task<TView> CreateAsync<TView, TViewModel>(
            TViewModel viewModel = null,
            CancellationToken cancellationToken = default)
            where TView : UIView<TViewModel>
            where TViewModel : class, IViewModel;

        /// <summary>
        /// Destroys a view and its ViewModel.
        /// </summary>
        /// <param name="view">The view to destroy.</param>
        void Destroy(IUIView view);
    }
}
