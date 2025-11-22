using System;

namespace UIFramework.MVVM
{
    /// <summary>
    /// Read-only interface for reactive properties.
    /// Use this to expose properties without allowing external modification.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    public interface IReadOnlyReactiveProperty<T>
    {
        /// <summary>
        /// Gets the current value of the property.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Subscribes to value changes.
        /// </summary>
        /// <param name="onValueChanged">Callback invoked when the value changes.</param>
        /// <returns>A disposable handle to unsubscribe.</returns>
        IDisposable Subscribe(Action<T> onValueChanged);
    }
}
