using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UIFramework.Core;
using UIFramework.Configuration;

namespace UIFramework.Pooling
{

    /// <summary>
    /// Custom object pool implementation for UI elements.
    /// Provides async loading integration and IPoolable callback support.
    /// Uses a custom Stack-based pool implementation for full control.
    /// </summary>
    public class UIObjectPool : IUIObjectPool
    {
        private interface IPoolWrapper
        {
            void ReleaseInstance(IUIView instance);
            void ClearPool();
            Transform PoolRoot { get; }
        }

        private class PoolWrapper<T> : IPoolWrapper where T : Component, IUIView
        {
            public GenericObjectPool<T> Pool { get; set; }
            public GameObject Prefab { get; set; }
            public Transform PoolRoot { get; set; }

            public void ReleaseInstance(IUIView instance)
            {
                Pool.Release((T)instance);
            }

            public void ClearPool()
            {
                Pool.Clear();
            }
        }

        private readonly Dictionary<Type, object> _pools = new();
        private readonly IUILoader _uiLoader;
        private readonly Transform _poolRootTransform;
        private readonly int _defaultCapacity;
        private readonly int _maxSize;
        private readonly bool _collectionCheck;

        /// <summary>
        /// Creates a new UIObjectPool using custom GenericObjectPool internally.
        /// </summary>
        /// <param name="uiLoader">The UI loader for loading prefabs.</param>
        /// <param name="config">Framework configuration (optional).</param>
        public UIObjectPool(IUILoader uiLoader, UIFrameworkConfig config = null)
        {
            _uiLoader = uiLoader ?? throw new ArgumentNullException(nameof(uiLoader));

            _defaultCapacity = config?.DefaultPoolSize ?? 10;
            _maxSize = config?.MaxCachedViews ?? 100;
            _collectionCheck = config?.VerboseLogging ?? false; // Enable checks in debug mode

            // Create root transform for pooled objects
            var poolRoot = new GameObject("[UI Object Pool]");
            GameObject.DontDestroyOnLoad(poolRoot);
            _poolRootTransform = poolRoot.transform;
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken = default) where T : Component, IUIView
        {
            var poolWrapper = await GetOrCreatePoolAsync<T>(cancellationToken);

            var instance = poolWrapper.Pool.Get();

            return instance;
        }

        public void Return<T>(T instance) where T : Component, IUIView
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
                UnityEngine.Object.Destroy(instance.gameObject);
                return;
            }

            var poolWrapper = (PoolWrapper<T>)poolObj;
            poolWrapper.Pool.Release(instance);
        }

        public void Return(IUIView instance)
        {
            if (instance == null)
            {
                Debug.LogWarning("[UIObjectPool] Attempted to return null instance.");
                return;
            }

            var concreteType = instance.GetType();

            if (!_pools.TryGetValue(concreteType, out var poolObj))
            {
                Debug.LogWarning($"[UIObjectPool] No pool found for type {concreteType.Name}. Destroying instance.");
                UnityEngine.Object.Destroy(instance.GameObject);
                return;
            }

            var poolWrapper = (IPoolWrapper)poolObj;
            poolWrapper.ReleaseInstance(instance);
        }

        public void Clear()
        {
            foreach (var kvp in _pools)
            {
                var type = kvp.Key;
                var poolWrapper = (IPoolWrapper)kvp.Value;

                // Clear the pool
                poolWrapper.ClearPool();

                // Destroy pool root
                if (poolWrapper.PoolRoot != null)
                {
                    UnityEngine.Object.Destroy(poolWrapper.PoolRoot.gameObject);
                }
            }

            _pools.Clear();
        }

        /// <summary>
        /// Gets or creates a GenericObjectPool for the specified type.
        /// </summary>
        private async Task<PoolWrapper<T>> GetOrCreatePoolAsync<T>(CancellationToken cancellationToken = default)
            where T : Component, IUIView
        {
            var type = typeof(T);

            if (_pools.TryGetValue(type, out var existing))
            {
                return (PoolWrapper<T>)existing;
            }

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

            // Create custom GenericObjectPool with callbacks
            var pool = new GenericObjectPool<T>(
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
            return wrapper;
        }

        #region GenericObjectPool Callbacks

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

            return instance;
        }

        /// <summary>
        /// Called when getting an instance from the pool.
        /// </summary>
        private void OnGetFromPool<T>(T instance) where T : Component, IUIView
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
        private void OnReturnToPool<T>(T instance) where T : Component, IUIView
        {
            if (instance == null) return;

            // Notify IPoolable implementers
            if (instance is IPoolable poolable)
            {
                poolable.OnReturnedToPool();
            }

            // Reset transform
            if (instance.Transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
            }
            else
            {
                instance.Transform.localPosition = Vector3.zero;
                instance.Transform.localRotation = Quaternion.identity;
                instance.Transform.localScale = Vector3.one;
            }

            instance.GameObject.SetActive(false);
        }

        /// <summary>
        /// Called when destroying an instance (pool exceeded max size).
        /// </summary>
        private void OnDestroyPoolObject<T>(T instance) where T : Component, IUIView
        {
            if (instance != null && instance.GameObject != null)
            {
                GameObject.Destroy(instance.GameObject);
            }
        }

        #endregion
    }
}
