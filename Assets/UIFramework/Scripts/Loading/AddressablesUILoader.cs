#if UIFRAMEWORK_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UIFramework.Core;

namespace UIFramework.Loading
{
    /// <summary>
    /// UI loader that uses Unity's Addressables system.
    /// Provides async loading and proper memory management.
    /// Recommended for production projects.
    /// </summary>
    public class AddressablesUILoader : IUILoader
    {
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _loadedAssets
            = new Dictionary<string, AsyncOperationHandle<GameObject>>();

        private readonly Dictionary<Component, string> _instanceToKey
            = new Dictionary<Component, string>();

        public async Task<T> LoadAsync<T>(string key, CancellationToken cancellationToken = default) where T : Component
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            Debug.Log($"[AddressablesUILoader] Loading: {key}");

            // Check if already loaded
            if (_loadedAssets.TryGetValue(key, out var existingHandle))
            {
                if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    var existingPrefab = existingHandle.Result;
                    var existingComponent = existingPrefab.GetComponent<T>();

                    if (existingComponent != null)
                    {
                        Debug.Log($"[AddressablesUILoader] Using cached asset: {key}");
                        return existingComponent;
                    }
                }
                else
                {
                    // Handle was invalid, remove it
                    _loadedAssets.Remove(key);
                }
            }

            // Load new asset
            var handle = Addressables.LoadAssetAsync<GameObject>(key);

            try
            {
                // Wait for the operation to complete with cancellation support
                while (!handle.IsDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to load UI asset '{key}' from Addressables. " +
                        $"Error: {handle.OperationException?.Message ?? "Unknown error"}");
                }

                var prefab = handle.Result;

                if (prefab == null)
                {
                    throw new InvalidOperationException(
                        $"Loaded asset '{key}' is null.");
                }

                var component = prefab.GetComponent<T>();

                if (component == null)
                {
                    throw new InvalidOperationException(
                        $"Loaded prefab '{key}' does not have component of type '{typeof(T).Name}'.");
                }

                // Cache the handle
                _loadedAssets[key] = handle;

                Debug.Log($"[AddressablesUILoader] Loaded successfully: {key}");

                return component;
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[AddressablesUILoader] Load cancelled: {key}");

                // Release the handle if cancelled
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressablesUILoader] Error loading '{key}': {ex.Message}");

                // Release the handle on error
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                throw;
            }
        }

        public void Release<T>(T instance) where T : Component
        {
            if (instance == null)
            {
                Debug.LogWarning("[AddressablesUILoader] Cannot release null instance.");
                return;
            }

            // Try to find the key associated with this instance
            if (_instanceToKey.TryGetValue(instance, out var key))
            {
                _instanceToKey.Remove(instance);

                if (_loadedAssets.TryGetValue(key, out var handle))
                {
                    Debug.Log($"[AddressablesUILoader] Releasing: {key}");

                    Addressables.Release(handle);
                    _loadedAssets.Remove(key);
                }
            }
            else
            {
                Debug.LogWarning($"[AddressablesUILoader] Instance of type '{typeof(T).Name}' was not tracked. Cannot release.");
            }
        }

        /// <summary>
        /// Releases all loaded assets (call on cleanup/scene unload).
        /// </summary>
        public void ReleaseAll()
        {
            Debug.Log($"[AddressablesUILoader] Releasing all loaded assets ({_loadedAssets.Count})...");

            foreach (var handle in _loadedAssets.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            _loadedAssets.Clear();
            _instanceToKey.Clear();

            Debug.Log("[AddressablesUILoader] All assets released.");
        }
    }
}
#endif
