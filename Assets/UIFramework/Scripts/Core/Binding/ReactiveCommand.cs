using System;

namespace UIFramework.MVVM
{
    /// <summary>
    /// Represents a command that can be invoked, typically bound to UI buttons.
    /// Supports subscription for decoupled command handling.
    /// </summary>
    public class ReactiveCommand : IDisposable
    {
        private event Action _onExecute;

        /// <summary>
        /// Subscribes a handler to be invoked when the command executes.
        /// </summary>
        /// <param name="handler">The handler to invoke.</param>
        /// <returns>A disposable handle to unsubscribe.</returns>
        public IDisposable Subscribe(Action handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _onExecute += handler;
            return new UnsubscribeHandle(() => _onExecute -= handler);
        }

        /// <summary>
        /// Executes the command, invoking all subscribed handlers.
        /// </summary>
        public void Execute()
        {
            _onExecute?.Invoke();
        }

        /// <summary>
        /// Disposes the command and clears all subscriptions.
        /// </summary>
        public void Dispose()
        {
            _onExecute = null;
        }
    }

    /// <summary>
    /// Represents a command with a parameter, typically bound to UI elements.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class ReactiveCommand<T> : IDisposable
    {
        private event Action<T> _onExecute;

        /// <summary>
        /// Subscribes a handler to be invoked when the command executes.
        /// </summary>
        /// <param name="handler">The handler to invoke.</param>
        /// <returns>A disposable handle to unsubscribe.</returns>
        public IDisposable Subscribe(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _onExecute += handler;
            return new UnsubscribeHandle(() => _onExecute -= handler);
        }

        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to handlers.</param>
        public void Execute(T parameter)
        {
            _onExecute?.Invoke(parameter);
        }

        /// <summary>
        /// Disposes the command and clears all subscriptions.
        /// </summary>
        public void Dispose()
        {
            _onExecute = null;
        }
    }
}
