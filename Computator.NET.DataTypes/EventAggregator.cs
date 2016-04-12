using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Computator.NET.Evaluation;

namespace Computator.NET.DataTypes
{
    public interface IApplicationEvent
    {
    }

    public class CalculationsModeChangedEvent : IApplicationEvent
    {
        public CalculationsModeChangedEvent(CalculationsMode mode)
        {
            CalculationsMode = mode;
        }

        public CalculationsMode CalculationsMode { get; private set; }
    }

    public interface IEventAggregator
    {
        void Publish<T>(T message) where T : IApplicationEvent;
        void Subscribe<T>(Action<T> action) where T : IApplicationEvent;
        void Unsubscribe<T>(Action<T> action) where T : IApplicationEvent;
    }

    public class EventAggregator : IEventAggregator
    {
        private readonly ConcurrentDictionary<Type, List<object>> subscriptions =
            new ConcurrentDictionary<Type, List<object>>();

        private EventAggregator()
        {
        }

        public static EventAggregator Instance { get; } = new EventAggregator();

        public void Publish<T>(T message) where T : IApplicationEvent
        {
            List<object> subscribers;

            if (subscriptions.TryGetValue(typeof (T), out subscribers))
            {
                foreach (var subscriber in subscribers.ToArray())
                {
                    ((Action<T>) subscriber)(message);
                }
            }
        }

        public void Subscribe<T>(Action<T> action) where T : IApplicationEvent
        {
            lock (this)
            {
                if (!subscriptions.ContainsKey(typeof (T)))
                    subscriptions.TryAdd(typeof (T), new List<object>());
            }
            subscriptions[typeof (T)].Add(action);
        }

        public void Unsubscribe<T>(Action<T> action) where T : IApplicationEvent
        {
            if (subscriptions.ContainsKey(typeof (T)))
                subscriptions[typeof (T)].Remove(action);
        }
    }
}