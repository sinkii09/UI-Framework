using System;
using System.Collections.Generic;

namespace UIFramework.MVVM
{
    /// <summary>
    /// Manages multiple IDisposable objects and disposes them all at once.
    /// Essential for automatic cleanup of subscriptions in ViewModels.
    /// </summary>
    public class CompositeDisposable : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private bool _isDisposed = false;

        /// <summary>
        /// Adds a disposable to this composite.
        /// </summary>
        /// <param name="disposable">The disposable to add.</param>
        public void Add(IDisposable disposable)
        {
            if (_isDisposed)
            {
                disposable?.Dispose();
                return;
            }

            if (disposable != null)
            {
                _disposables.Add(disposable);
            }
        }

        /// <summary>
        /// Removes a disposable from this composite without disposing it.
        /// </summary>
        /// <param name="disposable">The disposable to remove.</param>
        /// <returns>True if the disposable was found and removed.</returns>
        public bool Remove(IDisposable disposable)
        {
            return _disposables.Remove(disposable);
        }

        /// <summary>
        /// Clears all disposables without disposing them.
        /// </summary>
        public void Clear()
        {
            _disposables.Clear();
        }

        /// <summary>
        /// Gets the number of disposables in this composite.
        /// </summary>
        public int Count => _disposables.Count;

        /// <summary>
        /// Disposes all contained disposables and clears the collection.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable?.Dispose();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error disposing object: {ex}");
                }
            }

            _disposables.Clear();
        }
    }

    /// <summary>
    /// Extension methods for IDisposable to work with CompositeDisposable.
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Adds this disposable to a CompositeDisposable for automatic cleanup.
        /// </summary>
        /// <param name="disposable">The disposable to add.</param>
        /// <param name="compositeDisposable">The composite to add to.</param>
        /// <returns>The original disposable (for method chaining).</returns>
        public static T AddTo<T>(this T disposable, CompositeDisposable compositeDisposable) where T : IDisposable
        {
            if (compositeDisposable != null)
            {
                compositeDisposable.Add(disposable);
            }
            return disposable;
        }
    }
}
