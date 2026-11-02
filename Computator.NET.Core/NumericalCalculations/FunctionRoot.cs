#if NETFRAMEWORK
using RuntimeMath = Computator.NET.Engine.Compatibility.FrameworkMath;
#else
using RuntimeMath = System.Math;
#endif

// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation
namespace Computator.NET.Core.NumericalCalculations
{
    public static class FunctionRoot
    {
        private const uint ITERATIONS_MAX = 10000;
        private const double ERROR_MAX_MIN = 1e-8;


        public static double findRoot(string methodName, System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
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

        public static double BrentMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Brent.TryFindRoot(fx, a, b, errorMax, (int)N, out root) ? root : double.NaN;
        }

        public static double BroydenMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double[] root;

            System.Func<double[], double[]> function = (variables =>
            {
                var results = new double[variables.Length];

                for (var i = 0; i < variables.Length; i++)
                    results[i] = fx(variables[i]);

                return results;
            });

            // Keep the direct sum for cancellation and subnormal precision.
            // If it overflows, halving these large operands first is exact.
            var sum = a + b;
            var initialGuess = double.IsInfinity(sum) ? a / 2 + b / 2 : sum / 2;
            return MathNet.Numerics.RootFinding.Broyden.TryFindRoot(function, new[] { initialGuess }, errorMax, (int)N, out root) ? root[0] : double.NaN;
        }

        /*public static double BroydenMethod(Func<double, double, double> fxy, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
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

        public static double secantNewtonRaphsonMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double root;

            // Select interior points relative to the interval, without overflowing
            // either the endpoint sum or the width of a large interval across zero.
            var width = b - a;
            var firstGuess = double.IsInfinity(width) ? a / 2 + b / 2 : a + width / 2;
            var thirdWidth = double.IsInfinity(width) ? b / 3 - a / 3 : width / 3;
            var secondGuess = a + thirdWidth;

            // Rounding can merge the guesses in a very narrow interval. If there
            // is only one interior value, Math.NET rejects the endpoint reached
            // here before evaluating fx, while preserving its argument validation.
            if (secondGuess == firstGuess)
            {
                secondGuess = RuntimeMath.BitIncrement(firstGuess);
                if (secondGuess >= b)
                    secondGuess = RuntimeMath.BitDecrement(firstGuess);
            }

            return MathNet.Numerics.RootFinding.Secant.TryFindRoot(fx, firstGuess, secondGuess, a, b, errorMax, (int)N, out root) ? root : double.NaN;
        }

        public static double secantMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double x1 = a, x2 = b;
            if (double.IsNaN(x2) || double.IsInfinity(x2))
                return double.NaN;
            var f2 = fx(x2);
            if (double.IsNaN(f2) || double.IsInfinity(f2))
                return double.NaN;

            // With no steps or coincident guesses only b is sampled, as before.
            // An exact root also needs no second point or repeated evaluation.
            if (N == 0 || x1 == x2 || f2 == 0)
                return System.Math.Abs(f2) <= errorMax ? x2 : double.NaN;
            if (double.IsNaN(x1) || double.IsInfinity(x1))
                return double.NaN;
            var f1 = fx(x1);

            for (uint i = 0; i < N; i++)
            {
                if (double.IsNaN(f1) || double.IsInfinity(f1))
                    return double.NaN;
                if (f1 == 0)
                    return System.Math.Abs(f1) <= errorMax ? x1 : double.NaN;
                if (f1 == f2)
                    break;

                var next = SecantEstimate(x1, f1, x2, f2);
                if (double.IsNaN(next) || double.IsInfinity(next))
                    return double.NaN;
                if (next == x2)
                    break;

                x1 = x2;
                f1 = f2;
                x2 = next;
                f2 = fx(x2);
                if (double.IsNaN(f2) || double.IsInfinity(f2))
                    return double.NaN;
                if (f2 == 0)
                    break;
            }

            return System.Math.Abs(f2) <= errorMax ? x2 : double.NaN;
        }

        private static double SecantEstimate(double x1, double f1, double x2, double f2)
        {
            // Start at the smaller residual and divide before multiplying by
            // distance. Subtract finite nearby residuals before division so
            // their difference is not lost by rounding a ratio towards one.
            if (System.Math.Abs(f1) > System.Math.Abs(f2))
                return SecantEstimate(x2, f2, x1, f1);
            var difference = f1 - f2;
            double weight;
            if (double.IsInfinity(difference))
            {
                var ratio = f1 / f2;
                weight = ratio / (ratio - 1);
            }
            else
                weight = f1 / difference;
            var width = x2 - x1;

            // An overflowing distance requires opposite-sign finite endpoints.
            // Weighted endpoints retain finite interpolation/extrapolation here.
            if (double.IsInfinity(width))
                return (1 - weight) * x1 + weight * x2;

            double offset;
            if (System.Math.Abs(weight) < 2.2250738585072014e-308)
            {
                // A subnormal/zero weight can lose a representable small root
                // when multiplied by a large distance. In this case the
                // residual difference is finite; change the operation order.
                var product = f1 * width;
                offset = product != 0 && !double.IsInfinity(product)
                    ? product / difference
                    : f1 * (width / difference);
            }
            else
                offset = weight * width;

            // A finite extrapolated root can be over MaxValue away from x1.
            // Halve before adding so that this cancelling offset stays finite.
            if (double.IsInfinity(offset))
                return (x1 / 2 + weight * (width / 2)) * 2;
            return x1 + offset;
        }

        public static double bisectionMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Bisection.TryFindRoot(fx, a, b, errorMax, (int)N, out root) ? root : double.NaN;
        }

        public  const string ToCode = @"
    public static class FunctionRoot
    {
        private const uint ITERATIONS_MAX = 10000;
        private const double ERROR_MAX_MIN = 1e-8;


        public static double findRoot(string methodName, System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            switch (methodName)
            {
                case ""bisection method"":
                    return bisectionMethod(fx, a, b, errorMax, N);

                case ""secant method"":
                    return secantMethod(fx, a, b, errorMax, N);


                case ""secant Newton Raphson method"":
                    return secantNewtonRaphsonMethod(fx, a, b, errorMax, N);
                case ""Brent's method"":
                    return BrentMethod(fx, a, b, errorMax, N);
                case ""Broyden's method"":
                    return BroydenMethod(fx, a, b, errorMax, N);

            }
            return double.NaN;
        }

        public static double BrentMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Brent.TryFindRoot(fx, a, b, errorMax, (int)N, out root) ? root : double.NaN;
        }

        public static double BroydenMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double[] root;

            System.Func<double[], double[]> function = (variables =>
            {
                var results = new double[variables.Length];

                for (var i = 0; i < variables.Length; i++)
                    results[i] = fx(variables[i]);

                return results;
            });

            // Keep the direct sum for cancellation and subnormal precision.
            // If it overflows, halving these large operands first is exact.
            var sum = a + b;
            var initialGuess = double.IsInfinity(sum) ? a / 2 + b / 2 : sum / 2;
            return MathNet.Numerics.RootFinding.Broyden.TryFindRoot(function, new[] { initialGuess }, errorMax, (int)N, out root) ? root[0] : double.NaN;
        }

        /*public static double BroydenMethod(Func<double, double, double> fxy, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
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

        public static double secantNewtonRaphsonMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double root;

            // Select interior points relative to the interval, without overflowing
            // either the endpoint sum or the width of a large interval across zero.
            var width = b - a;
            var firstGuess = double.IsInfinity(width) ? a / 2 + b / 2 : a + width / 2;
            var thirdWidth = double.IsInfinity(width) ? b / 3 - a / 3 : width / 3;
            var secondGuess = a + thirdWidth;

            // Rounding can merge the guesses in a very narrow interval. If there
            // is only one interior value, Math.NET rejects the endpoint reached
            // here before evaluating fx, while preserving its argument validation.
            if (secondGuess == firstGuess)
            {
                secondGuess = Computator.NET.Engine.Compatibility.FrameworkMath.BitIncrement(firstGuess);
                if (secondGuess >= b)
                    secondGuess = Computator.NET.Engine.Compatibility.FrameworkMath.BitDecrement(firstGuess);
            }

            return MathNet.Numerics.RootFinding.Secant.TryFindRoot(fx, firstGuess, secondGuess, a, b, errorMax, (int)N, out root) ? root : double.NaN;
        }

        public static double secantMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double x1 = a, x2 = b;
            if (double.IsNaN(x2) || double.IsInfinity(x2))
                return double.NaN;
            var f2 = fx(x2);
            if (double.IsNaN(f2) || double.IsInfinity(f2))
                return double.NaN;

            // With no steps or coincident guesses only b is sampled, as before.
            // An exact root also needs no second point or repeated evaluation.
            if (N == 0 || x1 == x2 || f2 == 0)
                return System.Math.Abs(f2) <= errorMax ? x2 : double.NaN;
            if (double.IsNaN(x1) || double.IsInfinity(x1))
                return double.NaN;
            var f1 = fx(x1);

            for (uint i = 0; i < N; i++)
            {
                if (double.IsNaN(f1) || double.IsInfinity(f1))
                    return double.NaN;
                if (f1 == 0)
                    return System.Math.Abs(f1) <= errorMax ? x1 : double.NaN;
                if (f1 == f2)
                    break;

                var next = SecantEstimate(x1, f1, x2, f2);
                if (double.IsNaN(next) || double.IsInfinity(next))
                    return double.NaN;
                if (next == x2)
                    break;

                x1 = x2;
                f1 = f2;
                x2 = next;
                f2 = fx(x2);
                if (double.IsNaN(f2) || double.IsInfinity(f2))
                    return double.NaN;
                if (f2 == 0)
                    break;
            }

            return System.Math.Abs(f2) <= errorMax ? x2 : double.NaN;
        }

        private static double SecantEstimate(double x1, double f1, double x2, double f2)
        {
            // Start at the smaller residual and divide before multiplying by
            // distance. Subtract finite nearby residuals before division so
            // their difference is not lost by rounding a ratio towards one.
            if (System.Math.Abs(f1) > System.Math.Abs(f2))
                return SecantEstimate(x2, f2, x1, f1);
            var difference = f1 - f2;
            double weight;
            if (double.IsInfinity(difference))
            {
                var ratio = f1 / f2;
                weight = ratio / (ratio - 1);
            }
            else
                weight = f1 / difference;
            var width = x2 - x1;

            // An overflowing distance requires opposite-sign finite endpoints.
            // Weighted endpoints retain finite interpolation/extrapolation here.
            if (double.IsInfinity(width))
                return (1 - weight) * x1 + weight * x2;

            double offset;
            if (System.Math.Abs(weight) < 2.2250738585072014e-308)
            {
                // A subnormal/zero weight can lose a representable small root
                // when multiplied by a large distance. In this case the
                // residual difference is finite; change the operation order.
                var product = f1 * width;
                offset = product != 0 && !double.IsInfinity(product)
                    ? product / difference
                    : f1 * (width / difference);
            }
            else
                offset = weight * width;

            // A finite extrapolated root can be over MaxValue away from x1.
            // Halve before adding so that this cancelling offset stays finite.
            if (double.IsInfinity(offset))
                return (x1 / 2 + weight * (width / 2)) * 2;
            return x1 + offset;
        }

        public static double bisectionMethod(System.Func<double, double> fx, double a, double b, double errorMax = ERROR_MAX_MIN, uint N = ITERATIONS_MAX)
        {
            double root;

            return MathNet.Numerics.RootFinding.Bisection.TryFindRoot(fx, a, b, errorMax, (int)N, out root) ? root : double.NaN;
        }
}
";
    }
}
