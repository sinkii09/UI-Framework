using System;
using System.Collections.Generic;

namespace UIFramework.MVVM
{
    /// <summary>
    /// Observable property that notifies subscribers when its value changes.
    /// Forms the foundation of MVVM data binding.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    public class ReactiveProperty<T> : IReadOnlyReactiveProperty<T>, IDisposable
    {
        private T _value;
        private event Action<T> _onValueChanged;

        /// <summary>
        /// Creates a new ReactiveProperty with an optional initial value.
        /// </summary>
        /// <param name="initialValue">The initial value of the property.</param>
        public ReactiveProperty(T initialValue = default)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Gets or sets the current value.
        /// Setting the value will notify all subscribers if the value changed.
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                // Only notify if the value actually changed
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    _onValueChanged?.Invoke(_value);
                }
            }
        }

        /// <summary>
        /// Subscribes to value changes.
        /// The callback is immediately invoked with the current value.
        /// </summary>
        /// <param name="onValueChanged">Callback invoked when the value changes.</param>
        /// <returns>A disposable handle to unsubscribe.</returns>
        public IDisposable Subscribe(Action<T> onValueChanged)
        {
            if (onValueChanged == null)
                throw new ArgumentNullException(nameof(onValueChanged));

            _onValueChanged += onValueChanged;

            // Immediately invoke with current value
            onValueChanged(_value);

            return new UnsubscribeHandle(() => _onValueChanged -= onValueChanged);
        }

        /// <summary>
        /// Forces notification of all subscribers even if the value hasn't changed.
        /// </summary>
        public void ForceNotify()
        {
            _onValueChanged?.Invoke(_value);
        }

        /// <summary>
        /// Disposes the property and clears all subscriptions.
        /// </summary>
        public void Dispose()
        {
            _onValueChanged = null;
            _value = default;
        }

        /// <summary>
        /// Implicit conversion to T for convenient value access.
        /// </summary>
        public static implicit operator T(ReactiveProperty<T> property)
        {
            return property.Value;
        }

        /// <summary>
        /// Returns the string representation of the current value.
        /// </summary>
        public override string ToString()
        {
            return _value?.ToString() ?? "null";
        }
    }
}
