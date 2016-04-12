// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

namespace Computator.NET.NumericalCalculations
{
    public static class Derivative
    {
        private const double EPS_MIN = 1e-25;
        private const double EPS_MAX = 1e-9;

        public static double derivative(string methodName, System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            switch (methodName)
            {
                case "centered order-point method":
                    return derivative(fx, x, order);

                case "finite difference formula":
                    return finiteDifferenceFormula(fx, x, order, eps);
                case "stable finite difference formula":
                    return stableFiniteDifferenceFormula(fx, x, order, eps);
                case "two-point finite difference formula":
                    return twoPointfiniteDifferenceFormula(fx, x, order, eps);
                case "centered five-point method":
                    return centeredFivePointMethod(fx, x, order, eps);
            }
            return double.NaN;
        }

        public static double finiteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = x*eps;
            if (dx == 0)
                dx = EPS_MIN;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x))/dx;
            return (finiteDifferenceFormula(fx, x + dx, order - 1) - finiteDifferenceFormula(fx, x, order - 1))/dx;
        }

        public static double stableFiniteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            double dx;
            var h = System.Math.Sqrt(eps)*x;
            if (h == 0.0)
                h = System.Math.Sqrt(EPS_MIN);

            var xph = x + h;

            dx = xph - x;

            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(xph) - fx(x))/dx;
            return (stableFiniteDifferenceFormula(fx, xph, order - 1) - stableFiniteDifferenceFormula(fx, x, order - 1))/
                   dx;
        }

        public static double twoPointfiniteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = x*eps;
            if (dx == 0)
                dx = EPS_MIN;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x - dx))/(2*dx);
            return (twoPointfiniteDifferenceFormula(fx, x + dx, order - 1) -
                    twoPointfiniteDifferenceFormula(fx, x - dx, order - 1))/(2*dx);
        }

        public static double centeredFivePointMethod(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = x*eps;
            if (dx == 0)
                dx = EPS_MIN;
            //[f(x-2h) - 8f(x-h) + 8f(x+h) - f(x+2h)] / 12h

            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x - 2*dx) - 8*fx(x - dx) + 8*fx(x + dx) - fx(x + 2*dx))/(12*dx);
            return (centeredFivePointMethod(fx, x - 2*dx, order - 1) - 8*centeredFivePointMethod(fx, x - dx, order - 1) +
                    8*centeredFivePointMethod(fx, x + dx, order - 1) - centeredFivePointMethod(fx, x + 2*dx, order - 1))/
                   (12*dx);
        }

        public static double derivativeAtPoint(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            return MathNet.Numerics.Differentiate.Derivative(fx, x, (int)order);
            //if (order == 0)
            //    return fx(x);
            //if (order == 1)
            //    return (fx(x + double.Epsilon) - fx(x))/double.Epsilon;
            //return (derivativeAtPoint(fx, x + double.Epsilon, order - 1) - derivativeAtPoint(fx, x, order - 1))/
            //       double.Epsilon;
        }

        public static double derivative(System.Func<double, double> fx, double x, uint order = 1)
        {
            return MathNet.Numerics.Differentiate.Derivative(fx, x, (int)order);
        }

        public static System.Func<double,double> derivativeAsFunction(System.Func<double, double> fx, double x, uint order = 1)
        {
            return MathNet.Numerics.Differentiate.DerivativeFunc(fx, (int)order);
        }

        public const string ToCode = @"
    public static class Derivative
    {
        private const double EPS_MIN = 1e-25;
        private const double EPS_MAX = 1e-9;

        public static double finiteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = x*eps;
            if (dx == 0)
                dx = EPS_MIN;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x))/dx;
            return (finiteDifferenceFormula(fx, x + dx, order - 1) - finiteDifferenceFormula(fx, x, order - 1))/dx;
        }

        public static double stableFiniteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            double dx;
            var h = System.Math.Sqrt(eps)*x;
            if (h == 0.0)
                h = System.Math.Sqrt(EPS_MIN);

            var xph = x + h;

            dx = xph - x;

            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(xph) - fx(x))/dx;
            return (stableFiniteDifferenceFormula(fx, xph, order - 1) - stableFiniteDifferenceFormula(fx, x, order - 1))/
                   dx;
        }

        public static double twoPointfiniteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = x*eps;
            if (dx == 0)
                dx = EPS_MIN;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x - dx))/(2*dx);
            return (twoPointfiniteDifferenceFormula(fx, x + dx, order - 1) -
                    twoPointfiniteDifferenceFormula(fx, x - dx, order - 1))/(2*dx);
        }

        public static double centeredFivePointMethod(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = x*eps;
            if (dx == 0)
                dx = EPS_MIN;
            //[f(x-2h) - 8f(x-h) + 8f(x+h) - f(x+2h)] / 12h

            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x - 2*dx) - 8*fx(x - dx) + 8*fx(x + dx) - fx(x + 2*dx))/(12*dx);
            return (centeredFivePointMethod(fx, x - 2*dx, order - 1) - 8*centeredFivePointMethod(fx, x - dx, order - 1) +
                    8*centeredFivePointMethod(fx, x + dx, order - 1) - centeredFivePointMethod(fx, x + 2*dx, order - 1))/
                   (12*dx);
        }

        public static double derivativeAtPoint(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + double.Epsilon) - fx(x))/double.Epsilon;
            return (derivativeAtPoint(fx, x + double.Epsilon, order - 1) - derivativeAtPoint(fx, x, order - 1))/
                   double.Epsilon;
        }

        public static double derivative(System.Func<double, double> fx, double x, uint order = 1)
        {
            return MathNet.Numerics.Differentiate.Derivative(fx, x, (int)order);
        }

        public static System.Func<double,double> derivativeAsFunction(System.Func<double, double> fx, double x, uint order = 1)
        {
            return MathNet.Numerics.Differentiate.DerivativeFunc(fx, (int)order);
        }
}
        ";
    }
}