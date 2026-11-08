using System;
using System.Collections.Generic;

namespace WheelOfFortune.Core
{
    /// <summary>
    /// Type-safe, global event bus for decoupled communication between layers.
    /// Core, UI, and other layers publish and subscribe without referencing each other.
    ///
    /// Usage (typed event):
    ///   EventBus.Subscribe&lt;StateChangedEvent&gt;(OnStateChanged);
    ///   EventBus.Unsubscribe&lt;StateChangedEvent&gt;(OnStateChanged);
    ///   EventBus.Publish(new StateChangedEvent(newState));
    ///
    /// Usage (parameterless event):
    ///   EventBus.Subscribe(OnBombHit);
    ///   EventBus.Unsubscribe(OnBombHit);
    ///   EventBus.Publish();
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> s_subscribers = new();

        /// <summary>Subscribes a handler to a typed event message.</summary>
        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (s_subscribers)
            {
                var type = typeof(T);
                s_subscribers[type] = Delegate.Combine(
                    s_subscribers.TryGetValue(type, out var existing) ? existing : null,
                    handler);
            }
        }

        /// <summary>Unsubscribes a previously registered typed handler.</summary>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            lock (s_subscribers)
            {
                var type = typeof(T);
                if (!s_subscribers.TryGetValue(type, out var existing)) return;
                var updated = Delegate.Remove(existing, handler);
                if (updated == null)
                {
                    s_subscribers.Remove(type);
                }
                else
                {
                    s_subscribers[type] = updated;
                }
            }
        }

        /// <summary>Publishes a typed event message to all subscribers.</summary>
        public static void Publish<T>(T message)
        {
            Action<T> handler;
            lock (s_subscribers)
            {
                if (!s_subscribers.TryGetValue(typeof(T), out var existing)) return;
                handler = existing as Action<T>;
            }
            handler?.Invoke(message);
        }

        /// <summary>Subscribes a handler to a parameterless event.</summary>
        public static void Subscribe(Action handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            Subscribe<Unit>(_ => handler());
        }

        /// <summary>Unsubscribes a previously registered parameterless handler.</summary>
        public static void Unsubscribe(Action handler)
        {
            if (handler == null) return;
            Unsubscribe<Unit>(_ => handler());
        }

        /// <summary>Publishes a parameterless event.</summary>
        public static void Publish()
        {
            Publish(default(Unit));
        }

        /// <summary>
        /// Unit type used internally to represent parameterless events on the bus.
        /// </summary>
        private struct Unit { }
    }
}
