using System;
using System.Collections.Generic;
using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Pooling
{
    /// <summary>
    /// Custom generic object pool implementation for managing pooled instances.
    /// Provides configurable capacity, lifecycle callbacks, and efficient Stack-based storage.
    /// </summary>
    /// <typeparam name="T">The type of objects to pool (must be Component and implement IUIView).</typeparam>
    public class GenericObjectPool<T> where T : Component, IUIView
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnGet;
        private readonly Action<T> _actionOnRelease;
        private readonly Action<T> _actionOnDestroy;
        private readonly bool _collectionCheck;
        private readonly int _maxSize;
        private int _currentSize;

        /// <summary>
        /// Creates a new GenericObjectPool.
        /// </summary>
        /// <param name="createFunc">Function to create new instances.</param>
        /// <param name="actionOnGet">Action to perform when getting from pool (optional).</param>
        /// <param name="actionOnRelease">Action to perform when returning to pool (optional).</param>
        /// <param name="actionOnDestroy">Action to perform when destroying an instance (optional).</param>
        /// <param name="collectionCheck">Enable collection checks for debugging (optional).</param>
        /// <param name="defaultCapacity">Pre-allocate capacity (optional, unused but kept for API compatibility).</param>
        /// <param name="maxSize">Maximum pool size, excess instances are destroyed (optional).</param>
        public GenericObjectPool(
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = false,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _collectionCheck = collectionCheck;
            _maxSize = maxSize;
            _currentSize = 0;
        }

        /// <summary>
        /// Gets an instance from the pool or creates a new one if the pool is empty.
        /// </summary>
        public T Get()
        {
            T instance;

            if (_pool.Count > 0)
            {
                instance = _pool.Pop();

                if (_collectionCheck && instance == null)
                {
                    throw new InvalidOperationException("Pool contained null instance. This indicates a bug in the pool management.");
                }
            }
            else
            {
                instance = _createFunc();
                _currentSize++;
            }

            _actionOnGet?.Invoke(instance);
            return instance;
        }

        /// <summary>
        /// Returns an instance to the pool for reuse.
        /// </summary>
        public void Release(T instance)
        {
            if (_collectionCheck && _pool.Contains(instance))
            {
                throw new InvalidOperationException("Attempting to release an instance that is already in the pool. This indicates a double-release bug.");
            }

            _actionOnRelease?.Invoke(instance);

            if (_pool.Count < _maxSize)
            {
                _pool.Push(instance);
            }
            else
            {
                // Pool is full, destroy the excess instance
                _actionOnDestroy?.Invoke(instance);
                _currentSize--;
            }
        }

        /// <summary>
        /// Clears the pool and destroys all pooled instances.
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var instance = _pool.Pop();
                _actionOnDestroy?.Invoke(instance);
            }

            _currentSize = 0;
        }

        /// <summary>
        /// Gets the current number of instances in the pool (available for reuse).
        /// </summary>
        public int CountInactive => _pool.Count;

        /// <summary>
        /// Gets the total number of instances created (in use + in pool).
        /// </summary>
        public int CountAll => _currentSize;
    }
}
