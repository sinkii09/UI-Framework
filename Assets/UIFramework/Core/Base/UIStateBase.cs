using System.Threading;
using System.Threading.Tasks;

namespace UIFramework.Core
{
    /// <summary>
    /// Base class for UI states in the state machine.
    /// Override OnEnterAsync/OnExitAsync to define state behavior.
    /// </summary>
    public abstract class UIStateBase : IUIState
    {
        /// <summary>
        /// Unique identifier for this state.
        /// </summary>
        public abstract string StateId { get; }

        /// <summary>
        /// Called when transitioning into this state.
        /// Override to load UI, initialize systems, etc.
        /// </summary>
        public virtual Task OnEnterAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when transitioning out of this state.
        /// Override to cleanup, save state, unload UI, etc.
        /// </summary>
        public virtual Task OnExitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called every frame while this state is active (optional).
        /// Override to implement per-frame logic.
        /// </summary>
        public virtual void OnUpdate()
        {
            // Override in derived classes if needed
        }
    }
}
