using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Interface for object pooling UI elements to improve performance.
    /// </summary>
    public interface IUIObjectPool
    {
        /// <summary>
        /// Gets an instance from the pool or creates a new one if none available.
        /// </summary>
        /// <typeparam name="T">The UI view type (must be Component and implement IUIView).</typeparam>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An instance of the requested type.</returns>
        Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : Component, IUIView;

        /// <summary>
        /// Returns an instance to the pool for reuse (generic version).
        /// </summary>
        /// <typeparam name="T">The UI view type (must be Component and implement IUIView).</typeparam>
        /// <param name="instance">The instance to return.</param>
        void Return<T>(T instance) where T : Component, IUIView;

        /// <summary>
        /// Returns an instance to the pool for reuse (non-generic version).
        /// Automatically determines the concrete type and routes to the appropriate pool.
        /// </summary>
        /// <param name="instance">The instance to return.</param>
        void Return(IUIView instance);

        /// <summary>
        /// Clears all pools and destroys all instances.
        /// </summary>
        void Clear();
    }
}
