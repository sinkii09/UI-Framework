using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIFramework.Events
{
    /// <summary>
    /// Event bus for decoupled UI communication.
    /// Prevents nested action callbacks and provides type-safe event publishing/subscribing.
    /// </summary>
    public class UIEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();
        private readonly object _lock = new object();

        /// <summary>
        /// Subscribes to events of type TEvent.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="handler">The handler to invoke when the event is published.</param>
        /// <returns>A disposable handle to unsubscribe.</returns>
        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IUIEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(TEvent);

            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<Delegate>();
                }

                _subscribers[eventType].Add(handler);
            }

            return new UIFramework.MVVM.UnsubscribeHandle(() => Unsubscribe(eventType, handler));
        }

        /// <summary>
        /// Publishes an event to all subscribers.
        /// Errors in individual handlers are caught and logged without affecting other handlers.
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <param name="eventData">The event data to publish.</param>
        public void Publish<TEvent>(TEvent eventData) where TEvent : IUIEvent
        {
            var eventType = typeof(TEvent);
            List<Delegate> handlers;

            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                    return;

                // Create a copy to avoid modification during iteration
                handlers = _subscribers[eventType].ToList();
            }

            // Invoke handlers outside the lock to prevent deadlocks
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<TEvent>)handler)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[UIEventBus] Error in event handler for {eventType.Name}: {ex}");
                    // Continue processing other handlers
                }
            }
        }

        /// <summary>
        /// Unsubscribes a handler from events.
        /// </summary>
        private void Unsubscribe(Type eventType, Delegate handler)
        {
            lock (_lock)
            {
                if (_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType].Remove(handler);

                    // Clean up empty lists
                    if (_subscribers[eventType].Count == 0)
                    {
                        _subscribers.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all subscriptions (useful for testing or cleanup).
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _subscribers.Clear();
            }
        }

        /// <summary>
        /// Gets the number of subscribers for a specific event type (for debugging).
        /// </summary>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>The number of subscribers.</returns>
        public int GetSubscriberCount<TEvent>() where TEvent : IUIEvent
        {
            var eventType = typeof(TEvent);

            lock (_lock)
            {
                return _subscribers.ContainsKey(eventType) ? _subscribers[eventType].Count : 0;
            }
        }
    }
}
