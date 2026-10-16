using System;
using System.Numerics;
using Computator.NET.Core.Transformations;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.TransformationsTests
{
    [TestFixture]
    public class MathematicalTransformationsTests
    {
        [Test, Combinatorial]
        public void RealHartleyMatchesItsDefinitionAndRoundTripsInPlace(
            [Values("zero", "constant", "impulse", "mixed")] string signal,
            [Values(0, 1, 2, 3, 5)] int length)
        {
            var samples = new double[length];
            for (var index = 0; index < length; index++)
                samples[index] = signal == "zero" ? 0 : signal == "constant" ? 1
                    : signal == "impulse" ? (index == 0 ? 1 : 0) : index * index - 0.75;
            var original = (double[])samples.Clone();
            var expected = new double[length];
            for (var frequency = 0; frequency < length; frequency++)
                for (var sample = 0; sample < length; sample++)
                {
                    var angle = 2 * Math.PI * sample * frequency / length;
                    expected[frequency] += original[sample] * (Math.Cos(angle) + Math.Sin(angle)) / Math.Sqrt(length);
                }

            var transformed = MathematicalTransformations.Transform(samples, "DHT");

            NUnit.Framework.Assert.That(transformed, Is.SameAs(samples), "Preserve the real overload's in-place contract.");
            NUnit.Framework.Assert.That(transformed, Is.EqualTo(expected).Within(1e-10));
            var restored = MathematicalTransformations.Transform(transformed, "DHT");
            NUnit.Framework.Assert.That(restored, Is.SameAs(samples));
            NUnit.Framework.Assert.That(restored, Is.EqualTo(original).Within(1e-10));
        }

        [Test, Combinatorial]
        public void ComplexTransformsMatchIndependentRectangularCoefficients(
            [Values("DCT", "IDCT", "DHT")] string transform,
            [Values(1, 2, 3, 5)] int length)
        {
            var samples = Samples(length);
            var original = (Complex[])samples.Clone();

            var actual = MathematicalTransformations.Transform(samples, transform);

            AssertClose(actual, ReferenceTransform(original, transform));
            NUnit.Framework.Assert.That(samples, Is.EqualTo(original), "Input samples must remain unchanged.");
            NUnit.Framework.Assert.That(actual, Is.Not.SameAs(samples));
        }

        [Test, Combinatorial]
        public void ComplexTransformsRoundTripIncludingEmptyAndSingleSampleArrays(
            [Values("DCT", "IDCT", "DHT")] string transform,
            [Values(0, 1, 2, 3, 5)] int length)
        {
            var samples = Samples(length);
            var inverse = transform == "DCT" ? "IDCT" : transform == "IDCT" ? "DCT" : "DHT";

            var transformed = MathematicalTransformations.Transform(samples, transform);
            var restored = MathematicalTransformations.Transform(transformed, inverse);

            NUnit.Framework.Assert.That(transformed.Length, Is.EqualTo(length));
            AssertClose(restored, samples);
        }

        [Test]
        public void ComplexCosineTransformPreservesTheTwoColumnConvention()
        {
            // The orthonormal 2D DCT of [[1, 2], [3, 4]] is [[5, -1], [-2, 0]].
            var actual = MathematicalTransformations.Transform(
                new[] { new Complex(1, 2), new Complex(3, 4) }, "DCT");

            AssertClose(actual, new[] { new Complex(5, -1), new Complex(-2, 0) });
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public void ComplexHartleyConstantSignalHasOnlyTheOrthonormalDcCoefficient(int length)
        {
            var samples = new Complex[length];
            for (var index = 0; index < length; index++) samples[index] = new Complex(1, 1);
            var expected = new Complex[length];
            expected[0] = new Complex(Math.Sqrt(2 * length), 0);

            AssertClose(MathematicalTransformations.Transform(samples, "DHT"), expected);
        }

        private static Complex[] Samples(int length)
        {
            var samples = new Complex[length];
            for (var index = 0; index < length; index++)
                samples[index] = new Complex(index * index - 0.75, 2.5 - 1.25 * index);
            return samples;
        }

        private static void AssertClose(Complex[] actual, Complex[] expected)
        {
            NUnit.Framework.Assert.That(actual.Length, Is.EqualTo(expected.Length));
            for (var index = 0; index < expected.Length; index++)
                NUnit.Framework.Assert.That((actual[index] - expected[index]).Magnitude,
                    Is.LessThan(1e-10), "Coefficient " + index);
        }

        private static Complex[] ReferenceTransform(Complex[] samples, string transform)
        {
            // Direct 2D defining sums, independent of the adapter's 1D library routines.
            // Each input row is [Re(sample), Im(sample)], so the second dimension is 2.
            var result = new Complex[samples.Length];
            for (var outputRow = 0; outputRow < samples.Length; outputRow++)
            {
                var columns = new double[2];
                for (var outputColumn = 0; outputColumn < 2; outputColumn++)
                    for (var inputRow = 0; inputRow < samples.Length; inputRow++)
                        for (var inputColumn = 0; inputColumn < 2; inputColumn++)
                        {
                            var value = inputColumn == 0 ? samples[inputRow].Real : samples[inputRow].Imaginary;
                            double weight;
                            if (transform == "DHT")
                            {
                                var angle = 2 * Math.PI * (inputRow * outputRow / (double)samples.Length
                                    + inputColumn * outputColumn / 2.0);
                                weight = (Math.Cos(angle) + Math.Sin(angle)) / Math.Sqrt(2 * samples.Length);
                            }
                            else if (transform == "DCT")
                            {
                                weight = CosineBasis(inputRow, outputRow, samples.Length)
                                    * CosineBasis(inputColumn, outputColumn, 2);
                            }
                            else
                            {
                                weight = CosineBasis(outputRow, inputRow, samples.Length)
                                    * CosineBasis(outputColumn, inputColumn, 2);
                            }
                            columns[outputColumn] += value * weight;
                        }
                result[outputRow] = new Complex(columns[0], columns[1]);
            }
            return result;
        }

        private static double CosineBasis(int sample, int frequency, int length) =>
            Math.Sqrt((frequency == 0 ? 1.0 : 2.0) / length)
            * Math.Cos(Math.PI * (2 * sample + 1) * frequency / (2 * length));
    }
}
