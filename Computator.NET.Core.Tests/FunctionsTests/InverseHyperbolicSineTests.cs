using System;
using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    public class InverseHyperbolicSineTests
    {
        private Func<double, double> _generatedReal;
        private Func<Complex, Complex> _generatedComplex;

        [OneTimeSetUp]
        public void CompileActualGeneratedDefinitions()
        {
            GSLInitializer.Initialize();
            var real = new ExpressionsEvaluator().Evaluate("arsinh(x)", "", CalculationsMode.Real);
            var complex = new ExpressionsEvaluator().Evaluate("arsinh(z)", "", CalculationsMode.Complex);
            _generatedReal = x => real.Evaluate(x);
            _generatedComplex = z => complex.Evaluate(z);
        }

        [TestCase(0d, 0d)]
        [TestCase(1d, 0.88137358701954302523)]
        [TestCase(-1d, -0.88137358701954302523)]
        [TestCase(2d, 1.4436354751788103425)]
        [TestCase(-2d, -1.4436354751788103425)]
        [TestCase(10d, 2.9982229502979697388)]
        [TestCase(-10d, -2.9982229502979697388)]
        public void RealValuesAgreeWithIndependentReferences(double input, double expected)
        {
            AssertScalar(ElementaryFunctions.arsinh(input), expected);
            AssertScalar(_generatedReal(input), expected);
        }

        [TestCase(double.Epsilon)]
        [TestCase(-double.Epsilon)]
        [TestCase(1e-300)]
        [TestCase(-1e-300)]
        [TestCase(1e300)]
        [TestCase(-1e300)]
        [TestCase(double.MaxValue)]
        [TestCase(double.MinValue)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        public void RealExtremesMatchTheRuntimeWithoutIntermediateOverflow(double input)
        {
            var expected = Math.Asinh(input);
            AssertBits(ElementaryFunctions.arsinh(input), expected);
            AssertBits(_generatedReal(input), expected);
            if (!double.IsInfinity(input))
                Assert.That(double.IsFinite(_generatedReal(input)), Is.True);
        }

        [Test]
        public void RealNaNRemainsNaN()
        {
            Assert.That(double.IsNaN(ElementaryFunctions.arsinh(double.NaN)), Is.True);
            Assert.That(double.IsNaN(_generatedReal(double.NaN)), Is.True);
        }

        [TestCase(1)]
        [TestCase(-1)]
        public void RealSignedZeroIsPreserved(int sign)
        {
            var zero = Math.CopySign(0d, sign);
            AssertBits(ElementaryFunctions.arsinh(zero), zero);
            AssertBits(_generatedReal(zero), zero);
        }

        // Independent principal-branch fixtures evaluated at 90 decimal digits
        // with mpmath, then rounded to binary64. Do not use the implementation's
        // rotated Complex.Asin expression to generate its own expected values.
        [TestCase(1d, 1d, 1.0612750619050356520, 0.6662394324925152551)]
        [TestCase(1d, -1d, 1.0612750619050356520, -0.6662394324925152551)]
        [TestCase(-1d, 1d, -1.0612750619050356520, 0.6662394324925152551)]
        [TestCase(-1d, -1d, -1.0612750619050356520, -0.6662394324925152551)]
        [TestCase(2.3, 1.4, 1.7003867119390188, 0.5176222301342543)]
        [TestCase(-2.3, 1.4, -1.7003867119390188, 0.5176222301342543)]
        [TestCase(1e300, 1e300, 691.8152486690536, 0.7853981633974483)]
        [TestCase(-1e300, 1e300, -691.8152486690536, 0.7853981633974483)]
        public void ComplexValuesAgreeWithIndependentReferences(double real, double imaginary, double expectedReal, double expectedImaginary)
        {
            var input = new Complex(real, imaginary);
            var expected = new Complex(expectedReal, expectedImaginary);
            AssertComplex(ElementaryFunctions.arsinh(input), expected);
            AssertComplex(_generatedComplex(input), expected);
        }

        [TestCase(1, 0.5)]
        [TestCase(-1, 0.5)]
        [TestCase(1, -0.5)]
        [TestCase(-1, -0.5)]
        [TestCase(1, 1d)]
        [TestCase(-1, 1d)]
        [TestCase(1, -1d)]
        [TestCase(-1, -1d)]
        [TestCase(1, 2d)]
        [TestCase(-1, 2d)]
        [TestCase(1, -2d)]
        [TestCase(-1, -2d)]
        public void ImaginaryAxisRetainsTheSignedZeroSideOfEachCut(int side, double imaginary)
        {
            var input = new Complex(Math.CopySign(0d, side), imaginary);
            var magnitude = Math.Abs(imaginary);
            var expected = magnitude <= 1
                ? new Complex(Math.CopySign(0d, side), Math.Asin(imaginary))
                : new Complex(Math.CopySign(Math.Acosh(magnitude), side), Math.CopySign(Math.PI / 2, imaginary));
            foreach (var result in new[] { ElementaryFunctions.arsinh(input), _generatedComplex(input) })
            {
                AssertComplex(result, expected);
                if (magnitude <= 1)
                    AssertBits(result.Real, expected.Real);
                else
                    Assert.That(Math.Sign(result.Real), Is.EqualTo(side));
            }
        }

        [TestCase(0d)]
        [TestCase(-2d)]
        [TestCase(double.Epsilon)]
        [TestCase(-double.Epsilon)]
        [TestCase(double.MaxValue)]
        [TestCase(double.MinValue)]
        public void ComplexRealAxisMatchesRealOverloadAndPreservesImaginaryZero(double real)
        {
            foreach (var imaginary in new[] { 0d, BitConverter.Int64BitsToDouble(long.MinValue) })
            {
                var input = new Complex(real, imaginary);
                foreach (var result in new[] { ElementaryFunctions.arsinh(input), _generatedComplex(input) })
                {
                    AssertBits(result.Real, Math.Asinh(real));
                    AssertBits(result.Imaginary, imaginary);
                }
            }
        }

        [TestCase(0.2, 0.3)]
        [TestCase(-2.3, 1.4)]
        [TestCase(2.3, -1.4)]
        [TestCase(-2.3, -1.4)]
        [TestCase(1e-10, 2d)]
        [TestCase(-1e-10, 2d)]
        public void TakingSinhRecoversTheOriginalComplexInput(double real, double imaginary)
        {
            var input = new Complex(real, imaginary);
            AssertComplex(Complex.Sinh(ElementaryFunctions.arsinh(input)), input);
            AssertComplex(Complex.Sinh(_generatedComplex(input)), input);
        }

        [TestCase(1, 1)]
        [TestCase(1, -1)]
        [TestCase(-1, 1)]
        [TestCase(-1, -1)]
        public void InfiniteComponentsHaveTheCorrectQuadrantLimit(int realSign, int imaginarySign)
        {
            var input = new Complex(Math.CopySign(double.PositiveInfinity, realSign), Math.CopySign(double.PositiveInfinity, imaginarySign));
            foreach (var result in new[] { ElementaryFunctions.arsinh(input), _generatedComplex(input) })
            {
                AssertBits(result.Real, input.Real);
                AssertScalar(result.Imaginary, Math.CopySign(Math.PI / 4, imaginarySign));
            }
        }

        private static void AssertScalar(double actual, double expected)
        {
            Assert.That(actual, Is.EqualTo(expected).Within(3e-14 * Math.Max(1d, Math.Abs(expected))));
        }

        private static void AssertComplex(Complex actual, Complex expected)
        {
            AssertScalar(actual.Real, expected.Real);
            AssertScalar(actual.Imaginary, expected.Imaginary);
        }

        private static void AssertBits(double actual, double expected)
        {
            Assert.That(BitConverter.DoubleToInt64Bits(actual), Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
        }
    }
}
