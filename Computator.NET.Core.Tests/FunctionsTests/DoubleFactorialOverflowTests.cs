using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture, NonParallelizable]
    public class DoubleFactorialOverflowTests
    {
        private NativeCompiler _compiler;
        private Func<long, double> _emittedInteger;
        private Func<double, double> _emittedReal;
        private IntPtr _gsl;

        [OneTimeSetUp]
        public void CompileTheCompleteEmittedElementaryFunctions()
        {
            GSLInitializer.Initialize();
            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + ElementaryFunctions.ToCode + "}");
            _gsl = NativeLibrary.Load(GslConfig.GslLibraryName);
            NativeLibrary.SetDllImportResolver(assembly,
                (name, _, _) => name == GslConfig.GslDllName ? _gsl : IntPtr.Zero);
            var functions = assembly.GetType("EmittedFunctions");
            _emittedInteger = functions.GetMethod("DoubleFactorial", new[] { typeof(long) })
                .CreateDelegate<Func<long, double>>();
            _emittedReal = functions.GetMethod("DoubleFactorial", new[] { typeof(double) })
                .CreateDelegate<Func<double, double>>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedHelper()
        {
            _emittedInteger = null;
            _emittedReal = null;
            _compiler?.Dispose();
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
        }

        // Independent exact-integer identities give (2k)!! = 2^k k! and
        // (2k+1)!! = (2k+1)! / (2^k k!). The large finite references below
        // were rounded from those integers, not obtained from the GSL table.
        private const double Even296 = 9.121268533982767e302;
        private const double Odd297 = 1.2552756225993064e304;

        private static IEnumerable<TestCaseData> IntegerValues
        {
            get
            {
                var values = new (long Input, double Expected)[]
                {
                    (0, 1), (1, 1), (2, 2), (7, 105), (8, 384),
                    (296, Even296), (297, Odd297),
                    // GSL deliberately treats every order above 297 as overflow.
                    // This fix retains that cutoff, including 298 through 300.
                    (298, double.PositiveInfinity), (299, double.PositiveInfinity),
                    (300, double.PositiveInfinity), (uint.MaxValue, double.PositiveInfinity),
                    (1L << 32, double.PositiveInfinity), ((1L << 32) + 1, double.PositiveInfinity),
                    ((1L << 32) + 7, double.PositiveInfinity), (1L << 40, double.PositiveInfinity),
                    (long.MaxValue, double.PositiveInfinity),
                    (-1, double.NaN), (long.MinValue, double.NaN)
                };
                foreach (var value in values)
                    foreach (var emitted in new[] { false, true })
                        yield return new TestCaseData(value.Input, value.Expected, emitted);
            }
        }

        private static IEnumerable<TestCaseData> RealValues
        {
            get
            {
                var values = new (double Input, double Expected)[]
                {
                    (0.5, 1), (7.9, 105), (296.75, Even296), (297.75, Odd297),
                    (Math.BitDecrement(298), Odd297), (298, double.PositiveInfinity),
                    (1L << 32, double.PositiveInfinity), ((1L << 32) + 1, double.PositiveInfinity),
                    ((1L << 32) + 7, double.PositiveInfinity), (1L << 40, double.PositiveInfinity),
                    (Math.BitDecrement(9223372036854775808d), double.PositiveInfinity),
                    (9223372036854775808d, double.PositiveInfinity),
                    (double.MaxValue, double.PositiveInfinity), (double.PositiveInfinity, double.PositiveInfinity),
                    // Keep truncation toward zero and the existing negative-order guard.
                    (-0.5, 1), (-1.5, double.NaN), (double.MinValue, double.NaN),
                    (double.NegativeInfinity, double.NaN), (double.NaN, double.NaN)
                };
                foreach (var value in values)
                    foreach (var emitted in new[] { false, true })
                        yield return new TestCaseData(value.Input, value.Expected, emitted);
            }
        }

        [TestCaseSource(nameof(IntegerValues))]
        public void IntegerOrdersCannotWrapToSmallFiniteValues(long input, double expected, bool emitted)
        {
            var function = emitted ? _emittedInteger : ElementaryFunctions.DoubleFactorial;
            CheckReturnValue(() => function(input), expected);
        }

        [TestCaseSource(nameof(RealValues))]
        public void RealOverloadRetainsTruncationAndTheSameOverflowContract(double input, double expected, bool emitted)
        {
            var function = emitted ? _emittedReal : ElementaryFunctions.DoubleFactorial;
            CheckReturnValue(() => function(input), expected);
        }

        private static void CheckReturnValue(Func<double> evaluate, double expected)
        {
            var previous = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = false;
                if (double.IsNaN(expected))
                    NUnit.Framework.Assert.That(evaluate(), Is.NaN);
                else if (double.IsPositiveInfinity(expected))
                    NUnit.Framework.Assert.That(evaluate(), Is.EqualTo(double.PositiveInfinity));
                else
                    NUnit.Framework.Assert.That(evaluate(), Is.EqualTo(expected).Within(expected * 4e-15));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }

        [TestCase(298L)]
        [TestCase(4294967296L)]
        [TestCase(4294967303L)]
        [TestCase(long.MaxValue)]
        public void OversizedIntegerOrdersKeepNativeErrorTranslationAndRecovery(long input)
        {
            foreach (var function in new Func<long, double>[] { ElementaryFunctions.DoubleFactorial, _emittedInteger })
                CheckErrorTranslation(() => function(input), () => function(7), () => function(-1));
        }

        [TestCase(4294967296.0)]
        [TestCase(double.MaxValue)]
        [TestCase(double.PositiveInfinity)]
        public void RealOverloadKeepsNativeErrorTranslationAndRecovery(double input)
        {
            foreach (var function in new Func<double, double>[] { ElementaryFunctions.DoubleFactorial, _emittedReal })
                CheckErrorTranslation(() => function(input), () => function(7), () => function(-1));
        }

        private static void CheckErrorTranslation(Func<double> overflow, Func<double> finite, Func<double> invalid)
        {
            var previous = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = true;
                NUnit.Framework.Assert.Throws<ArithmeticException>(() => overflow());
                NUnit.Framework.Assert.That(finite(), Is.EqualTo(105));
                NUnit.Framework.Assert.That(invalid(), Is.NaN);
                GslErrorBoundary.ThrowOnError = false;
                NUnit.Framework.Assert.That(overflow(), Is.EqualTo(double.PositiveInfinity));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }
    }
}
