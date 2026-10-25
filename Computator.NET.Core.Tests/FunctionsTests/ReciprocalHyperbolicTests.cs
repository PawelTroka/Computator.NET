using System;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(0)]
    [TestFixture(1)]
    [TestFixture(2)]
    public class ReciprocalHyperbolicTests
    {
        private readonly int _surface;
        private NativeCompiler _compiler;
        private Func<double, double> _secant;
        private Func<double, double>[] _cosecants;
        private sealed class ProgramTag { }

        public ReciprocalHyperbolicTests(int surface) => _surface = surface;

        [OneTimeSetUp]
        public void SelectHostEmittedOrPrecompiledFunctions()
        {
            if (_surface == 0)
            {
                _secant = ElementaryFunctions.sech;
                _cosecants = new Func<double, double>[] { ElementaryFunctions.csch, ElementaryFunctions.cosech };
                return;
            }
            if (_surface == 2)
            {
                _secant = PrecompiledExpressionBuiltins<ProgramTag>.sech;
                _cosecants = new Func<double, double>[] { PrecompiledExpressionBuiltins<ProgramTag>.csch,
                    PrecompiledExpressionBuiltins<ProgramTag>.cosech };
                return;
            }

            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + ElementaryFunctions.ToCode + "}");
            var type = assembly.GetType("EmittedFunctions");
            Func<double, double> Emitted(string name) => type.GetMethod(name, new[] { typeof(double) })
                .CreateDelegate<Func<double, double>>();
            _secant = Emitted("sech");
            _cosecants = new[] { Emitted("csch"), Emitted("cosech") };
        }

        [OneTimeTearDown]
        public void ReleaseEmittedFunctions()
        {
            _secant = null;
            _cosecants = null;
            _compiler?.Dispose();
        }

        // Independent fixtures: evaluate 2/(exp(x) +/- exp(-x)) at 820 decimal
        // digits from each exact binary64 input, then round once to double.
        // Include both sides of the implementation switch and the final
        // nonzero result near the underflow boundary 1076*ln(2).
        [TestCase(1e-300, 1.0, 9.999999999999999e+299)]
        [TestCase(1e-20, 1.0, 1e+20)]
        [TestCase(1e-08, 1.0, 100000000.0)]
        [TestCase(0.1, 0.9950207489532265, 9.983352757296108)]
        [TestCase(0.5, 0.8868188839700739, 1.9190347513349437)]
        [TestCase(1.0, 0.6480542736638853, 0.8509181282393216)]
        [TestCase(2.0, 0.26580222883407967, 0.2757205647717832)]
        [TestCase(10.0, 9.079985933781725e-05, 9.079985971212217e-05)]
        [TestCase(50.0, 3.8574996959278356e-22, 3.8574996959278356e-22)]
        [TestCase(100.0, 7.440151952041672e-44, 7.440151952041672e-44)]
        [TestCase(700.0, 1.971935308751954e-304, 1.971935308751954e-304)]
        [TestCase(709.0, 2.4335615012468463e-308, 2.4335615012468463e-308)]
        [TestCase(709.9999999999999, 8.952572451351277e-309, 8.952572451351277e-309)]
        [TestCase(710.0, 8.95257245135026e-309, 8.95257245135026e-309)]
        [TestCase(710.0000000000001, 8.95257245134924e-309, 8.95257245134924e-309)]
        [TestCase(710.4, 6.00108877772664e-309, 6.00108877772664e-309)]
        [TestCase(710.48, 5.53970314683663e-309, 5.53970314683663e-309)]
        [TestCase(711.0, 3.293467350449585e-309, 3.293467350449585e-309)]
        [TestCase(720.0, 4.06446160484e-313, 4.06446160484e-313)]
        [TestCase(730.0, 1.8452626e-317, 1.8452626e-317)]
        [TestCase(740.0, 8.4e-322, 8.4e-322)]
        [TestCase(744.0, 1.5e-323, 1.5e-323)]
        [TestCase(745.0, 5e-324, 5e-324)]
        [TestCase(745.2, 5e-324, 5e-324)]
        [TestCase(745.5, 5e-324, 5e-324)]
        [TestCase(745.8, 5e-324, 5e-324)]
        [TestCase(745.826366282501, 5e-324, 5e-324)]
        [TestCase(745.8263662825011, 5e-324, 5e-324)]
        [TestCase(745.8263662825012, 0.0, 0.0)]
        [TestCase(746.0, 0.0, 0.0)]
        public void RealFunctionsRetainOrdinaryAccuracyAndRepresentableTails(double input, double expectedSecant, double expectedCosecant)
        {
            foreach (var sign in new[] { 1d, -1d })
            {
                AssertMagnitude(_secant(sign * input), expectedSecant);
                foreach (var function in _cosecants)
                    AssertMagnitude(function(sign * input), sign * expectedCosecant);
            }
        }

        [Test]
        public void SignedZerosRetainTheSecantValueAndCosecantPoles()
        {
            foreach (var sign in new[] { 1d, -1d })
            {
                var zero = Math.CopySign(0d, sign);
                AssertBits(_secant(zero), 1d);
                foreach (var function in _cosecants)
                    AssertBits(function(zero), Math.CopySign(double.PositiveInfinity, sign));
            }
        }

        [TestCase(double.Epsilon)]
        [TestCase(1e-320)]
        public void TinyCosecantArgumentsRetainOverflowWithTheirSign(double input)
        {
            foreach (var sign in new[] { 1d, -1d })
            {
                AssertBits(_secant(sign * input), 1d);
                foreach (var function in _cosecants)
                    AssertBits(function(sign * input), Math.CopySign(double.PositiveInfinity, sign));
            }
        }

        [TestCase(750d)]
        [TestCase(double.MaxValue)]
        [TestCase(double.PositiveInfinity)]
        public void UnrepresentableTailsRetainCorrectlySignedZeros(double input)
        {
            foreach (var sign in new[] { 1d, -1d })
            {
                AssertBits(_secant(sign * input), 0d);
                foreach (var function in _cosecants)
                    AssertBits(function(sign * input), Math.CopySign(0d, sign));
            }
        }

        [Test]
        public void NaNRemainsNaN()
        {
            NUnit.Framework.Assert.That(_secant(double.NaN), Is.NaN);
            foreach (var function in _cosecants)
                NUnit.Framework.Assert.That(function(double.NaN), Is.NaN);
        }

        private static void AssertMagnitude(double actual, double expected)
        {
            // At the final few subnormal values, a tolerance could otherwise
            // accept zero for a representable result or the wrong zero sign.
            if (Math.Abs(expected) < 1e-320)
                AssertBits(actual, expected);
            else
                NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(3).Ulps);
        }

        private static void AssertBits(double actual, double expected) =>
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(actual),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
    }
}
