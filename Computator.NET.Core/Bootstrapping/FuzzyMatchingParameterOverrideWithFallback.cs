using System;
using System.Collections.Generic;
using System.Linq;
using Guards;
using Unity.Builder;
using Unity.Builder.Operation;
using Unity.Injection;
using Unity.Policy;
using Unity.Resolution;

namespace Computator.NET.Core.Bootstrapping
{
    /// <summary>
    /// A <see cref="ResolverOverride"/> class that lets you
    /// override a named parameter passed to a constructor.
    /// </summary>
    public class FuzzyMatchingParameterOverrideWithFallback<T> : ResolverOverride
        where T : class
    {
        private readonly InjectionParameterValue _fallbackValue;

        private readonly Dictionary<string, InjectionParameterValue> _injectionParameters;

        public FuzzyMatchingParameterOverrideWithFallback(Dictionary<string,T> parameters, T fallbackValue=null)
        {
            if(fallbackValue!=null)
                _fallbackValue = InjectionParameterValue.ToParameter(fallbackValue);

            _injectionParameters = new Dictionary<string, InjectionParameterValue>();
            foreach (var parameter in parameters)
            {
                _injectionParameters.Add(parameter.Key.ToLowerInvariant(), InjectionParameterValue.ToParameter(parameter.Value));
            }
        }

        public override IResolverPolicy GetResolver(IBuilderContext context, Type dependencyType)
        {
            Guard.ArgumentNotNull(context, nameof(context));

            if (context.CurrentOperation is ConstructorArgumentResolveOperation currentOperation &&
                //(typeof(T)==dependencyType || typeof(T).GetInterfaces().Contains(dependencyType))
                dependencyType.IsAssignableFrom(typeof(T)))
            {
                var parameter =
                    _injectionParameters.FirstOrDefault(
                        kv => currentOperation.ParameterName.ToLowerInvariant().Contains(kv.Key));//TODO: later introduce real fuzzy matching

                return parameter.Key==null ? this._fallbackValue?.GetResolverPolicy(dependencyType) : parameter.Value.GetResolverPolicy(dependencyType);
            }

            return null;
        }
    }
}