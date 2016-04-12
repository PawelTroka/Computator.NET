// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation
namespace Computator.NET.NumericalCalculations
{
    public static class FunctionRoot
    {
        private const int ITERATIONS_MAX = 10000;
        private const double ERROR_MAX_MIN = 1e-8;


        public static double findRoot(string methodName, System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            switch (methodName)
            {
                case "bisection method":
                    return bisectionMethod(fx, a, b, errorMax, N);

                case "secant method":
                    return secantMethod(fx, a, b, errorMax, N);


                case "secant Newton Raphson method":
                    return secantNewtonRaphsonMethod(fx, a, b, errorMax, N);
                case "Brent's method":
                    return BrentMethod(fx, a, b, errorMax, N);
                case "Broyden's method":
                    return BroydenMethod(fx, a, b, errorMax, N);

            }
            return double.NaN;
        }

        public static double BrentMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Brent.TryFindRoot(fx, a, b, errorMax, N, out root) ? root : double.NaN;
        }

        public static double BroydenMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double[] root;

            System.Func<double[], double[]> function = (variables =>
            {
                var results = new double[variables.Length];

                for (var i = 0; i < variables.Length; i++)
                    results[i] = fx(variables[i]);

                return results;
            });

            return MathNet.Numerics.RootFinding.Broyden.TryFindRoot(function, new[] { (b + a) / 2 }, errorMax, N, out root) ? root[0] : double.NaN;
        }

        /*public static double BroydenMethod(Func<double, double, double> fxy, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double[] root;

            Func<double[], double[]> function = (variables =>
            {
                var results = new double[variables.Length];

                for (var i = 0; i < variables.Length; i++)
                    results[i] = fx(variables[i]);

                return results;
            });

            return Broyden.TryFindRoot(function, new[] { (b + a) / 2 }, errorMax, N, out root) ? root[0] : double.NaN;
        }*/

        public static double secantNewtonRaphsonMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Secant.TryFindRoot(fx, (a + b) / 2, (a + b) / 3, a, b, errorMax, N, out root) ? root : double.NaN;
        }

        public static double secantMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double x1 = a, x2 = b;

            for (var i = 0; i < N; i++)
            {
                if (x2 - x1 == 0.0 || (fx(x2) - fx(x1)) == 0.0 || fx(x2) == 0.0)
                    break;
                var xOld = x2;
                x2 = x2 - (fx(x2) * (x2 - x1)) / (fx(x2) - fx(x1));
                x1 = xOld;
            }

            return System.Math.Abs(fx(x2)) <= errorMax ? x2 : double.NaN;
        }

        public static double bisectionMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Bisection.TryFindRoot(fx, a, b, errorMax, N, out root) ? root : double.NaN;
        }

        public  const string ToCode = @"
    public static class FunctionRoot
    {
        private const int ITERATIONS_MAX = 100000;
        private const double ERROR_MAX_MIN=1e-8;

        public static double BrentMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Brent.TryFindRoot(fx, a, b, errorMax, N, out root) ? root : double.NaN;
        }

        public static double BroydenMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double[] root;

            System.Func<double[], double[]> function = (variables =>
            {
                var results = new double[variables.Length];

                for (var i = 0; i < variables.Length; i++)
                    results[i] = fx(variables[i]);

                return results;
            });

            return MathNet.Numerics.RootFinding.Broyden.TryFindRoot(function, new[] {(b + a)/2}, errorMax,  N, out root) ? root[0] : double.NaN;
        }

        /*public static double BroydenMethod(Func<double, double, double> fxy, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double[] root;

            Func<double[], double[]> function = (variables =>
            {
                var results = new double[variables.Length];

                for (var i = 0; i < variables.Length; i++)
                    results[i] = fx(variables[i]);

                return results;
            });

            return Broyden.TryFindRoot(function, new[] { (b + a) / 2 }, errorMax, N, out root) ? root[0] : double.NaN;
        }*/

        public static double secantNewtonRaphsonMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Secant.TryFindRoot(fx, (a + b)/2, (a + b)/3, a, b, errorMax, N, out root) ? root : double.NaN;
        }

        public static double secantMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double x1 = a, x2 = b;

            for (var i = 0; i < N; i++)
            {
                if (x2 - x1 == 0.0 || (fx(x2) - fx(x1)) == 0.0 || fx(x2) == 0.0)
                    break;
                var xOld = x2;
                x2 = x2 - (fx(x2)*(x2 - x1))/(fx(x2) - fx(x1));
                x1 = xOld;
            }

            return System.Math.Abs(fx(x2)) <= errorMax ? x2 : double.NaN;
        }

        public static double bisectionMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, int N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Bisection.TryFindRoot(fx, a, b, errorMax, N, out root) ? root : double.NaN;
        }
}
";
    }
}