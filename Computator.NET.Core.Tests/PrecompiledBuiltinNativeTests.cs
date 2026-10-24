using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Evaluation;
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
        private sealed class MutableFixture { public static int Value = 1; }
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
        public void NativeEntryPointsCallingConventionsAndLayoutsMatchTheEmittedSurface()
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
                NUnit.Framework.Assert.That(actual.GetCustomAttributesData().Select(a => a.ToString()),
                    Is.EqualTo(expected.GetCustomAttributesData().Select(a => a.ToString())), expected.Name);
                AssertAbi(actual.ReturnType, expected.ReturnType);
                var a = actual.GetParameters(); var e = expected.GetParameters();
                NUnit.Framework.Assert.That(a.Length, Is.EqualTo(e.Length));
                for (int i = 0; i < a.Length; i++)
                {
                    AssertAbi(a[i].ParameterType, e[i].ParameterType);
                    NUnit.Framework.Assert.That(a[i].Attributes, Is.EqualTo(e[i].Attributes));
                    NUnit.Framework.Assert.That(a[i].GetCustomAttributesData().Select(attribute => attribute.ToString()),
                        Is.EqualTo(e[i].GetCustomAttributesData().Select(attribute => attribute.ToString())));
                }
            }
        }

        private static void AssertAbi(Type actual, Type expected)
        {
            if (actual == expected) return;
            NUnit.Framework.Assert.That(actual.IsByRef, Is.EqualTo(expected.IsByRef));
            NUnit.Framework.Assert.That(actual.IsPointer, Is.EqualTo(expected.IsPointer));
            NUnit.Framework.Assert.That(actual.IsArray, Is.EqualTo(expected.IsArray));
            if (actual.HasElementType || expected.HasElementType)
            {
                NUnit.Framework.Assert.That(actual.HasElementType, Is.EqualTo(expected.HasElementType));
                if (actual.IsArray) NUnit.Framework.Assert.That(actual.GetArrayRank(), Is.EqualTo(expected.GetArrayRank()));
                AssertAbi(actual.GetElementType(), expected.GetElementType());
                return;
            }
            // Native structs move to a different managed owner. Their ABI is
            // field types, marshalling and byte offsets, not the owner's name.
            NUnit.Framework.Assert.That(actual.IsValueType && expected.IsValueType, Is.True);
            NUnit.Framework.Assert.That(actual.Name, Is.EqualTo(expected.Name));
            NUnit.Framework.Assert.That(actual.StructLayoutAttribute.Value, Is.EqualTo(expected.StructLayoutAttribute.Value));
            NUnit.Framework.Assert.That(actual.StructLayoutAttribute.Pack, Is.EqualTo(expected.StructLayoutAttribute.Pack));
            NUnit.Framework.Assert.That(actual.StructLayoutAttribute.Size, Is.EqualTo(expected.StructLayoutAttribute.Size));
            NUnit.Framework.Assert.That(actual.StructLayoutAttribute.CharSet, Is.EqualTo(expected.StructLayoutAttribute.CharSet));
            NUnit.Framework.Assert.That(Marshal.SizeOf(actual), Is.EqualTo(Marshal.SizeOf(expected)));
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var actualFields = actual.GetFields(flags).OrderBy(field => field.Name).ToArray();
            var expectedFields = expected.GetFields(flags).OrderBy(field => field.Name).ToArray();
            NUnit.Framework.Assert.That(actualFields.Length, Is.EqualTo(expectedFields.Length));
            for (int i = 0; i < actualFields.Length; i++)
            {
                NUnit.Framework.Assert.That(actualFields[i].Name, Is.EqualTo(expectedFields[i].Name));
                NUnit.Framework.Assert.That(Marshal.OffsetOf(actual, actualFields[i].Name), Is.EqualTo(Marshal.OffsetOf(expected, expectedFields[i].Name)));
                NUnit.Framework.Assert.That(actualFields[i].GetCustomAttributesData().Select(a => a.ToString()),
                    Is.EqualTo(expectedFields[i].GetCustomAttributesData().Select(a => a.ToString())));
                AssertAbi(actualFields[i].FieldType, expectedFields[i].FieldType);
            }
        }

        [Test]
        public void ReplaceableSeededRandomStateRemainsLocalToEachCompiledProgram()
        {
            var first = new ExpressionsEvaluator().Evaluate("rnd.NextDouble()", "", CalculationsMode.Real);
            var second = new ExpressionsEvaluator().Evaluate("rnd.NextDouble()", "", CalculationsMode.Real);
            var a = Program(first).Method.DeclaringType.BaseType;
            var b = Program(second).Method.DeclaringType.BaseType;
            NUnit.Framework.Assert.That(a.IsGenericType && b.IsGenericType, Is.True);
            var fieldA = a.GetField("rnd", BindingFlags.Public | BindingFlags.Static);
            var fieldB = b.GetField("rnd", BindingFlags.Public | BindingFlags.Static);
            fieldA.SetValue(null, new Random(191));
            fieldB.SetValue(null, new Random(191));
            NUnit.Framework.Assert.That(fieldA.GetValue(null), Is.Not.SameAs(fieldB.GetValue(null)));
            Func<double,double> fa = first, fb = second;
            var oracleA = new Random(191); var oracleB = new Random(191);
            NUnit.Framework.Assert.That(fa(0), Is.EqualTo(oracleA.NextDouble()));
            NUnit.Framework.Assert.That(fa(0), Is.EqualTo(oracleA.NextDouble()));
            NUnit.Framework.Assert.That(fb(0), Is.EqualTo(oracleB.NextDouble()));
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
                var original = Program(new LegacyEvaluator().Evaluate("x", "", CalculationsMode.Real)).Method.DeclaringType;
                var a = typeof(PrecompiledExpressionSurface).GetMethod("gsl_sf_gamma_inc_P", BindingFlags.Static | BindingFlags.NonPublic);
                var e = original.GetMethod("gsl_sf_gamma_inc_P", BindingFlags.Static | BindingFlags.NonPublic);
                var actual = (Func<double,double,double>)Delegate.CreateDelegate(typeof(Func<double,double,double>), a);
                var expected = (Func<double,double,double>)Delegate.CreateDelegate(typeof(Func<double,double,double>), e);
                GslErrorBoundary.ThrowOnError = throwOnError;
                Exception actualError = null, expectedError = null;
                double av = 0, ev = 0;
                try { av = actual(-1, 1); } catch (Exception error) { actualError = error; }
                try { ev = expected(-1, 1); } catch (Exception error) { expectedError = error; }
                NUnit.Framework.Assert.That(actualError?.GetType(), Is.EqualTo(expectedError?.GetType()));
                if (throwOnError) NUnit.Framework.Assert.That(actualError, Is.Not.Null);
                else { NUnit.Framework.Assert.That(actualError, Is.Null); NUnit.Framework.Assert.That(av, Is.NaN); NUnit.Framework.Assert.That(ev, Is.NaN); }
                NUnit.Framework.Assert.That(actual(1,1), Is.EqualTo(1-Math.Exp(-1)).Within(2e-13));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }
    }
}
