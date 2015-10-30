namespace Computator.NET.NumericalCalculations.MathematicalAnalysis
{
    internal class Integral
    {
        private const double N_MAX = 1e5;

        public static double monteCarloMethod(System.Func<double, double> fx, double a, double b, double N = N_MAX)
        {
            var parameters = new Accord.Math.Integration.MonteCarloIntegration(1);
            return Accord.Math.Integration.MonteCarloIntegration.Integrate(fx, a, b, (int) N*10);
        }

        public static double infiniteAdaptiveGaussKronrodMethod(System.Func<double, double> fx, double a, double b,
            double N = N_MAX)
        {
            return Accord.Math.Integration.InfiniteAdaptiveGaussKronrod.Integrate(fx, a, b, 1/N);
        }

        public static double nonAdaptiveGaussKronrodMethod(System.Func<double, double> fx, double a, double b,
            double N = N_MAX)
        {
            return Accord.Math.Integration.NonAdaptiveGaussKronrod.Integrate(fx, a, b, 1/N);
        }

        public static double rombergMethod(System.Func<double, double> fx, double a, double b, double N = N_MAX)
        {
            return Accord.Math.Integration.RombergMethod.Integrate(fx, a, b);
        }

        public static double doubleExponentialTransformation(System.Func<double, double> fx, double a, double b,
            double N = N_MAX)
        {
            return MathNet.Numerics.Integration.DoubleExponentialTransformation.Integrate(fx, a, b, 1/N);
        }

        public static double trapezoidalMethod(System.Func<double, double> fx, double a, double b, double N = N_MAX)
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

        public static double simpsonNumericalIntegration(System.Collections.Generic.List<System.Numerics.Complex> _x,
            int n)
        {
            var n2 = (n - 1)/2;
            int j1, j2;
            double sum = 0;
            for (var i = 0; i < n2; i++)
            {
                j1 = 2*i + 1; //[1,3,5...]
                j2 = 2*i + 2; //[2,4,6...]
                sum = sum + 4*_x[j1].Real + 2*_x[j2].Real;
            }
            //sum = ParallelEnumerable.Range(0, n2).Sum(i => 4 * _x[2 * i + 1].Real + 2 * _x[2 * i + 2].Real);
            return (n - 1)*(_x[0].Real + sum - _x[n - 1].Real)/(3*(n - 1));
        }

        public static double simpsonNumericalIntegration(System.Numerics.Complex[] _x, int n)
        {
            var n2 = (n - 1)/2;
            int j1, j2;
            double sum = 0;
            for (var i = 0; i < n2; i++)
            {
                j1 = 2*i + 1; //[1,3,5...]
                j2 = 2*i + 2; //[2,4,6...]
                sum = sum + 4*_x[j1].Real + 2*_x[j2].Real;
            }
            //sum = ParallelEnumerable.Range(0, n2).Sum(i => 4 * _x[2 * i + 1].Real + 2 * _x[2 * i + 2].Real);
            return (n - 1)*(_x[0].Real + sum - _x[n - 1].Real)/(3*(n - 1));
        }

        public static double rectangleMethod(System.Func<double, double> fx, double a, double b, double N = N_MAX)
        {
            var h = System.Math.Abs(b - a)/N;
            double s = 0, x;

            for (var i = 0; i < N; i++)
            {
                x = a + (i + 0.5)*h;
                ;
                s += fx(x);
            }
            return h*s;
        }

        public static double simpsonMethodOld(System.Func<double, double> fx, double a, double b, double N = N_MAX)
        {
            var h = System.Math.Abs(b - a)/N;

            var N2 = (((int) N) - 1)/2;

            double s = 0, x1, x2;
            int j1, j2;

            for (var i = 0; i < N2; i++)
            {
                j1 = 2*i + 1; //[1,3,5...]
                j2 = 2*i + 2; //[2,4,6...]
                x1 = a + j1*h;
                x2 = a + j2*h;

                s += 4*fx(x1) + 2*fx(x2);
            }
            return (fx(a) + s - fx(b))/(3*(N - 1));
        }

        public static double simpsonMethod(System.Func<double, double> fx, double a, double b, double N = N_MAX)
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
}