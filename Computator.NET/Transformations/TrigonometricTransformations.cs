using System;
using Accord.Math;
using AForge.Math;
using MathNet.Numerics.IntegralTransforms;
using Complex = System.Numerics.Complex;

namespace Computator.NET.Transformations
{
    public static class MathematicalTransformations
    {
        public static double[] Transform(double[] functionPoints, string transformate)
        {
            var copyofFunctionsPoints = new double[functionPoints.Length];
            functionPoints.CopyTo(copyofFunctionsPoints, 0);


            switch (transformate)
            {
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
                    HartleyTransform.DHT(copyofFunctionsPoints);
                    break;

                case "FHT":
                    HilbertTransform.FHT(copyofFunctionsPoints, FourierTransform.Direction.Forward);
                    break;

                case "IFHT":
                    HilbertTransform.FHT(copyofFunctionsPoints, FourierTransform.Direction.Backward);
                    break;

                default:
                    throw new ArgumentException("Unknown transformation!");
            }
            return copyofFunctionsPoints; //athenia programuje//dididididi//di/kocham PaciA// JJKAKAKK  K
        }

        public static Complex[] Transform(Complex[] functionPoints, string transformate)
        {
            var copyofFunctionsPoints = new Complex[functionPoints.Length];
            functionPoints.CopyTo(copyofFunctionsPoints, 0);


            var copyofFunctionsPoints2 = new AForge.Math.Complex[functionPoints.Length];
            var multidimensialArray = new double[functionPoints.Length, 2];

            var jaggedArray = new double[functionPoints.Length][];
            for (int i = 0; i < functionPoints.Length; i++)
                jaggedArray[i] = new double[2];


            for (int i = 0; i < functionPoints.Length; i++)
            {
                jaggedArray[i][0] = multidimensialArray[i, 0] = copyofFunctionsPoints2[i].Re = functionPoints[i].Real;
                jaggedArray[i][1] =
                    multidimensialArray[i, 1] = copyofFunctionsPoints2[i].Im = functionPoints[i].Imaginary;
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
                    SineTransform.DST(jaggedArray);
                    copyofFunctionsPoints = jaggedToComplex(jaggedArray);
                    break;

                case "IDST":
                    SineTransform.IDST(jaggedArray);
                    copyofFunctionsPoints = jaggedToComplex(jaggedArray);
                    break;

                case "DCT":
                    CosineTransform.DCT(multidimensialArray);
                    copyofFunctionsPoints = multidimensialToComplex(multidimensialArray);
                    break;

                case "IDCT":
                    CosineTransform.IDCT(multidimensialArray);
                    copyofFunctionsPoints = multidimensialToComplex(multidimensialArray);
                    break;

                case "DHT":
                    HartleyTransform.DHT(multidimensialArray);
                    copyofFunctionsPoints = multidimensialToComplex(multidimensialArray);
                    break;

                case "FHT":
                    HilbertTransform.FHT(copyofFunctionsPoints2, FourierTransform.Direction.Forward);
                    copyofFunctionsPoints = aforgeComplexToComplex(copyofFunctionsPoints2);
                    break;

                case "IFHT":
                    HilbertTransform.FHT(copyofFunctionsPoints2, FourierTransform.Direction.Backward);
                    copyofFunctionsPoints = aforgeComplexToComplex(copyofFunctionsPoints2);
                    break;

                default:
                    throw new ArgumentException("Unknown transformation!");
            }
            return copyofFunctionsPoints; //athenia programuje//dididididi//di/kocham PaciA// JJKAKAKK  K
        }

        private static Complex[] jaggedToComplex(double[][] array)
        {
            var retArr = new Complex[array.GetLength(0)];
            for (int i = 0; i < array.Length; i++)
                retArr[i] = new Complex(array[i][0], array[i][1]);
            return retArr;
        }

        private static Complex[] multidimensialToComplex(double[,] array)
        {
            var retArr = new Complex[array.Length];
            for (int i = 0; i < array.Length; i++)
                retArr[i] = new Complex(array[i, 0], array[i, 1]);
            return retArr;
        }

        private static Complex[] aforgeComplexToComplex(AForge.Math.Complex[] array)
        {
            var retArr = new Complex[array.Length];
            for (int i = 0; i < array.Length; i++)
                retArr[i] = new Complex(array[i].Re, array[i].Im);
            return retArr;
        }
    }
}