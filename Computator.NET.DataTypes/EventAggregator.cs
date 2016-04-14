using System;
using System.CodeDom.Compiler;
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

    public class ChangeViewEvent : IApplicationEvent
    {
        public ChangeViewEvent(ViewName view)
        {
            View = view;
        }

        public ViewName View { get; private set; }
    }

    public class NoErrorsInCustomFunctionsEvent : IApplicationEvent { }

    public class ErrorsInCustomFunctionsEvent : IApplicationEvent
    {
        public ErrorsInCustomFunctionsEvent(IEnumerable<CompilerError> errors)
        {
            Errors = errors;
        }

        public IEnumerable<CompilerError> Errors { get; private set; } 
    }

    public enum ViewName
    {
        Charting,Calculations,NumericalCalculations,SymbolicCalculations,Scripting,CustomFunctions
    }

    public interface IEventAggregator
    {
        void Publish<T>(T message) where T : IApplicationEvent;
        void Subscribe<T>(Action<T> action) where T : IApplicationEvent;
        void Unsubscribe<T>(Action<T> action) where T : IApplicationEvent;
    }

    public class EventAggregator : IEventAggregator
    {
        public static IEventAggregator Instance { get; } = new EventAggregator();

        private readonly ConcurrentDictionary<Type, List<object>> subscriptions = new ConcurrentDictionary<Type, List<object>>();

        public void Publish<T>(T message) where T : IApplicationEvent
        {
            List<object> subscribers;
            if (subscriptions.TryGetValue(typeof(T), out subscribers))
            {
                // To Array creates a copy in case someone unsubscribes in their own handler
                foreach (var subscriber in subscribers.ToArray())
                {
                    ((Action<T>)subscriber)(message);
                }
            }
        }

        public void Subscribe<T>(Action<T> action) where T : IApplicationEvent
        {
            var subscribers = subscriptions.GetOrAdd(typeof(T), t => new List<object>());
            lock (subscribers)
            {
                subscribers.Add(action);
            }
        }

        public void Unsubscribe<T>(Action<T> action) where T : IApplicationEvent
        {
            List<object> subscribers;
            if (subscriptions.TryGetValue(typeof(T), out subscribers))
            {
                lock (subscribers)
                {
                    subscribers.Remove(action);
                }
            }
        }

        public void Dispose()
        {
            subscriptions.Clear();
        }
    }
}