using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Loading
{
    /// <summary>
    /// UI loader that uses Unity's Resources system.
    /// Prefabs should be located in Resources/UI/ folder.
    /// Simple but not recommended for large projects (loads everything at startup).
    /// </summary>
    public class ResourcesUILoader : IUILoader
    {
        private const string UI_RESOURCES_PATH = "UI/";

        public Task<T> LoadAsync<T>(string key, CancellationToken cancellationToken = default) where T : Component
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new System.ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            var fullPath = UI_RESOURCES_PATH + key;

            Debug.Log($"[ResourcesUILoader] Loading: {fullPath}");

            // Resources.Load is synchronous, but we return a Task for consistency with async loading
            var prefab = Resources.Load<GameObject>(fullPath);

            if (prefab == null)
            {
                throw new System.InvalidOperationException(
                    $"Failed to load UI prefab from Resources at path '{fullPath}'. " +
                    $"Ensure the prefab exists in 'Resources/UI/{key}.prefab'");
            }

            var component = prefab.GetComponent<T>();

            if (component == null)
            {
                throw new System.InvalidOperationException(
                    $"Loaded prefab '{fullPath}' does not have component of type '{typeof(T).Name}'.");
            }

            Debug.Log($"[ResourcesUILoader] Loaded successfully: {fullPath}");

            return Task.FromResult(component);
        }

        public void Release<T>(T instance) where T : Component
        {
            // Resources doesn't require explicit unloading for prefabs
            // Memory is managed by Unity
            Debug.Log($"[ResourcesUILoader] Release called for {instance?.GetType().Name} (no-op for Resources)");
        }
    }
}
