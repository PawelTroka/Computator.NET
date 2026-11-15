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
    public class BinomialCoefficientRangeTests
    {
        private NativeCompiler _compiler;
        private Func<long, long, double> _emitted;
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
            _emitted = assembly.GetType("EmittedFunctions").GetMethod("BinomialCoeff")
                .CreateDelegate<Func<long, long, double>>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedHelper()
        {
            _emitted = null;
            _compiler?.Dispose();
            if (_gsl != IntPtr.Zero) NativeLibrary.Free(_gsl);
        }

        private static IEnumerable<TestCaseData> InvalidInputs
        {
            get
            {
                var pairs = new (long N, long K)[]
                {
                    (-1, 0), (5, -1), (5, 6),
                    // Narrowing these inputs previously produced a different
                    // valid coefficient, or a native domain error for (0,1).
                    (4294967296L, 0), (4294967296L, 1),
                    (4294967297L, 1), (4294967298L, 2),
                    (4294967296L, 4294967296L),
                    (long.MaxValue, 1), (long.MaxValue, long.MaxValue)
                };
                foreach (var pair in pairs)
                    foreach (var emitted in new[] { false, true })
                        foreach (var throwOnNativeError in new[] { false, true })
                            yield return new TestCaseData(pair.N, pair.K, emitted, throwOnNativeError);
            }
        }

        [TestCaseSource(nameof(InvalidInputs))]
        public void InvalidSignedOrNativeBoundsReturnNaNWithoutCallingGsl(
            long n, long k, bool emitted, bool throwOnNativeError)
        {
            var previous = GslErrorBoundary.ThrowOnError;
            try
            {
                GslErrorBoundary.ThrowOnError = throwOnNativeError;
                var function = emitted ? _emitted : ElementaryFunctions.BinomialCoeff;
                NUnit.Framework.Assert.That(function(n, k), Is.NaN);
                NUnit.Framework.Assert.That(function(5, 2), Is.EqualTo(10));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }

        private static IEnumerable<TestCaseData> SupportedInputs
        {
            get
            {
                var values = new (long N, long K, double Expected)[]
                {
                    (0, 0, 1), (1, 0, 1), (1, 1, 1), (5, 2, 10), (20, 10, 184756),
                    (uint.MaxValue, 0, 1), (uint.MaxValue, 1, uint.MaxValue),
                    // C(n,2)=n(n-1)/2, independently evaluated as an integer.
                    (uint.MaxValue, 2, 9223372030412324865d),
                    (uint.MaxValue, uint.MaxValue, 1)
                };
                foreach (var value in values)
                    foreach (var emitted in new[] { false, true })
                        yield return new TestCaseData(value.N, value.K, value.Expected, emitted);
            }
        }

        [TestCaseSource(nameof(SupportedInputs))]
        public void NativeRangeIncludingItsUpperEndpointRemainsSupported(long n, long k, double expected, bool emitted)
        {
            var function = emitted ? _emitted : ElementaryFunctions.BinomialCoeff;
            NUnit.Framework.Assert.That(function(n, k), Is.EqualTo(expected).Within(expected * 4e-15));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SupportedArgumentsStillUseTheConfiguredNativeOverflowBehavior(bool emitted)
        {
            var previous = GslErrorBoundary.ThrowOnError;
            try
            {
                var function = emitted ? _emitted : ElementaryFunctions.BinomialCoeff;
                GslErrorBoundary.ThrowOnError = false;
                NUnit.Framework.Assert.That(function(2000, 1000), Is.EqualTo(double.PositiveInfinity));
                GslErrorBoundary.ThrowOnError = true;
                NUnit.Framework.Assert.Throws<ArithmeticException>(() => function(2000, 1000));
                NUnit.Framework.Assert.That(function(5, 2), Is.EqualTo(10));
            }
            finally { GslErrorBoundary.ThrowOnError = previous; }
        }
    }
}
