using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
{
    [TestFixture, NonParallelizable]
    public class PrecompiledBuiltinNativeTests
    {
        private sealed class LegacyEvaluator : ExpressionsEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }
        private static Delegate Program(BaseFunction function) => (Delegate)typeof(BaseFunction)
            .GetField("_function", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(function);
        private static bool Shareable(Type type) => (bool)typeof(NativeCompiler).Assembly
            .GetType("Computator.NET.Core.Compilation.BuiltinFacadePolicy").GetMethod("HasShareableState", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { type });
        private sealed class MutableFixture { public static int Value; }
        private sealed class ReadonlyMutableFixture { public static readonly int[] Value = new int[1]; }

        [OneTimeSetUp] public void Initialize() => GSLInitializer.Initialize();

        [Test]
        public void OnlyShareableMathematicalStorageMayUseTheFacade()
        {
            NUnit.Framework.Assert.That(Shareable(typeof(PrecompiledExpressionSurface)), Is.True);
            NUnit.Framework.Assert.That(Shareable(typeof(PrecompiledScriptMathSurface)), Is.True);
            NUnit.Framework.Assert.That(Shareable(typeof(MutableFixture)), Is.False);
            NUnit.Framework.Assert.That(Shareable(typeof(ReadonlyMutableFixture)), Is.False);
        }

        [Test]
        public void NativeEntryPointsCallingConventionsAndSignaturesMatchTheEmittedSurface()
        {
            var legacy = new LegacyEvaluator().Evaluate("x", "", CalculationsMode.Real);
            var original = Program(legacy).Method.DeclaringType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttribute<DllImportAttribute>() != null).ToArray();
            var precompiled = typeof(PrecompiledExpressionSurface).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttribute<DllImportAttribute>() != null).ToArray();
            NUnit.Framework.Assert.That(original.Length, Is.GreaterThan(0));
            NUnit.Framework.Assert.That(precompiled.Length, Is.EqualTo(original.Length));
            foreach (var expected in original)
            {
                var actual = precompiled.Single(method => method.Name == expected.Name);
                var a = actual.GetCustomAttribute<DllImportAttribute>(); var e = expected.GetCustomAttribute<DllImportAttribute>();
                NUnit.Framework.Assert.That(a.Value, Is.EqualTo(e.Value));
                NUnit.Framework.Assert.That(a.EntryPoint, Is.EqualTo(e.EntryPoint));
                NUnit.Framework.Assert.That(a.CallingConvention, Is.EqualTo(e.CallingConvention));
                NUnit.Framework.Assert.That(a.CharSet, Is.EqualTo(e.CharSet));
                NUnit.Framework.Assert.That(a.ExactSpelling, Is.EqualTo(e.ExactSpelling));
                NUnit.Framework.Assert.That(a.SetLastError, Is.EqualTo(e.SetLastError));
                NUnit.Framework.Assert.That(actual.ReturnType.FullName, Is.EqualTo(expected.ReturnType.FullName));
                NUnit.Framework.Assert.That(actual.GetParameters().Select(p => p.ParameterType.FullName),
                    Is.EqualTo(expected.GetParameters().Select(p => p.ParameterType.FullName)));
            }
        }

        [Test]
        public void NativeGammaHasAnIndependentOracleOnTheFastPath()
        {
            var function = new ExpressionsEvaluator().Evaluate("gammaP(1,x)", "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(Program(function).Method.DeclaringType.BaseType.IsGenericType, Is.True);
            Func<double, double> evaluate = function;
            foreach (double x in new[] { 0.25, 0.5, 1, 2, 4 })
                NUnit.Framework.Assert.That(evaluate(x), Is.EqualTo(1-Math.Exp(-x)).Within(2e-13));
        }

        [TestCase(false), TestCase(true)]
        public void NativeErrorPolicyIsNotWeakened(bool throwOnError)
        {
            bool previous = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = throwOnError;
                Func<double,double> actual = new ExpressionsEvaluator().Evaluate("gammaP(x,1)", "", CalculationsMode.Real);
                Func<double,double> expected = new LegacyEvaluator().Evaluate("gammaP(x,1)", "", CalculationsMode.Real);
                Exception actualError = null, expectedError = null;
                double a = 0, e = 0;
                try { a = actual(-1); } catch (Exception error) { actualError = error; }
                try { e = expected(-1); } catch (Exception error) { expectedError = error; }
                NUnit.Framework.Assert.That(actualError?.GetType(), Is.EqualTo(expectedError?.GetType()));
                if (actualError == null) NUnit.Framework.Assert.That(a, Is.EqualTo(e));
                NUnit.Framework.Assert.That(actual(1), Is.EqualTo(1-Math.Exp(-1)).Within(2e-13));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }
    }
}
