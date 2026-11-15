// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation

namespace Computator.NET.Core.NumericalCalculations
{
    public static class Derivative
    {
        private const double EPS_MIN = 1e-25;
        private const double EPS_MAX = 1e-9;

        private static double StepScale(double x)
        {
            // Keep relative, sign-directed steps away from zero. A unit scale
            // near the origin avoids collapsing the default spacing with x:
            // evaluating exp(x) at x +/- 1e-25 can otherwise round to one.
            var scale = System.Math.Max(1.0, System.Math.Abs(x));
            return x < 0.0 ? -scale : scale;
        }

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
            var dx = StepScale(x)*eps;
            if (dx == 0)
                dx = EPS_MIN;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x))/dx;
            return (finiteDifferenceFormula(fx, x + dx, order - 1, eps) - finiteDifferenceFormula(fx, x, order - 1, eps))/dx;
        }

        public static double stableFiniteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            double dx;
            var h = System.Math.Sqrt(eps)*StepScale(x);
            if (h == 0.0)
                h = System.Math.Sqrt(EPS_MIN);

            var xph = x + h;

            dx = xph - x;

            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(xph) - fx(x))/dx;
            return (stableFiniteDifferenceFormula(fx, xph, order - 1, eps) - stableFiniteDifferenceFormula(fx, x, order - 1, eps))/
                   dx;
        }

        public static double twoPointfiniteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = StepScale(x)*eps;
            if (dx == 0)
                dx = EPS_MIN;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x - dx))/(2*dx);
            return (twoPointfiniteDifferenceFormula(fx, x + dx, order - 1, eps) -
                    twoPointfiniteDifferenceFormula(fx, x - dx, order - 1, eps))/(2*dx);
        }

        public static double centeredFivePointMethod(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = StepScale(x)*eps;
            if (dx == 0)
                dx = EPS_MIN;
            //[f(x-2h) - 8f(x-h) + 8f(x+h) - f(x+2h)] / 12h

            if (order == 0)
                return fx(x);
            if (order == 1)
                return FivePointDifference(fx(x - 2*dx), fx(x - dx), fx(x + dx), fx(x + 2*dx), dx);
            return FivePointDifference(centeredFivePointMethod(fx, x - 2*dx, order - 1, eps),
                centeredFivePointMethod(fx, x - dx, order - 1, eps),
                centeredFivePointMethod(fx, x + dx, order - 1, eps),
                centeredFivePointMethod(fx, x + 2*dx, order - 1, eps), dx);
        }

        private static double FivePointDifference(double left2, double left1, double right1, double right2, double step)
        {
            // Preserve the existing arithmetic for actual nonfinite inputs.
            if (!IsFinite(left2) || !IsFinite(left1) || !IsFinite(right1) || !IsFinite(right2) || !IsFinite(step))
                return (left2 - 8*left1 + 8*right1 - right2)/(12*step);
            if (left2 == left1 && left1 == right1 && right1 == right2)
                return 0.0/step;

            // Preserve ordinary cancellation order: regrouping every stencil
            // can erase a small residual which the original order retains.
            var numerator = left2 - 8*left1 + 8*right1 - right2;
            if (!IsFinite(numerator))
            {
                // Symmetric differences can cancel large common offsets before
                // weighting. Apply this only when the original path overflows.
                var inner = right1 - left1;
                var outer = left2 - right2;
                numerator = 8*inner + outer;
                if (!IsFinite(numerator))
                {
                    // Dividing by 32 bounds the full numerator below MaxValue.
                    // Retain already finite differences after cancellation.
                    var scaledInner = IsFinite(inner) ? inner/32 : right1/32 - left1/32;
                    var scaledOuter = IsFinite(outer) ? outer/32 : left2/32 - right2/32;
                    var scaledNumerator = 8*scaledInner + scaledOuter;
                    // Exact cancellation stays zero even when scaling a
                    // subnormal step would round the denominator to zero.
                    return scaledNumerator == 0 ? scaledNumerator/step : scaledNumerator/(0.375*step);
                }
            }

            var denominator = 12*step;
            // A large finite step can overflow only the denominator. In that
            // case divide by the step before the remaining factor.
            return double.IsInfinity(denominator) ? (numerator/step)/12 : numerator/denominator;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        public static double derivativeAtPoint(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            if (order == 0)
                return fx(x);

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
            if (order == 0)
                return fx(x);
            return MathNet.Numerics.Differentiate.Derivative(fx, x, (int)order);
        }

        public static System.Func<double,double> derivativeAsFunction(System.Func<double, double> fx, double x, uint order = 1)
        {
            if (order == 0)
                return fx;
            return MathNet.Numerics.Differentiate.DerivativeFunc(fx, (int)order);
        }

        public const string ToCode = @"
   public static class Derivative
    {
        private const double EPS_MIN = 1e-25;
        private const double EPS_MAX = 1e-9;

        private static double StepScale(double x)
        {
            // Keep relative, sign-directed steps away from zero. A unit scale
            // near the origin avoids collapsing the default spacing with x:
            // evaluating exp(x) at x +/- 1e-25 can otherwise round to one.
            var scale = System.Math.Max(1.0, System.Math.Abs(x));
            return x < 0.0 ? -scale : scale;
        }

        public static double derivative(string methodName, System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            switch (methodName)
            {
                case ""centered order-point method"":
                    return derivative(fx, x, order);

                case ""finite difference formula"":
                    return finiteDifferenceFormula(fx, x, order, eps);
                case ""stable finite difference formula"":
                    return stableFiniteDifferenceFormula(fx, x, order, eps);
                case ""two-point finite difference formula"":
                    return twoPointfiniteDifferenceFormula(fx, x, order, eps);
                case ""centered five-point method"":
                    return centeredFivePointMethod(fx, x, order, eps);
            }
            return double.NaN;
        }

        public static double finiteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = StepScale(x)*eps;
            if (dx == 0)
                dx = EPS_MIN;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x))/dx;
            return (finiteDifferenceFormula(fx, x + dx, order - 1, eps) - finiteDifferenceFormula(fx, x, order - 1, eps))/dx;
        }

        public static double stableFiniteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            double dx;
            var h = System.Math.Sqrt(eps)*StepScale(x);
            if (h == 0.0)
                h = System.Math.Sqrt(EPS_MIN);

            var xph = x + h;

            dx = xph - x;

            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(xph) - fx(x))/dx;
            return (stableFiniteDifferenceFormula(fx, xph, order - 1, eps) - stableFiniteDifferenceFormula(fx, x, order - 1, eps))/
                   dx;
        }

        public static double twoPointfiniteDifferenceFormula(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = StepScale(x)*eps;
            if (dx == 0)
                dx = EPS_MIN;
            if (order == 0)
                return fx(x);
            if (order == 1)
                return (fx(x + dx) - fx(x - dx))/(2*dx);
            return (twoPointfiniteDifferenceFormula(fx, x + dx, order - 1, eps) -
                    twoPointfiniteDifferenceFormula(fx, x - dx, order - 1, eps))/(2*dx);
        }

        public static double centeredFivePointMethod(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            var dx = StepScale(x)*eps;
            if (dx == 0)
                dx = EPS_MIN;
            //[f(x-2h) - 8f(x-h) + 8f(x+h) - f(x+2h)] / 12h

            if (order == 0)
                return fx(x);
            if (order == 1)
                return FivePointDifference(fx(x - 2*dx), fx(x - dx), fx(x + dx), fx(x + 2*dx), dx);
            return FivePointDifference(centeredFivePointMethod(fx, x - 2*dx, order - 1, eps),
                centeredFivePointMethod(fx, x - dx, order - 1, eps),
                centeredFivePointMethod(fx, x + dx, order - 1, eps),
                centeredFivePointMethod(fx, x + 2*dx, order - 1, eps), dx);
        }

        private static double FivePointDifference(double left2, double left1, double right1, double right2, double step)
        {
            // Preserve the existing arithmetic for actual nonfinite inputs.
            if (!IsFinite(left2) || !IsFinite(left1) || !IsFinite(right1) || !IsFinite(right2) || !IsFinite(step))
                return (left2 - 8*left1 + 8*right1 - right2)/(12*step);
            if (left2 == left1 && left1 == right1 && right1 == right2)
                return 0.0/step;

            // Preserve ordinary cancellation order: regrouping every stencil
            // can erase a small residual which the original order retains.
            var numerator = left2 - 8*left1 + 8*right1 - right2;
            if (!IsFinite(numerator))
            {
                // Symmetric differences can cancel large common offsets before
                // weighting. Apply this only when the original path overflows.
                var inner = right1 - left1;
                var outer = left2 - right2;
                numerator = 8*inner + outer;
                if (!IsFinite(numerator))
                {
                    // Dividing by 32 bounds the full numerator below MaxValue.
                    // Retain already finite differences after cancellation.
                    var scaledInner = IsFinite(inner) ? inner/32 : right1/32 - left1/32;
                    var scaledOuter = IsFinite(outer) ? outer/32 : left2/32 - right2/32;
                    var scaledNumerator = 8*scaledInner + scaledOuter;
                    // Exact cancellation stays zero even when scaling a
                    // subnormal step would round the denominator to zero.
                    return scaledNumerator == 0 ? scaledNumerator/step : scaledNumerator/(0.375*step);
                }
            }

            var denominator = 12*step;
            // A large finite step can overflow only the denominator. In that
            // case divide by the step before the remaining factor.
            return double.IsInfinity(denominator) ? (numerator/step)/12 : numerator/denominator;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
   public static double derivativeAtPoint(System.Func<double, double> fx, double x, uint order = 1, double eps = EPS_MAX)
        {
            if (order == 0)
                return fx(x);

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
            if (order == 0)
                return fx(x);
            return MathNet.Numerics.Differentiate.Derivative(fx, x, (int)order);
        }

        public static System.Func<double,double> derivativeAsFunction(System.Func<double, double> fx, double x, uint order = 1)
        {
            if (order == 0)
                return fx;
            return MathNet.Numerics.Differentiate.DerivativeFunc(fx, (int)order);
        }


}
        ";
    }
}