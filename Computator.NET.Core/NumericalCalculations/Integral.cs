// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

// ReSharper disable LocalizableElement
namespace Computator.NET.Core.NumericalCalculations
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

        public static double monteCarloMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            // Accord 3.8's scalar overload uses Random.Next() for coordinates
            // and increments its sample counter twice. The multidimensional
            // overload samples uniformly and counts each evaluation once.
            return Accord.Math.Integration.MonteCarloIntegration.Integrate(
                point => fx(point[0]), new[] { a }, new[] { b }, (int)N);
        }

        public static double infiniteAdaptiveGaussKronrodMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var iagk = new Accord.Math.Integration.InfiniteAdaptiveGaussKronrod((int) N, fx, a, b);
            //iagk.ToleranceAbsolute = iagk.ToleranceRelative = 0;//?????????????????????????????????????????????????????
            return iagk.Compute() ? iagk.Area : double.NaN;
        }

        public static double nonAdaptiveGaussKronrodMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            return Accord.Math.Integration.NonAdaptiveGaussKronrod.Integrate(fx, a, b, 1.0/N);
        }

        public static double rombergMethod(System.Func<double, double> fx, double a, double b, double N = 6)
        {
            if(N>30)
                throw new System.ArgumentException("Number of steps should be small for rombergMethod, default is 6", "N");
            return Accord.Math.Integration.RombergMethod.Integrate(fx, a, b, (int) N);
        }

        public static double doubleExponentialTransformation(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            return MathNet.Numerics.Integration.DoubleExponentialTransformation.Integrate(fx, a, b, 1.0/N);
        }

        private static int CompositePanelCount(double N, bool requireEven)
        {
            // Keep every loop increment in Int32 range, including Simpson's
            // existing adjustment of an odd count to the next even count.
            var maximum = requireEven ? int.MaxValue - 1 : int.MaxValue;
            if (N < 1 || N > maximum || N != System.Math.Truncate(N))
                throw new System.ArgumentOutOfRangeException(nameof(N), N,
                    "Panel count must be a positive integer no greater than " + maximum + ".");

            var panels = (int)N;
            return requireEven && panels % 2 != 0 ? panels + 1 : panels;
        }

        public static double trapezoidalMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var panels = CompositePanelCount(N, requireEven: false);
            var h = (b - a)/panels;
            double s = 0, x1, x2;

            for (var i = 0; i < panels; i++)
            {
                x1 = a + i*h;
                x2 = a + (i + 1)*h;
                var y2 = fx(x2);
                var y1 = fx(x1);
                var sum = y2 + y1;
                // Two finite endpoints can overflow their sum although their
                // average and panel area are representable. Halving these
                // large values is safe; keep the ordinary path for subnormals.
                if (double.IsInfinity(sum) && !double.IsInfinity(y2) && !double.IsInfinity(y1))
                    s += h*(y2/2 + y1/2);
                else if (System.Math.Abs(h) < 2.2250738585072014e-308 && !double.IsInfinity(sum))
                    // Halving a subnormal width can erase or round it before
                    // a large finite sum produces a representable panel area.
                    s += h*(sum/2);
                else
                    s += 0.5*h*sum;
            }
            return s;
        }

        public static double rectangleMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var panels = CompositePanelCount(N, requireEven: false);
            var h = (b - a)/panels;
            double s = 0, x;

            for (var i = 0; i < panels; i++)
            {
                x = a + (i + 0.5)*h;
                s += fx(x);
            }
            return h*s;
        }

        public static double simpsonMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var panels = CompositePanelCount(N, requireEven: true);

            var h = (b - a)/panels;
            double s = 0, x;

            for (var i = 1; i < panels; i++)
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
            // Use the corrected path in emitted/precompiled TSL as well.
            return Accord.Math.Integration.MonteCarloIntegration.Integrate(
                point => fx(point[0]), new[] { a }, new[] { b }, N);
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

        private static int CompositePanelCount(double N, bool requireEven)
        {
            // Keep every loop increment in Int32 range, including Simpson's
            // existing adjustment of an odd count to the next even count.
            var maximum = requireEven ? int.MaxValue - 1 : int.MaxValue;
            if (N < 1 || N > maximum || N != System.Math.Truncate(N))
                throw new System.ArgumentOutOfRangeException(nameof(N), N,
                    ""Panel count must be a positive integer no greater than "" + maximum + ""."");

            var panels = (int)N;
            return requireEven && panels % 2 != 0 ? panels + 1 : panels;
        }

        public static double trapezoidalMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var panels = CompositePanelCount(N, requireEven: false);
            var h = (b - a)/panels;
            double s = 0, x1, x2;

            for (var i = 0; i < panels; i++)
            {
                x1 = a + i*h;
                x2 = a + (i + 1)*h;
                var y2 = fx(x2);
                var y1 = fx(x1);
                var sum = y2 + y1;
                // Two finite endpoints can overflow their sum although their
                // average and panel area are representable. Halving these
                // large values is safe; keep the ordinary path for subnormals.
                if (double.IsInfinity(sum) && !double.IsInfinity(y2) && !double.IsInfinity(y1))
                    s += h*(y2/2 + y1/2);
                else if (System.Math.Abs(h) < 2.2250738585072014e-308 && !double.IsInfinity(sum))
                    // Halving a subnormal width can erase or round it before
                    // a large finite sum produces a representable panel area.
                    s += h*(sum/2);
                else
                    s += 0.5*h*sum;
            }
            return s;
        }

        public static double rectangleMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var panels = CompositePanelCount(N, requireEven: false);
            var h = (b - a)/panels;
            double s = 0, x;

            for (var i = 0; i < panels; i++)
            {
                x = a + (i + 0.5)*h;
                s += fx(x);
            }
            return h*s;
        }

        public static double simpsonMethod(System.Func<double, double> fx, double a, double b, double N = STEPS_MAX)
        {
            var panels = CompositePanelCount(N, requireEven: true);

            var h = (b - a)/panels;
            double s = 0, x;

            for (var i = 1; i < panels; i++)
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
