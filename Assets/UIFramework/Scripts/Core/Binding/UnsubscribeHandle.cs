using System;

namespace UIFramework.MVVM
{
    /// <summary>
    /// Disposable handle that unsubscribes from a subscription when disposed.
    /// Enables automatic cleanup using 'using' statements or CompositeDisposable.
    /// </summary>
    public class UnsubscribeHandle : IDisposable
    {
        private Action _unsubscribeAction;

        /// <summary>
        /// Creates a new unsubscribe handle.
        /// </summary>
        /// <param name="unsubscribeAction">The action to invoke on disposal.</param>
        public UnsubscribeHandle(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction ?? throw new ArgumentNullException(nameof(unsubscribeAction));
        }

        /// <summary>
        /// Invokes the unsubscribe action and clears the reference.
        /// </summary>
        public void Dispose()
        {
            _unsubscribeAction?.Invoke();
            _unsubscribeAction = null;
        }
    }
}
