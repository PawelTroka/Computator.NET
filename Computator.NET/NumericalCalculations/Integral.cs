// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

// ReSharper disable LocalizableElement
namespace Computator.NET.NumericalCalculations
{
    public static class Integral
    {
        private const int STEPS_MAX = 10000;


        public static double integrate(string methodName, System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            switch (methodName)
            {
                case "trapezoidal method":

                    return trapezoidalMethod(fx, a, b, N);
                case "rectangle method":
                    return rectangleMethod(fx, a, b, N);
                case "Simpson's method":

                    return simpsonMethod(fx, a, b, N);
                case "double exponential transformation":

                    return doubleExponentialTransformation(fx, a, b, N);

                case "non-adaptive Gauss–Kronrod method":

                    return nonAdaptiveGaussKronrodMethod(fx, a, b, N);

                case "infinity-adaptive Gauss–Kronrod method":
                    return infiniteAdaptiveGaussKronrodMethod(fx, a, b, N);

                case "Monte Carlo method":
                    return monteCarloMethod(fx, a, b, N);
                case "Romberg's method":
                    return rombergMethod(fx, a, b, N);
            }

            return double.NaN;
        }

        public static double monteCarloMethod(System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            return Accord.Math.Integration.MonteCarloIntegration.Integrate(fx, a, b, N);
        }

        public static double infiniteAdaptiveGaussKronrodMethod(System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            var iagk = new Accord.Math.Integration.InfiniteAdaptiveGaussKronrod(N, fx, a, b);
            //iagk.ToleranceAbsolute = iagk.ToleranceRelative = 0;//?????????????????????????????????????????????????????
            return iagk.Compute() ? iagk.Area : double.NaN;
        }

        public static double nonAdaptiveGaussKronrodMethod(System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            return Accord.Math.Integration.NonAdaptiveGaussKronrod.Integrate(fx, a, b, 1.0/N);
        }

        public static double rombergMethod(System.Func<double, double> fx, double a, double b, int N = 6)
        {
            if(N>30)
                throw new System.ArgumentException("Number of steps should be small for rombergMethod, default is 6", "N");
            return Accord.Math.Integration.RombergMethod.Integrate(fx, a, b, N);
        }

        public static double doubleExponentialTransformation(System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            return MathNet.Numerics.Integration.DoubleExponentialTransformation.Integrate(fx, a, b, 1.0/N);
        }

        public static double trapezoidalMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var h = System.Math.Abs(b - a)/N;
            double s = 0, x1, x2;

            for (var i = 0; i < N; i++)
            {
                x1 = a + i*h;
                x2 = a + (i + 1)*h;
                s += 0.5*h*(fx(x2) + fx(x1));
            }
            return s;
        }

        public static double rectangleMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var h = System.Math.Abs(b - a)/N;
            double s = 0, x;

            for (var i = 0; i < N; i++)
            {
                x = a + (i + 0.5)*h;
                s += fx(x);
            }
            return h*s;
        }

        public static double simpsonMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            if (N%2 != 0) //not even
                N++;

            var h = System.Math.Abs(b - a)/N;
            double s = 0, x;

            for (var i = 1; i < N; i++)
            {
                x = a + i*h;

                if (i%2 == 1)
                    s += 4*fx(x);
                else
                    s += 2*fx(x);
            }

            return h*(fx(a) + s + fx(b))/3.0;
        }

        public const string ToCode = @"
    public static class Integral
    {
        private const int STEPS_MAX = 100000;

        public static double monteCarloMethod(System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            return Accord.Math.Integration.MonteCarloIntegration.Integrate(fx, a, b, N);
        }

        public static double infiniteAdaptiveGaussKronrodMethod(System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            var iagk = new Accord.Math.Integration.InfiniteAdaptiveGaussKronrod(N, fx, a, b);
            //iagk.ToleranceAbsolute = iagk.ToleranceRelative = 0;//?????????????????????????????????????????????????????
            return iagk.Compute() ? iagk.Area : double.NaN;
        }

        public static double nonAdaptiveGaussKronrodMethod(System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            return Accord.Math.Integration.NonAdaptiveGaussKronrod.Integrate(fx, a, b, 1.0/N);
        }

        public static double rombergMethod(System.Func<double, double> fx, double a, double b, int N = 6)
        {
            if(N>30)
                throw new System.ArgumentException(""Number of steps should be small for rombergMethod, default is 6"", ""N"");
            return Accord.Math.Integration.RombergMethod.Integrate(fx, a, b, N);
        }

        public static double doubleExponentialTransformation(System.Func<double, double> fx, double a, double b, int N = STEPS_MAX)
        {
            return MathNet.Numerics.Integration.DoubleExponentialTransformation.Integrate(fx, a, b, 1.0/N);
        }

        public static double trapezoidalMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var h = System.Math.Abs(b - a)/N;
            double s = 0, x1, x2;

            for (var i = 0; i < N; i++)
            {
                x1 = a + i*h;
                x2 = a + (i + 1)*h;
                s += 0.5*h*(fx(x2) + fx(x1));
            }
            return s;
        }

        public static double rectangleMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var h = System.Math.Abs(b - a)/N;
            double s = 0, x;

            for (var i = 0; i < N; i++)
            {
                x = a + (i + 0.5)*h;
                s += fx(x);
            }
            return h*s;
        }

        public static double simpsonMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            if (N%2 != 0) //not even
                N++;

            var h = System.Math.Abs(b - a)/N;
            double s = 0, x;

            for (var i = 1; i < N; i++)
            {
                x = a + i*h;

                if (i%2 == 1)
                    s += 4*fx(x);
                else
                    s += 2*fx(x);
            }

            return h*(fx(a) + s + fx(b))/3.0;
        }    
  
    }   
";
    }
}