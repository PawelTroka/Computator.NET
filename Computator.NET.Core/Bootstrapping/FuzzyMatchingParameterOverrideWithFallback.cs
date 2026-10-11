using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Resolution;

namespace Computator.NET.Core.Bootstrapping
{
    /// <summary>
    /// Constructor-only, case-insensitive substring overrides for editor roles.
    /// Unity 5.11 matches ParameterInfo instead of exposing the old builder operation.
    /// Each override owns an immutable value: concurrent resolutions cannot exchange
    /// the scripting editor and custom-functions editor through shared match state.
    /// </summary>
    public sealed class FuzzyMatchingParameterOverrideWithFallback<T> where T : class
    {
        private readonly ResolverOverride[] _overrides;

        public FuzzyMatchingParameterOverrideWithFallback(Dictionary<string, T> parameters, T fallbackValue = null)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (parameters.Keys.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Parameter-name fragments must not be empty.", nameof(parameters));

            var entries = parameters.ToArray();
            var names = entries.Select(entry => entry.Key).ToArray();
            var overrides = new List<ResolverOverride>();
            for (var index = 0; index < entries.Length; index++)
                overrides.Add(new ConstructorOverride(names, index, entries[index].Value));
            if (fallbackValue != null)
                overrides.Add(new ConstructorOverride(names, names.Length, fallbackValue));
            _overrides = overrides.ToArray();
        }

        public ResolverOverride[] Overrides => (ResolverOverride[])_overrides.Clone();

        private sealed class ConstructorOverride : ResolverOverride, IEquatable<ParameterInfo>, IResolve
        {
            private readonly string[] _names;
            private readonly int _index;
            private readonly T _value;

            public ConstructorOverride(string[] names, int index, T value)
                : base(null, typeof(T), index < names.Length ? names[index] : null)
            {
                _names = names;
                _index = index;
                _value = value;
            }

            public bool Equals(ParameterInfo parameter)
            {
                if (parameter == null || !(parameter.Member is ConstructorInfo) ||
                    !parameter.ParameterType.IsAssignableFrom(typeof(T)) ||
                    (Target != null && parameter.Member.DeclaringType != Target))
                    return false;

                // Preserve the original first-match rule, independently of the order
                // in which Unity visits the overrides. A fallback never masks a match.
                var match = Array.FindIndex(_names, name =>
                    parameter.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
                return (match < 0 ? _names.Length : match) == _index;
            }

            public override bool Equals(object other) => other is ParameterInfo parameter
                ? Equals(parameter)
                : ReferenceEquals(this, other);

            public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

            public object Resolve<TContext>(ref TContext context) where TContext : IResolveContext => _value;
        }
    }
}
