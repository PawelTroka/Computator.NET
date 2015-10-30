namespace Computator.NET.Transformations
{
    public static class MathematicalTransformations
    {
        public static double[] Transform(double[] functionPoints, string transformate)
        {
            var copyofFunctionsPoints = functionPoints;


            switch (transformate)
            {
                case "DST":
                    Accord.Math.SineTransform.DST(copyofFunctionsPoints);
                    break;

                case "IDST":
                    Accord.Math.SineTransform.IDST(copyofFunctionsPoints);
                    break;

                case "DCT":
                    Accord.Math.CosineTransform.DCT(copyofFunctionsPoints);
                    break;

                case "IDCT":
                    Accord.Math.CosineTransform.IDCT(copyofFunctionsPoints);
                    break;

                case "DHT":
                    Accord.Math.HartleyTransform.DHT(copyofFunctionsPoints);
                    break;

                case "FHT":
                    Accord.Math.HilbertTransform.FHT(copyofFunctionsPoints,
                        AForge.Math.FourierTransform.Direction.Forward);
                    break;

                case "IFHT":
                    Accord.Math.HilbertTransform.FHT(copyofFunctionsPoints,
                        AForge.Math.FourierTransform.Direction.Backward);
                    break;

                default:
                    throw new System.ArgumentException("Unknown transformation!");
            }
            return copyofFunctionsPoints; //athenia programuje//dididididi//di/kocham PaciA// JJKAKAKK  K
        }

        public static System.Numerics.Complex[] Transform(System.Numerics.Complex[] functionPoints, string transformate)
        {
            var copyofFunctionsPoints = functionPoints;


            var copyofFunctionsPoints2 = new System.Numerics.Complex[functionPoints.Length];
            var multidimensialArray = new double[functionPoints.Length, 2];

            var jaggedArray = new double[functionPoints.Length][];
            for (var i = 0; i < functionPoints.Length; i++)
                jaggedArray[i] = new double[2];


            for (var i = 0; i < functionPoints.Length; i++)
            {
                jaggedArray[i][0] = multidimensialArray[i, 0] = functionPoints[i].Real;
                jaggedArray[i][1] =
                    multidimensialArray[i, 1] = functionPoints[i].Imaginary;
                copyofFunctionsPoints2[i] = new System.Numerics.Complex(functionPoints[i].Real,
                    copyofFunctionsPoints2[i].Imaginary);
            }


            switch (transformate)
            {
                case "FFT":
                    MathNet.Numerics.IntegralTransforms.Fourier.Forward(copyofFunctionsPoints);
                    break;


                case "IFFT":
                    MathNet.Numerics.IntegralTransforms.Fourier.Inverse(copyofFunctionsPoints);
                    break;

                case "DST":
                    Accord.Math.SineTransform.DST(jaggedArray);
                    copyofFunctionsPoints = jaggedToComplex(jaggedArray);
                    break;

                case "IDST":
                    Accord.Math.SineTransform.IDST(jaggedArray);
                    copyofFunctionsPoints = jaggedToComplex(jaggedArray);
                    break;

                case "DCT":
                    Accord.Math.CosineTransform.DCT(multidimensialArray);
                    copyofFunctionsPoints = multidimensialToComplex(multidimensialArray);
                    break;

                case "IDCT":
                    Accord.Math.CosineTransform.IDCT(multidimensialArray);
                    copyofFunctionsPoints = multidimensialToComplex(multidimensialArray);
                    break;

                case "DHT":
                    Accord.Math.HartleyTransform.DHT(multidimensialArray);
                    copyofFunctionsPoints = multidimensialToComplex(multidimensialArray);
                    break;

                case "FHT":
                    Accord.Math.HilbertTransform.FHT(copyofFunctionsPoints2,
                        AForge.Math.FourierTransform.Direction.Forward);
                    copyofFunctionsPoints = (copyofFunctionsPoints2);
                    break;

                case "IFHT":
                    Accord.Math.HilbertTransform.FHT(copyofFunctionsPoints2,
                        AForge.Math.FourierTransform.Direction.Backward);
                    copyofFunctionsPoints = (copyofFunctionsPoints2);
                    break;

                default:
                    throw new System.ArgumentException("Unknown transformation!");
            }
            return copyofFunctionsPoints; //athenia programuje//dididididi//di/kocham PaciA// JJKAKAKK  K
        }

        private static System.Numerics.Complex[] jaggedToComplex(double[][] array)
        {
            var retArr = new System.Numerics.Complex[array.GetLength(0)];
            for (var i = 0; i < array.Length; i++)
                retArr[i] = new System.Numerics.Complex(array[i][0], array[i][1]);
            return retArr;
        }

        private static System.Numerics.Complex[] multidimensialToComplex(double[,] array)
        {
            var retArr = new System.Numerics.Complex[array.Length];
            for (var i = 0; i < array.Length; i++)
                retArr[i] = new System.Numerics.Complex(array[i, 0], array[i, 1]);
            return retArr;
        }
    }
}