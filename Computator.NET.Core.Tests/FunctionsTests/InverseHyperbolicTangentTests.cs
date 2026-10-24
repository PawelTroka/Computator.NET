using System;
using System.Collections.Generic;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    public class InverseHyperbolicTangentTests
    {
        private readonly bool _generated;
        private NativeCompiler _compiler;
        private Func<double, double>[] _tangents;
        private Func<double, double>[] _cotangents;

        public InverseHyperbolicTangentTests(bool generated)
        {
            _generated = generated;
        }

        [OneTimeSetUp]
        public void CompileTheCompleteEmittedElementaryFunctions()
        {
            _tangents = new Func<double, double>[] { ElementaryFunctions.artanh, ElementaryFunctions.artgh };
            _cotangents = new Func<double, double>[] { ElementaryFunctions.arcoth, ElementaryFunctions.arctgh };
            if (!_generated) return;

            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + ElementaryFunctions.ToCode + "}");
            var type = assembly.GetType("EmittedFunctions");
            Func<double, double> Emitted(string name) => type.GetMethod(name, new[] { typeof(double) })
                .CreateDelegate<Func<double, double>>();
            _tangents = new[] { Emitted("artanh"), Emitted("artgh") };
            _cotangents = new[] { Emitted("arcoth"), Emitted("arctgh") };
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedFunctions()
        {
            _tangents = null;
            _cotangents = null;
            _compiler?.Dispose();
        }

        // Independent fixtures: evaluate the logarithmic definitions at 700
        // decimal digits, starting from the exact binary64 input, then round
        // the result to double. Both signs exercise the functions' oddness.
        [TestCase(0.25, 0.25541281188299536)]
        [TestCase(0.5, 0.5493061443340549)]
        [TestCase(0.9, 1.4722194895832204)]
        [TestCase(0.9999999999999999, 18.714973875118524)]
        public void TangentsAgreeWithIndependentReferences(double input, double expected)
        {
            AssertBothSigns(_tangents, input, expected);
        }

        [TestCase(1e-10)]
        [TestCase(1e-20)]
        [TestCase(1e-300)]
        [TestCase(double.Epsilon)]
        public void TinyTangentsRetainTheInputInsteadOfCancellingToZero(double input)
        {
            // artanh(x) = x + x^3/3 + ... rounds to x for these inputs.
            foreach (var function in _tangents)
                foreach (var value in new[] { input, -input })
                    AssertBits(function(value), value);
        }

        [Test]
        public void TangentsPreserveBothSignedZeros()
        {
            foreach (var function in _tangents)
                foreach (var zero in new[] { 0d, Math.CopySign(0d, -1d) })
                    AssertBits(function(zero), zero);
        }

        private static IEnumerable<TestCaseData> CotangentReferences
        {
            get
            {
                // A rounded reciprocal close to one can magnify its own error.
                yield return new TestCaseData(1.00000001, 9.556913967794891);
                yield return new TestCaseData(1.000001, 7.254329119303181);
                yield return new TestCaseData(1.01, 2.6516524540295374);
                yield return new TestCaseData(1.1, 1.522261218861711);
                yield return new TestCaseData(1.5, 0.8047189562170501);
                yield return new TestCaseData(Math.BitIncrement(1d), 18.36840028483855);
                // Verify both sides of the numerical branch transition.
                yield return new TestCaseData(Math.BitDecrement(2d), 0.5493061443340549);
                yield return new TestCaseData(2d, 0.5493061443340549);
                yield return new TestCaseData(Math.BitIncrement(2d), 0.5493061443340547);
                yield return new TestCaseData(1e20, 1e-20);
                yield return new TestCaseData(1e300, 1e-300);
                yield return new TestCaseData(double.MaxValue, 5.562684646268003e-309);
            }
        }

        [TestCaseSource(nameof(CotangentReferences))]
        public void CotangentsKeepAccurateResultsNearThePolesAndAtLargeMagnitudes(double input, double expected)
        {
            AssertBothSigns(_cotangents, input, expected);
        }

        [Test]
        public void BothFunctionsRetainTheirSignedInfiniteEndpointLimits()
        {
            foreach (var functions in new[] { _tangents, _cotangents })
                foreach (var function in functions)
                    foreach (var sign in new[] { 1d, -1d })
                        AssertBits(function(sign), Math.CopySign(double.PositiveInfinity, sign));
        }

        [Test]
        public void InfiniteCotangentArgumentsApproachSignedZero()
        {
            foreach (var function in _cotangents)
                foreach (var sign in new[] { 1d, -1d })
                    AssertBits(function(Math.CopySign(double.PositiveInfinity, sign)), Math.CopySign(0d, sign));
        }

        [Test]
        public void ValuesOutsideEachRealDomainAndNaNRemainNaN()
        {
            foreach (var function in _tangents)
                foreach (var input in new[] { Math.BitIncrement(1d), -Math.BitIncrement(1d), 2d, -2d,
                    double.PositiveInfinity, double.NegativeInfinity, double.NaN })
                    NUnit.Framework.Assert.That(function(input), Is.NaN, function.Method.Name);
            foreach (var function in _cotangents)
                foreach (var input in new[] { Math.BitDecrement(1d), -Math.BitDecrement(1d), 0.5, -0.5,
                    0d, Math.CopySign(0d, -1d), double.Epsilon, -double.Epsilon, double.NaN })
                    NUnit.Framework.Assert.That(function(input), Is.NaN, function.Method.Name);
        }

        private static void AssertBothSigns(Func<double, double>[] functions, double input, double expected)
        {
            foreach (var function in functions)
                foreach (var sign in new[] { 1d, -1d })
                    NUnit.Framework.Assert.That(function(sign * input), Is.EqualTo(sign * expected).Within(4).Ulps,
                        function.Method.Name);
        }

        private static void AssertBits(double actual, double expected)
        {
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(actual),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
        }
    }
}
