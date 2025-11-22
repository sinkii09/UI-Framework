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
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An instance of the requested type.</returns>
        Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : Component;

        /// <summary>
        /// Returns an instance to the pool for reuse.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="instance">The instance to return.</param>
        void Return<T>(T instance) where T : Component;

        /// <summary>
        /// Pre-instantiates a number of instances for immediate availability.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="count">The number of instances to create.</param>
        void Warmup<T>(int count) where T : Component;

        /// <summary>
        /// Clears all pools and destroys all instances.
        /// </summary>
        void Clear();
    }
}
