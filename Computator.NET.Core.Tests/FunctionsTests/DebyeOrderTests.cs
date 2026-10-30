using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture("host")]
    [TestFixture("generated")]
    [TestFixture("precompiled expression")]
    [TestFixture("precompiled script")]
    [NonParallelizable]
    public class DebyeOrderTests
    {
        private readonly string _surface;
        private NativeCompiler _compiler;
        private Func<int, double, double> _debye;
        private IntPtr _gsl;

        public DebyeOrderTests(string surface) => _surface = surface;

        [OneTimeSetUp]
        public void LoadTheProductionHostOrShippedTslDefinitions()
        {
            GSLInitializer.Initialize();
            _debye = SpecialFunctions.Debye;
            if (_surface == "precompiled expression")
                _debye = PrecompiledExpressionSurface.Debye;
            else if (_surface == "precompiled script")
                _debye = PrecompiledScriptMathSurface.Debye;
            else if (_surface == "generated")
            {
                _compiler = new NativeCompiler();
                _compiler.AddDll(typeof(GslConfig).Assembly.Location);
                var assembly = _compiler.Compile("public static class EmittedFunctions {" + SpecialFunctions.ToCode + "}");
                _gsl = NativeLibrary.Load(GslConfig.GslLibraryName);
                NativeLibrary.SetDllImportResolver(assembly,
                    (name, _, _) => name == GslConfig.GslDllName ? _gsl : IntPtr.Zero);
                _debye = assembly.GetType("EmittedFunctions").GetMethod("Debye")
                    .CreateDelegate<Func<int, double, double>>();
            }
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedDefinitions()
        {
            _debye = null;
            _compiler?.Dispose();
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
        }

        private static IEnumerable<TestCaseData> UnsupportedOrders()
        {
            yield return new TestCaseData(0, 0.0);
            yield return new TestCaseData(0, 10.0);
            yield return new TestCaseData(-1, 10.0);
            yield return new TestCaseData(int.MinValue, 10.0);
            yield return new TestCaseData(7, 0.0);
            yield return new TestCaseData(7, 9.0);
            yield return new TestCaseData(7, Math.BitIncrement(9.0));
            yield return new TestCaseData(7, 10.0);
            yield return new TestCaseData(8, 100.0);
            yield return new TestCaseData(int.MaxValue, 10.0);
            yield return new TestCaseData(7, double.PositiveInfinity);
            yield return new TestCaseData(7, double.NaN);
            yield return new TestCaseData(7, -1.0);
        }

        [TestCaseSource(nameof(UnsupportedOrders))]
        public void UnsupportedOrdersDoNotReturnAnUnrelatedIntegral(int order, double x)
        {
            // Previously Debye(7,10) returned about 5060.55, unchanged as x
            // increased. A normalized positive-order Debye value at x>0 is
            // between zero and one. An unimplemented order must stay explicit.
            NUnit.Framework.Assert.That(_debye(order, x), Is.NaN);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void ImplementedOrdersKeepTheNormalizedZeroLimitAndRealDomain(int order)
        {
            NUnit.Framework.Assert.That(_debye(order, 0), Is.EqualTo(1));
            NUnit.Framework.Assert.That(_debye(order, -1), Is.NaN);
        }

        // Independently integrate n*t^n/expm1(t) on [0,1] with 80 decimal
        // digits, then round to double. This also protects the native dispatch.
        [TestCase(1, 0.7775046341122482)]
        [TestCase(2, 0.7078784756278292)]
        [TestCase(3, 0.6744155640778147)]
        [TestCase(4, 0.6548740688867369)]
        [TestCase(5, 0.6421002580217791)]
        [TestCase(6, 0.6331114258349511)]
        public void ImplementedOrdersStillMatchIndependentNormalizedValues(int order, double expected)
        {
            NUnit.Framework.Assert.That(_debye(order, 1), Is.EqualTo(expected).Within(2e-14));
        }
    }
}
