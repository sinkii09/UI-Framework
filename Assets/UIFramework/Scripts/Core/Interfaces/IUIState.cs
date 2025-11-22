using System.Threading;
using System.Threading.Tasks;

namespace UIFramework.Core
{
    /// <summary>
    /// Interface for UI states in the state machine.
    /// Each state represents a distinct UI mode (Menu, Gameplay, Pause, etc.).
    /// </summary>
    public interface IUIState
    {
        /// <summary>
        /// Unique identifier for this state.
        /// </summary>
        string StateId { get; }

        /// <summary>
        /// Called when transitioning into this state.
        /// Use this to load UI, initialize systems, etc.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the transition.</param>
        Task OnEnterAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Called when transitioning out of this state.
        /// Use this to cleanup, save state, unload UI, etc.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the transition.</param>
        Task OnExitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Called every frame while this state is active (optional).
        /// </summary>
        void OnUpdate();
    }
}
