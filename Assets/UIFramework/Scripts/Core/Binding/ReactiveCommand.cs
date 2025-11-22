using System;

namespace UIFramework.MVVM
{
    /// <summary>
    /// Represents a command that can be invoked, typically bound to UI buttons.
    /// Supports CanExecute logic for enabling/disabling UI elements.
    /// </summary>
    public class ReactiveCommand : IDisposable
    {
        private event Action _onExecute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Reactive property that indicates whether the command can be executed.
        /// Bind this to button.interactable or similar properties.
        /// </summary>
        public IReadOnlyReactiveProperty<bool> CanExecute { get; }

        /// <summary>
        /// Creates a new ReactiveCommand that can always execute.
        /// </summary>
        public ReactiveCommand()
        {
            _canExecute = () => true;
            CanExecute = new ReactiveProperty<bool>(true);
        }

        /// <summary>
        /// Creates a new ReactiveCommand with CanExecute logic.
        /// </summary>
        /// <param name="canExecute">Function that determines if the command can execute.</param>
        public ReactiveCommand(Func<bool> canExecute)
        {
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            CanExecute = new ReactiveProperty<bool>(_canExecute());
        }

        /// <summary>
        /// Creates a new ReactiveCommand with reactive CanExecute based on a property.
        /// </summary>
        /// <param name="canExecuteProperty">Reactive property that determines if the command can execute.</param>
        public ReactiveCommand(IReadOnlyReactiveProperty<bool> canExecuteProperty)
        {
            if (canExecuteProperty == null)
                throw new ArgumentNullException(nameof(canExecuteProperty));

            _canExecute = () => canExecuteProperty.Value;
            CanExecute = canExecuteProperty;
        }

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
        /// Executes the command if CanExecute returns true.
        /// </summary>
        public void Execute()
        {
            if (_canExecute())
            {
                _onExecute?.Invoke();
            }
        }

        /// <summary>
        /// Notifies that CanExecute has changed. Call this when conditions change.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (CanExecute is ReactiveProperty<bool> reactiveCanExecute)
            {
                reactiveCanExecute.Value = _canExecute();
            }
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
    /// Supports CanExecute logic for enabling/disabling UI elements.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class ReactiveCommand<T> : IDisposable
    {
        private event Action<T> _onExecute;
        private readonly Func<T, bool> _canExecute;

        /// <summary>
        /// Reactive property that indicates whether the command can be executed.
        /// Bind this to button.interactable or similar properties.
        /// </summary>
        public IReadOnlyReactiveProperty<bool> CanExecute { get; }

        /// <summary>
        /// Creates a new ReactiveCommand that can always execute.
        /// </summary>
        public ReactiveCommand()
        {
            _canExecute = _ => true;
            CanExecute = new ReactiveProperty<bool>(true);
        }

        /// <summary>
        /// Creates a new ReactiveCommand with CanExecute logic.
        /// </summary>
        /// <param name="canExecute">Function that determines if the command can execute based on parameter.</param>
        public ReactiveCommand(Func<T, bool> canExecute)
        {
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            CanExecute = new ReactiveProperty<bool>(true); // Default to true, update with RaiseCanExecuteChanged
        }

        /// <summary>
        /// Creates a new ReactiveCommand with reactive CanExecute based on a property.
        /// </summary>
        /// <param name="canExecuteProperty">Reactive property that determines if the command can execute.</param>
        public ReactiveCommand(IReadOnlyReactiveProperty<bool> canExecuteProperty)
        {
            if (canExecuteProperty == null)
                throw new ArgumentNullException(nameof(canExecuteProperty));

            _canExecute = _ => canExecuteProperty.Value;
            CanExecute = canExecuteProperty;
        }

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
        /// Executes the command with the specified parameter if CanExecute returns true.
        /// </summary>
        /// <param name="parameter">The parameter to pass to handlers.</param>
        public void Execute(T parameter)
        {
            if (_canExecute(parameter))
            {
                _onExecute?.Invoke(parameter);
            }
        }

        /// <summary>
        /// Notifies that CanExecute has changed. Call this when conditions change.
        /// </summary>
        public void RaiseCanExecuteChanged(T parameter)
        {
            if (CanExecute is ReactiveProperty<bool> reactiveCanExecute)
            {
                reactiveCanExecute.Value = _canExecute(parameter);
            }
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
