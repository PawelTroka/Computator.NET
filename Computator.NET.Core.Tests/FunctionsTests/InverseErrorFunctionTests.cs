using System;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(false), TestFixture(true)]
    public class InverseErrorFunctionTests
    {
        private readonly bool _generated;
        private NativeCompiler _compiler;
        private Func<double, double> _inverseErf;
        private Func<double, double> _inverseErfc;
        private Func<double, double> _erf;
        private Func<double, double> _erfc;

        public InverseErrorFunctionTests(bool generated) => _generated = generated;

        [OneTimeSetUp]
        public void SelectTheHostOrActualEmittedFunctions()
        {
            if (!_generated)
            {
                _inverseErf = SpecialFunctions.inverseErf;
                _inverseErfc = SpecialFunctions.inverseErfc;
                _erf = SpecialFunctions.erf;
                _erfc = SpecialFunctions.erfc;
                return;
            }

            // Compile the complete shipping fragment, as the script path does.
            // These managed functions do not require initializing native GSL.
            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + SpecialFunctions.ToCode + "}");
            var type = assembly.GetType("EmittedFunctions");
            var arguments = new[] { typeof(double) };
            _inverseErf = type.GetMethod("inverseErf", arguments).CreateDelegate<Func<double, double>>();
            _inverseErfc = type.GetMethod("inverseErfc", arguments).CreateDelegate<Func<double, double>>();
            _erf = type.GetMethod("erf", arguments).CreateDelegate<Func<double, double>>();
            _erfc = type.GetMethod("erfc", arguments).CreateDelegate<Func<double, double>>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedFunctions()
        {
            _inverseErf = _inverseErfc = _erf = _erfc = null;
            _compiler?.Dispose();
        }

        // Independent binary64 fixtures from scipy.special.erfinv/erfcinv
        // (SciPy 1.17.0), not from the Meta.Numerics implementation under test.
        [TestCase(-0.99, -1.8213863677184496)]
        [TestCase(-0.75, -0.8134198475976184)]
        [TestCase(-0.5, -0.4769362762044699)]
        [TestCase(-0.125, -0.11123545184094996)]
        [TestCase(0.0, 0.0)]
        [TestCase(0.125, 0.11123545184094996)]
        [TestCase(0.5, 0.4769362762044699)]
        [TestCase(0.75, 0.8134198475976184)]
        [TestCase(0.99, 1.8213863677184496)]
        public void InverseErfAcceptsBothHalvesOfItsRealDomain(double x, double expected)
        {
            AssertClose(_inverseErf(x), expected);
        }

        [TestCase(0.125, 1.0847870400692832)]
        [TestCase(0.5, 0.4769362762044699)]
        [TestCase(1.0, 0.0)]
        [TestCase(1.125, -0.11123545184094996)]
        [TestCase(1.5, -0.4769362762044699)]
        [TestCase(1.75, -0.8134198475976184)]
        [TestCase(1.99, -1.8213863677184496)]
        public void InverseErfcAcceptsValuesAboveOne(double x, double expected)
        {
            AssertClose(_inverseErfc(x), expected);
        }

        [TestCase(-1.0, double.NegativeInfinity)]
        [TestCase(1.0, double.PositiveInfinity)]
        public void InverseErfEndpointsHaveTheirSignedInfiniteLimits(double x, double expected)
        {
            NUnit.Framework.Assert.That(_inverseErf(x), Is.EqualTo(expected));
        }

        [TestCase(0.0, double.PositiveInfinity)]
        [TestCase(2.0, double.NegativeInfinity)]
        public void InverseErfcEndpointsHaveTheirSignedInfiniteLimits(double x, double expected)
        {
            NUnit.Framework.Assert.That(_inverseErfc(x), Is.EqualTo(expected));
        }

        [TestCase(-1.01)]
        [TestCase(1.01)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NaN)]
        public void InverseErfInvalidInputsRemainNaN(double x)
        {
            NUnit.Framework.Assert.That(_inverseErf(x), Is.NaN);
        }

        [TestCase(-0.01)]
        [TestCase(2.01)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NaN)]
        public void InverseErfcInvalidInputsRemainNaN(double x)
        {
            NUnit.Framework.Assert.That(_inverseErfc(x), Is.NaN);
        }

        [TestCase(-2.0)]
        [TestCase(-1.0)]
        [TestCase(-0.25)]
        [TestCase(0.0)]
        [TestCase(0.25)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        public void BothInversesRecoverPositiveAndNegativeArguments(double x)
        {
            AssertClose(_inverseErf(_erf(x)), x);
            AssertClose(_inverseErfc(_erfc(x)), x);
        }

        [TestCase(1e-30, 8.148616223169864)]
        [TestCase(1e-100, 15.065574702592647)]
        public void InverseErfcKeepsDirectEvaluationOfSmallTails(double x, double expected)
        {
            // Computing inverseErf(1-x) instead would round 1-x to 1 here.
            AssertClose(_inverseErfc(x), expected);
        }

        // Independent references: bisect the erfc asymptotic expansion in
        // DLMF 7.12.1 at 110 decimal digits, using the exact binary64 input.
        // With 120 correction terms, the truncation error is below 1e-135.
        [TestCase(1e-310, 26.644806559364763)]
        [TestCase(1e-312, 26.731024310670822)]
        [TestCase(1e-320, 27.073153719853043)]
        [TestCase(double.Epsilon, 27.21329321081295)]
        public void InverseErfcRetainsFiniteAccurateValuesForSubnormalTails(double x, double expected)
        {
            NUnit.Framework.Assert.That(_inverseErfc(x), Is.EqualTo(expected).Within(4).Ulps);
        }

        private static double[] ExtremeTailCutoffNeighbors => new[]
        {
            Math.BitDecrement(1e-300), 1e-300, Math.BitIncrement(1e-300)
        };

        [TestCaseSource(nameof(ExtremeTailCutoffNeighbors))]
        public void InverseErfcRemainsAccurateAcrossTheExtremeTailCutoff(double x)
        {
            // The independently computed roots of all three exact inputs
            // round to the same binary64 value.
            NUnit.Framework.Assert.That(_inverseErfc(x), Is.EqualTo(26.209469960516124).Within(4).Ulps);
        }

        [Test]
        public void NeighboringRepresentableValuesRespectTheDomainBoundaries()
        {
            NUnit.Framework.Assert.That(_inverseErf(Math.BitDecrement(-1d)), Is.NaN);
            NUnit.Framework.Assert.That(_inverseErf(Math.BitIncrement(1d)), Is.NaN);
            NUnit.Framework.Assert.That(_inverseErfc(Math.BitDecrement(0d)), Is.NaN);
            NUnit.Framework.Assert.That(_inverseErfc(Math.BitIncrement(2d)), Is.NaN);

            NUnit.Framework.Assert.That(double.IsFinite(_inverseErf(Math.BitIncrement(-1d))), Is.True);
            NUnit.Framework.Assert.That(double.IsFinite(_inverseErf(Math.BitDecrement(1d))), Is.True);
            NUnit.Framework.Assert.That(double.IsFinite(_inverseErfc(Math.BitDecrement(2d))), Is.True);
        }

        private static void AssertClose(double actual, double expected)
        {
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(3e-14 * Math.Max(1d, Math.Abs(expected))));
        }
    }
}
