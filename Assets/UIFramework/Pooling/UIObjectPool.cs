using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UIFramework.Core;

namespace UIFramework.Pooling
{
    /// <summary>
    /// Wrapper around Unity's ObjectPool for UI elements.
    /// Provides async loading integration and IPoolable callback support.
    /// Uses Unity's optimized pool implementation internally.
    /// </summary>
    public class UIObjectPool : IUIObjectPool
    {
        private class PoolWrapper<T> where T : Component
        {
            public ObjectPool<T> Pool { get; set; }
            public GameObject Prefab { get; set; }
            public Transform PoolRoot { get; set; }
        }

        private readonly Dictionary<Type, object> _pools = new Dictionary<Type, object>();
        private readonly IUILoader _uiLoader;
        private readonly Transform _poolRootTransform;
        private readonly int _defaultCapacity;
        private readonly int _maxSize;
        private readonly bool _collectionCheck;

        /// <summary>
        /// Creates a new UIObjectPool using Unity's ObjectPool internally.
        /// </summary>
        /// <param name="uiLoader">The UI loader for loading prefabs.</param>
        /// <param name="config">Framework configuration (optional).</param>
        public UIObjectPool(IUILoader uiLoader, UIFramework.DependencyInjection.UIFrameworkConfig config = null)
        {
            _uiLoader = uiLoader ?? throw new ArgumentNullException(nameof(uiLoader));

            _defaultCapacity = config?.DefaultPoolSize ?? 10;
            _maxSize = config?.MaxCachedViews ?? 100;
            _collectionCheck = config?.VerboseLogging ?? false; // Enable checks in debug mode

            // Create root transform for pooled objects
            var poolRoot = new GameObject("[UI Object Pool]");
            GameObject.DontDestroyOnLoad(poolRoot);
            _poolRootTransform = poolRoot.transform;

            Debug.Log($"[UIObjectPool] Initialized with Unity's ObjectPool (Capacity: {_defaultCapacity}, MaxSize: {_maxSize})");
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : Component
        {
            var poolWrapper = await GetOrCreatePoolAsync<T>(cancellationToken);

            var instance = poolWrapper.Pool.Get();

            Debug.Log($"[UIObjectPool] Got instance of {typeof(T).Name} from pool");

            return instance;
        }

        public void Return<T>(T instance) where T : Component
        {
            if (instance == null)
            {
                Debug.LogWarning("[UIObjectPool] Attempted to return null instance.");
                return;
            }

            var type = typeof(T);

            if (!_pools.TryGetValue(type, out var poolObj))
            {
                Debug.LogWarning($"[UIObjectPool] No pool found for type {type.Name}. Destroying instance.");
                GameObject.Destroy(instance.gameObject);
                return;
            }

            var poolWrapper = (PoolWrapper<T>)poolObj;
            poolWrapper.Pool.Release(instance);

            Debug.Log($"[UIObjectPool] Returned instance of {type.Name} to pool");
        }

        public void Warmup<T>(int count) where T : Component
        {
            Debug.Log($"[UIObjectPool] Warming up pool for {typeof(T).Name} with {count} instances...");

            // Start warmup asynchronously
            _ = WarmupAsync<T>(count);
        }

        private async Task WarmupAsync<T>(int count) where T : Component
        {
            var poolWrapper = await GetOrCreatePoolAsync<T>();

            var instances = new List<T>(count);

            // Get instances from pool (which creates them)
            for (int i = 0; i < count; i++)
            {
                instances.Add(poolWrapper.Pool.Get());
            }

            // Return them all back
            foreach (var instance in instances)
            {
                poolWrapper.Pool.Release(instance);
            }

            Debug.Log($"[UIObjectPool] Warmup complete for {typeof(T).Name}: {count} instances pre-created.");
        }

        public void Clear()
        {
            Debug.Log($"[UIObjectPool] Clearing all pools ({_pools.Count})...");

            foreach (var kvp in _pools)
            {
                var type = kvp.Key;
                var poolObj = kvp.Value;

                // Use reflection to call Clear on the generic pool
                var clearMethod = poolObj.GetType().GetMethod("Clear");
                clearMethod?.Invoke(poolObj, null);

                // Destroy pool root
                var poolRootProperty = poolObj.GetType().GetProperty("PoolRoot");
                var poolRoot = poolRootProperty?.GetValue(poolObj) as Transform;
                if (poolRoot != null)
                {
                    GameObject.Destroy(poolRoot.gameObject);
                }

                Debug.Log($"[UIObjectPool] Cleared pool for {type.Name}");
            }

            _pools.Clear();
            Debug.Log("[UIObjectPool] All pools cleared.");
        }

        /// <summary>
        /// Gets or creates a Unity ObjectPool for the specified type.
        /// </summary>
        private async Task<PoolWrapper<T>> GetOrCreatePoolAsync<T>(CancellationToken cancellationToken = default)
            where T : Component
        {
            var type = typeof(T);

            if (_pools.TryGetValue(type, out var existing))
            {
                return (PoolWrapper<T>)existing;
            }

            Debug.Log($"[UIObjectPool] Creating new Unity ObjectPool for type: {type.Name}");

            // Load the prefab
            var prefab = await _uiLoader.LoadAsync<T>(type.Name, cancellationToken);

            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Failed to load prefab for pooled type '{type.Name}'. " +
                    "Ensure the prefab exists and is loadable via the configured IUILoader.");
            }

            // Create pool root
            var poolRoot = new GameObject($"[Pool] {type.Name}");
            poolRoot.transform.SetParent(_poolRootTransform);

            // Create Unity's ObjectPool with callbacks
            var pool = new ObjectPool<T>(
                createFunc: () => CreateInstance<T>(prefab.gameObject, poolRoot.transform),
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReturnToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: _collectionCheck,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxSize
            );

            var wrapper = new PoolWrapper<T>
            {
                Pool = pool,
                Prefab = prefab.gameObject,
                PoolRoot = poolRoot.transform
            };

            _pools[type] = wrapper;

            Debug.Log($"[UIObjectPool] Unity ObjectPool created for {type.Name} " +
                     $"(DefaultCapacity: {_defaultCapacity}, MaxSize: {_maxSize})");

            return wrapper;
        }

        #region Unity ObjectPool Callbacks

        /// <summary>
        /// Called when creating a new instance for the pool.
        /// </summary>
        private T CreateInstance<T>(GameObject prefab, Transform parent) where T : Component
        {
            var instance = GameObject.Instantiate(prefab, parent).GetComponent<T>();

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Prefab does not have component of type {typeof(T).Name}");
            }

            instance.gameObject.SetActive(false); // Start inactive

            Debug.Log($"[UIObjectPool] Created new instance: {typeof(T).Name}");

            return instance;
        }

        /// <summary>
        /// Called when getting an instance from the pool.
        /// </summary>
        private void OnGetFromPool<T>(T instance) where T : Component
        {
            if (instance == null) return;

            instance.gameObject.SetActive(true);

            // Notify IPoolable implementers
            if (instance is IPoolable poolable)
            {
                poolable.OnSpawnedFromPool();
            }
        }

        /// <summary>
        /// Called when returning an instance to the pool.
        /// </summary>
        private void OnReturnToPool<T>(T instance) where T : Component
        {
            if (instance == null) return;

            // Notify IPoolable implementers
            if (instance is IPoolable poolable)
            {
                poolable.OnReturnedToPool();
            }

            // Reset transform
            if (instance.transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
            }
            else
            {
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }

            instance.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when destroying an instance (pool exceeded max size).
        /// </summary>
        private void OnDestroyPoolObject<T>(T instance) where T : Component
        {
            if (instance != null && instance.gameObject != null)
            {
                Debug.Log($"[UIObjectPool] Destroying excess instance: {typeof(T).Name}");
                GameObject.Destroy(instance.gameObject);
            }
        }

        #endregion

        /// <summary>
        /// Gets the count of available and total instances in a pool (for debugging).
        /// </summary>
        public string GetPoolStats<T>() where T : Component
        {
            var type = typeof(T);

            if (!_pools.TryGetValue(type, out var poolObj))
            {
                return $"Pool<{type.Name}>: Not created yet";
            }

            var wrapper = (PoolWrapper<T>)poolObj;
            var pool = wrapper.Pool;

            return $"Pool<{type.Name}>: Available={pool.CountInactive}, Total={pool.CountAll}";
        }
    }
}
