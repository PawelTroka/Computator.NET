using System;
using System.Numerics;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Configuration;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    public class RealRootTests
    {
        private readonly bool _generated;
        private NativeCompiler _compiler;
        private Func<double, double, object> _root;
        private Func<Complex, Complex, Complex> _complexRoot;

        public RealRootTests(bool generated)
        {
            _generated = generated;
        }

        [OneTimeSetUp]
        public void CompileTheCompleteEmittedElementaryFunctions()
        {
            _root = ElementaryFunctions.root;
            _complexRoot = ElementaryFunctions.root;
            if (!_generated) return;

            _compiler = new NativeCompiler();
            _compiler.AddDll(typeof(GslConfig).Assembly.Location);
            var assembly = _compiler.Compile("public static class EmittedFunctions {" + ElementaryFunctions.ToCode + "}");
            var type = assembly.GetType("EmittedFunctions");
            _root = type.GetMethod("root", new[] { typeof(double), typeof(double) })
                .CreateDelegate<Func<double, double, object>>();
            _complexRoot = type.GetMethod("root", new[] { typeof(Complex), typeof(Complex) })
                .CreateDelegate<Func<Complex, Complex, Complex>>();
        }

        [OneTimeTearDown]
        public void ReleaseTheEmittedFunctions()
        {
            _root = null;
            _complexRoot = null;
            _compiler?.Dispose();
        }

        [TestCase(-8d, 3d, -2d)]
        [TestCase(-8d, -3d, -0.5)]
        [TestCase(-32d, 5d, -2d)]
        [TestCase(-32d, -5d, -0.5)]
        [TestCase(-512d, 9d, -2d)]
        [TestCase(-512d, -9d, -0.5)]
        [TestCase(-8d, 1d, -8d)]
        [TestCase(-8d, -1d, -0.125)]
        [TestCase(-1d, 49d, -1d)]
        [TestCase(-1d, -49d, -1d)]
        [TestCase(-1d, 9007199254740991d, -1d)]
        [TestCase(-1d, -9007199254740991d, -1d)]
        public void OddIntegerIndicesSelectTheNegativeRealRoot(double input, double index, double expected)
        {
            var actual = _root(input, index);
            NUnit.Framework.Assert.That(actual, Is.TypeOf<double>());
            NUnit.Framework.Assert.That((double)actual, Is.EqualTo(expected).Within(4).Ulps);
        }

        [TestCase(2d, 0d, 4d)]
        [TestCase(-2d, 0d, -0.25)]
        [TestCase(4d, 1.4142135623730951, 1.4142135623730951)]
        [TestCase(-4d, 0.3535533905932738, -0.3535533905932738)]
        public void EvenIntegerIndicesRetainThePrincipalComplexRoot(double index, double real, double imaginary)
        {
            var actual = _root(-16d, index);
            NUnit.Framework.Assert.That(actual, Is.TypeOf<Complex>());
            AssertComplex((Complex)actual, new Complex(real, imaginary));
        }

        [TestCase(9007199254740992d)]
        [TestCase(-9007199254740992d)]
        [TestCase(double.MaxValue)]
        [TestCase(double.MinValue)]
        public void LargeEvenIndicesRemainComplexWithoutRecoveringTheIndexFromItsReciprocal(double index)
        {
            var actual = _root(-1d, index);
            NUnit.Framework.Assert.That(actual, Is.TypeOf<Complex>());
            var value = (Complex)actual;
            NUnit.Framework.Assert.That(value.Real, Is.EqualTo(1d));
            // For these indices, sin(pi/n) rounds to pi/n; this also verifies
            // the nonzero, signed imaginary component at subnormal magnitudes.
            NUnit.Framework.Assert.That(value.Imaginary, Is.EqualTo(Math.PI / index).Within(4).Ulps);
        }

        [TestCase(8d, 3d, 2d)]
        [TestCase(8d, -3d, 0.5)]
        [TestCase(16d, 2d, 4d)]
        [TestCase(16d, -2d, 0.25)]
        [TestCase(81d, 4d, 3d)]
        [TestCase(81d, -4d, 0.3333333333333333)]
        public void PositiveRadicandsRetainRealResults(double input, double index, double expected)
        {
            var actual = _root(input, index);
            NUnit.Framework.Assert.That(actual, Is.TypeOf<double>());
            NUnit.Framework.Assert.That((double)actual, Is.EqualTo(expected).Within(4).Ulps);
        }

        [TestCase(0.5, 64d)]
        [TestCase(-0.5, 0.015625)]
        [TestCase(0.25, 4096d)]
        [TestCase(-0.25, 0.000244140625)]
        public void NonintegerIndicesWithIntegerReciprocalKeepTheirExistingPower(double index, double expected)
        {
            AssertRealBits(_root(-8d, index), expected);
        }

        [TestCase(1.5)]
        [TestCase(2.5)]
        public void OtherPositiveNonintegerIndicesKeepTheirExistingNaNResult(double index)
        {
            var actual = _root(-8d, index);
            NUnit.Framework.Assert.That(actual, Is.TypeOf<double>());
            NUnit.Framework.Assert.That((double)actual, Is.NaN);
        }

        [TestCase(-1.5, -0.125, -0.21650635094610965)]
        [TestCase(-2.5, 0.13450745926059285, -0.4139713929935977)]
        public void OtherNegativeNonintegerIndicesKeepTheirExistingComplexConvention(double index, double real, double imaginary)
        {
            var actual = _root(-8d, index);
            NUnit.Framework.Assert.That(actual, Is.TypeOf<Complex>());
            AssertComplex((Complex)actual, new Complex(real, imaginary));
        }

        [Test]
        public void OddRootsOfNegativeInfinityRetainTheRealSign()
        {
            AssertRealBits(_root(double.NegativeInfinity, 3d), double.NegativeInfinity);
            AssertRealBits(_root(double.NegativeInfinity, -3d), Math.CopySign(0d, -1d));
        }

        [Test]
        public void SpecialRadicandsKeepTheirExistingFloatingPointBehavior()
        {
            foreach (var zero in new[] { 0d, Math.CopySign(0d, -1d) })
            {
                AssertRealBits(_root(zero, 3d), 0d);
                AssertRealBits(_root(zero, -3d), double.PositiveInfinity);
                AssertRealBits(_root(zero, 1d), zero);
                AssertRealBits(_root(zero, -1d), Math.CopySign(double.PositiveInfinity, zero));
            }
            AssertRealBits(_root(double.PositiveInfinity, 3d), double.PositiveInfinity);
            AssertRealBits(_root(double.PositiveInfinity, -3d), 0d);
            NUnit.Framework.Assert.That(_root(double.NaN, 3d), Is.NaN);
        }

        [Test]
        public void ZeroAndNonfiniteIndicesKeepTheirExistingFloatingPointBehavior()
        {
            foreach (var zero in new[] { 0d, Math.CopySign(0d, -1d) })
            {
                var actual = _root(-8d, zero);
                NUnit.Framework.Assert.That(actual, Is.TypeOf<Complex>());
                NUnit.Framework.Assert.That(((Complex)actual).Real, Is.NaN);
                NUnit.Framework.Assert.That(((Complex)actual).Imaginary, Is.NaN);
            }
            AssertRealBits(_root(-8d, double.PositiveInfinity), 1d);
            AssertRealBits(_root(-8d, double.NegativeInfinity), 1d);
            NUnit.Framework.Assert.That(_root(-8d, double.NaN), Is.NaN);
        }

        [TestCase(3d, 1d, 1.7320508075688772)]
        [TestCase(-3d, 0.25, -0.4330127018922193)]
        public void ExplicitComplexArgumentsKeepTheirPrincipalBranch(double index, double real, double imaginary)
        {
            AssertComplex(_complexRoot(new Complex(-8d, 0d), new Complex(index, 0d)),
                new Complex(real, imaginary));
        }

        private static void AssertComplex(Complex actual, Complex expected)
        {
            NUnit.Framework.Assert.That(actual.Real, Is.EqualTo(expected.Real).Within(4e-15));
            NUnit.Framework.Assert.That(actual.Imaginary, Is.EqualTo(expected.Imaginary).Within(4e-15));
        }

        private static void AssertRealBits(object actual, double expected)
        {
            NUnit.Framework.Assert.That(actual, Is.TypeOf<double>());
            NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits((double)actual),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
        }
    }

    [TestFixture(false)]
    [TestFixture(true)]
    public class RealRootExpressionTests
    {
        private readonly bool _legacy;

        public RealRootExpressionTests(bool legacy)
        {
            _legacy = legacy;
        }

        private sealed class LegacyEvaluator : ExpressionsEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        [TestCase("root(-8,-3)", 0d, -0.5)]
        [TestCase("root(-pow(2,49),49)", 0d, -2d)]
        [TestCase("root(-pow(2,49),-49)", 0d, -0.5)]
        [TestCase("root(x,-3)", -8d, -0.5)]
        [TestCase("root(x,49)", -562949953421312d, -2d)]
        [TestCase("root(x,-3)+root(x,3)", -8d, -2.5)]
        public void RealExpressionsReturnTheSelectedRealRoot(string expression, double input, double expected)
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyEvaluator() : new ExpressionsEvaluator();
            Func<double, double> function = evaluator.Evaluate(expression, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(input), Is.EqualTo(expected).Within(4).Ulps);
        }

        [Test]
        public void ComplexExpressionsKeepTheirPrincipalBranch()
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyEvaluator() : new ExpressionsEvaluator();
            Func<Complex, Complex> function = evaluator.Evaluate("root(z,-3)", "", CalculationsMode.Complex);
            var result = function(new Complex(-8d, 0d));
            NUnit.Framework.Assert.That(result.Real, Is.EqualTo(0.25).Within(4e-15));
            NUnit.Framework.Assert.That(result.Imaginary, Is.EqualTo(-0.4330127018922193).Within(4e-15));
        }
    }
}
