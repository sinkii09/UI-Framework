using System.Threading;
using System.Threading.Tasks;
using UIFramework.Core;
using UIFramework.Animation;

namespace UIFramework.Navigation
{
    /// <summary>
    /// Interface for stack-based navigation (push/pop views).
    /// </summary>
    public interface INavigationStack
    {
        /// <summary>
        /// Gets the number of views in the stack.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the currently visible view (top of stack).
        /// </summary>
        IUIView CurrentView { get; }

        /// <summary>
        /// Pushes a new view onto the stack.
        /// </summary>
        /// <typeparam name="TView">The view type.</typeparam>
        /// <typeparam name="TViewModel">The view model type.</typeparam>
        /// <param name="viewModel">Optional view model instance (will be created via DI if null).</param>
        /// <param name="transition">Optional transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created view instance.</returns>
        Task<TView> PushAsync<TView, TViewModel>(
            TViewModel viewModel = null,
            UITransition transition = null,
            CancellationToken cancellationToken = default)
            where TView : UIView<TViewModel>
            where TViewModel : class, IViewModel;

        /// <summary>
        /// Pops the current view from the stack.
        /// </summary>
        /// <param name="transition">Optional transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The view that is now at the top of the stack.</returns>
        Task<IUIView> PopAsync(
            UITransition transition = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Pops all views except the root view.
        /// </summary>
        /// <param name="transition">Optional transition animation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PopToRootAsync(
            UITransition transition = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears the entire navigation stack.
        /// </summary>
        void Clear();
    }
}
