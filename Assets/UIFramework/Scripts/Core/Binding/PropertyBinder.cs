using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIFramework.MVVM
{
    /// <summary>
    /// Helper component for binding reactive properties to UI elements.
    /// Automatically manages subscriptions and cleans up on destroy.
    /// </summary>
    public class PropertyBinder : MonoBehaviour
    {
        private readonly List<IDisposable> _bindings = new List<IDisposable>();

        /// <summary>
        /// Binds a reactive property to an update action.
        /// The action is immediately invoked with the current value.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="property">The reactive property to bind.</param>
        /// <param name="updateAction">The action to invoke when the value changes.</param>
        /// <returns>The subscription handle.</returns>
        public IDisposable Bind<T>(IReadOnlyReactiveProperty<T> property, Action<T> updateAction)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (updateAction == null)
                throw new ArgumentNullException(nameof(updateAction));

            var subscription = property.Subscribe(updateAction);
            _bindings.Add(subscription);
            return subscription;
        }

        /// <summary>
        /// Binds a reactive command to a button.
        /// </summary>
        /// <param name="command">The command to bind.</param>
        /// <param name="button">The button to bind to.</param>
        /// <returns>The subscription handle.</returns>
        public IDisposable Bind(ReactiveCommand command, Button button)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (button == null)
                throw new ArgumentNullException(nameof(button));

            var subscription = command.Subscribe(() => { /* Command executed via button */ });
            button.onClick.AddListener(() => command.Execute());
            _bindings.Add(subscription);
            return subscription;
        }

        /// <summary>
        /// Binds a reactive command with parameter to a button.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="command">The command to bind.</param>
        /// <param name="button">The button to bind to.</param>
        /// <param name="parameter">The parameter to pass when clicked.</param>
        /// <returns>The subscription handle.</returns>
        public IDisposable Bind<T>(ReactiveCommand<T> command, Button button, T parameter)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (button == null)
                throw new ArgumentNullException(nameof(button));

            var subscription = command.Subscribe(_ => { /* Command executed via button */ });
            button.onClick.AddListener(() => command.Execute(parameter));
            _bindings.Add(subscription);
            return subscription;
        }

        /// <summary>
        /// Manually unbinds all bindings.
        /// </summary>
        public void UnbindAll()
        {
            foreach (var binding in _bindings)
            {
                try
                {
                    binding?.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error disposing binding: {ex}");
                }
            }
            _bindings.Clear();
        }

        private void OnDestroy()
        {
            UnbindAll();
        }
    }
}
