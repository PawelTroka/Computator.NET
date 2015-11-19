using System;

namespace Computator.NET.NumericalCalculations.MathematicalAnalysis
{
    internal class Derivative
    {
        private const double EPS_MAX = 1e-25;
        public static double EPS = 1e-9;

        public static double finiteDifferenceFormula(Func<double, double> fx, double x, uint order = 1)
        {
            var dx = x*EPS;
            if (dx == 0)
                dx = EPS_MAX;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x))/dx;
            return (finiteDifferenceFormula(fx, x + dx, order - 1) - finiteDifferenceFormula(fx, x, order - 1))/dx;
        }

        public static double stableFiniteDifferenceFormula(Func<double, double> fx, double x, uint order = 1)
        {
            double dx;
            var h = Math.Sqrt(EPS)*x;
            if (h == 0.0)
                h = Math.Sqrt(EPS_MAX);

            var xph = x + h;

            dx = xph - x;

            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(xph) - fx(x))/dx;
            return (stableFiniteDifferenceFormula(fx, xph, order - 1) - stableFiniteDifferenceFormula(fx, x, order - 1))/
                   dx;
        }

        public static double twoPointfiniteDifferenceFormula(Func<double, double> fx, double x, uint order = 1)
        {
            var dx = x*EPS;
            if (dx == 0)
                dx = EPS_MAX;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x - dx))/(2*dx);
            return (twoPointfiniteDifferenceFormula(fx, x + dx, order - 1) -
                    twoPointfiniteDifferenceFormula(fx, x - dx, order - 1))/(2*dx);
        }

        public static double centeredFivePointMethod(Func<double, double> fx, double x, uint order = 1)
        {
            var dx = x*EPS;
            if (dx == 0)
                dx = EPS_MAX;
            //[f(x-2h) - 8f(x-h) + 8f(x+h) - f(x+2h)] / 12h

            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x - 2*dx) - 8*fx(x - dx) + 8*fx(x + dx) - fx(x + 2*dx))/(12*dx);
            return (centeredFivePointMethod(fx, x - 2*dx, order - 1) - 8*centeredFivePointMethod(fx, x - dx, order - 1) +
                    8*centeredFivePointMethod(fx, x + dx, order - 1) - centeredFivePointMethod(fx, x + 2*dx, order - 1))/
                   (12*dx);
        }

        public static double derivativeAtPoint(Func<double, double> f, double x, uint order = 1)
        {
            if (order == 0)
                return f(x);
            if (order == 1)
                return (f(x + double.Epsilon) - f(x))/double.Epsilon;
            return (derivativeAtPoint(f, x + double.Epsilon, order - 1) - derivativeAtPoint(f, x, order - 1))/
                   double.Epsilon;
        }
    }
}