using System;
using System.Numerics;
using Accord.Math;
using MathNet.Numerics.IntegralTransforms;

namespace Computator.NET.Core.Transformations
{
    public static class MathematicalTransformations
    {
        public static double[] Transform(double[] functionPoints, string transformate)
        {
            var copyofFunctionsPoints = functionPoints;


            switch (transformate)
            {
                case "FFT":
                    var complexArray = Array.ConvertAll(copyofFunctionsPoints, x => new Complex(x, 0));

                    FourierTransform.DFT(complexArray, FourierTransform.Direction.Forward);

                    copyofFunctionsPoints = Array.ConvertAll(complexArray, z => z.Real);
                    break;

                case "IFFT":
                    var complexArray2 = Array.ConvertAll(copyofFunctionsPoints, x => new Complex(x, 0));

                    FourierTransform.DFT(complexArray2, FourierTransform.Direction.Backward);

                    copyofFunctionsPoints = Array.ConvertAll(complexArray2, z => z.Real);
                    break;


                case "DST":
                    SineTransform.DST(copyofFunctionsPoints);
                    break;

                case "IDST":
                    SineTransform.IDST(copyofFunctionsPoints);
                    break;

                case "DCT":
                    CosineTransform.DCT(copyofFunctionsPoints);
                    break;

                case "IDCT":
                    CosineTransform.IDCT(copyofFunctionsPoints);
                    break;

                case "DHT":
                    OrthonormalHartley(copyofFunctionsPoints);
                    break;

                case "FHT":
                    HilbertTransform.FHT(copyofFunctionsPoints,
                        FourierTransform.Direction.Forward);
                    break;

                case "IFHT":
                    HilbertTransform.FHT(copyofFunctionsPoints,
                        FourierTransform.Direction.Backward);
                    break;

                default:
                    throw new ArgumentException("Unknown transformation!");
            }
            return copyofFunctionsPoints; //athenia programuje//dididididi//di/kocham PaciA// JJKAKAKK  K
        }

        public static Complex[] Transform(Complex[] functionPoints, string transformate)
        {
            var copyofFunctionsPoints = functionPoints;


            var copyofFunctionsPoints2 = new Complex[functionPoints.Length];
            for (var i = 0; i < functionPoints.Length; i++)
            {
                copyofFunctionsPoints2[i] = new Complex(functionPoints[i].Real,
                    copyofFunctionsPoints2[i].Imaginary);
            }


            switch (transformate)
            {
                case "FFT":
                    Fourier.Forward(copyofFunctionsPoints);
                    break;


                case "IFFT":
                    Fourier.Inverse(copyofFunctionsPoints);
                    break;

                case "DST":
                    copyofFunctionsPoints = TransformTwoColumns(functionPoints, SineTransform.DST, Math.Sqrt(3) / 2);
                    break;

                case "IDST":
                    copyofFunctionsPoints = TransformTwoColumns(functionPoints, SineTransform.IDST, 1 / Math.Sqrt(3));
                    break;

                case "DCT":
                    copyofFunctionsPoints = TransformTwoColumns(functionPoints, CosineTransform.DCT);
                    break;

                case "IDCT":
                    copyofFunctionsPoints = TransformTwoColumns(functionPoints, CosineTransform.IDCT);
                    break;

                case "DHT":
                    copyofFunctionsPoints = TransformTwoColumns(functionPoints, OrthonormalHartley);
                    break;

                case "FHT":
                    HilbertTransform.FHT(copyofFunctionsPoints2,
                        FourierTransform.Direction.Forward);
                    copyofFunctionsPoints = copyofFunctionsPoints2;
                    break;

                case "IFHT":
                    HilbertTransform.FHT(copyofFunctionsPoints2,
                        FourierTransform.Direction.Backward);
                    copyofFunctionsPoints = copyofFunctionsPoints2;
                    break;

                default:
                    throw new ArgumentException("Unknown transformation!");
            }
            return copyofFunctionsPoints; //athenia programuje//dididididi//di/kocham PaciA// JJKAKAKK  K
        }

        private static Complex[] TransformTwoColumns(Complex[] samples, Action<double[]> columnTransform)
        {
            // The orthonormal two-point DCT, IDCT and DHT use this scale.
            return TransformTwoColumns(samples, columnTransform, 1 / Math.Sqrt(2));
        }

        private static Complex[] TransformTwoColumns(Complex[] samples, Action<double[]> columnTransform, double scale)
        {
            if (samples.Length == 0) return Array.Empty<Complex>();

            // Preserve the N-by-2 [Re, Im] convention without Accord 3.8.0's
            // rectangular inverse-transform indexing and Hartley square-dimension assumptions.
            var real = Array.ConvertAll(samples, sample => sample.Real);
            var imaginary = Array.ConvertAll(samples, sample => sample.Imaginary);
            columnTransform(real);
            columnTransform(imaginary);

            // Two-point sine transforms use the same butterfly, with forward
            // scale sin(PI / 3) = sqrt(3) / 2 and inverse scale 1 / sqrt(3).
            // A true 2D DHT is separable here specifically because the second
            // dimension is 2: its cross term contains sin(PI * integer) = 0.
            var result = new Complex[samples.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = new Complex(real[i] * scale + imaginary[i] * scale,
                    real[i] * scale - imaginary[i] * scale);
            return result;
        }

        private static void OrthonormalHartley(double[] samples)
        {
            if (samples.Length == 0) return;

            // For the negative-exponent DFT, cas(theta) = cos(theta) + sin(theta)
            // gives H(x) = (Re(FFT(x)) - Im(FFT(x))) / sqrt(N). Accord 3.8.0's
            // one-dimensional DHT instead takes the reciprocal of this result.
            var spectrum = Array.ConvertAll(samples, sample => new Complex(sample, 0));
            Fourier.Forward(spectrum, FourierOptions.NoScaling);
            var scale = 1 / Math.Sqrt(samples.Length);
            for (var i = 0; i < samples.Length; i++)
                samples[i] = (spectrum[i].Real - spectrum[i].Imaginary) * scale;
        }
    }
}
