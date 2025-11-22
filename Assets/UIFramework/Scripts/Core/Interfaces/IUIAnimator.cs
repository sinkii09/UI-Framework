using System.Threading;
using System.Threading.Tasks;
using UIFramework.Animation;

namespace UIFramework.Core
{
    /// <summary>
    /// Interface for animating UI transitions.
    /// Provides an abstraction layer over animation systems like DOTween.
    /// </summary>
    public interface IUIAnimator
    {
        /// <summary>
        /// Plays a UI transition animation asynchronously.
        /// </summary>
        /// <param name="transition">The transition to animate.</param>
        /// <param name="cancellationToken">Token to cancel the animation.</param>
        /// <returns>A task that completes when the animation finishes.</returns>
        Task AnimateAsync(UITransition transition, CancellationToken cancellationToken = default);
    }
}
