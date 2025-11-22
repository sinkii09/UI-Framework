using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// Interface for loading UI prefabs asynchronously.
    /// Implementations can use Addressables, Resources, or custom asset systems.
    /// </summary>
    public interface IUILoader
    {
        /// <summary>
        /// Loads a UI prefab asynchronously by key/address.
        /// </summary>
        /// <typeparam name="T">The component type to retrieve from the loaded prefab.</typeparam>
        /// <param name="key">The asset key, address, or path.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The component from the loaded prefab.</returns>
        Task<T> LoadAsync<T>(string key, CancellationToken cancellationToken = default) where T : Component;

        /// <summary>
        /// Releases a loaded asset and frees memory.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="instance">The instance to release.</param>
        void Release<T>(T instance) where T : Component;
    }
}
