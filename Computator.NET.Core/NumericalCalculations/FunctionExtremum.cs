using System;
using Accord.Math.Optimization;

namespace Computator.NET.Core.NumericalCalculations
{
    /// <summary>Bounded, derivative-free searches for a real function's extremum location.</summary>
    public static class FunctionExtremum
    {
        /// <summary>
        /// Return the x coordinate minimizing fx on [a,b], including the endpoints.
        /// Use a continuous unimodal function; this is not a global search for arbitrary functions.
        /// </summary>
        public static double minimize(Func<double, double> fx, double a, double b,
            double tolerance = 1e-8, int iterations = 1000)
        {
            return Search(fx, a, b, tolerance, iterations, false);
        }

        /// <summary>
        /// Return the x coordinate maximizing fx on [a,b], including the endpoints.
        /// Use a continuous unimodal function; this is not a global search for arbitrary functions.
        /// </summary>
        public static double maximize(Func<double, double> fx, double a, double b,
            double tolerance = 1e-8, int iterations = 1000)
        {
            return Search(fx, a, b, tolerance, iterations, true);
        }

        private static double Search(Func<double, double> fx, double a, double b,
            double tolerance, int iterations, bool maximize)
        {
            if (fx == null) throw new ArgumentNullException(nameof(fx));
            if (!IsFinite(a)) throw new ArgumentOutOfRangeException(nameof(a), "The lower bound must be finite.");
            if (!IsFinite(b)) throw new ArgumentOutOfRangeException(nameof(b), "The upper bound must be finite.");
            if (a > b || !IsFinite(b - a))
                throw new ArgumentException("Use ordered bounds with a finite width; rescale the coordinate if needed.");
            if (!IsFinite(tolerance) || tolerance <= 0)
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Use a finite positive coordinate tolerance.");
            if (iterations < 1 || iterations > 1000000)
                throw new ArgumentOutOfRangeException(nameof(iterations), "Use between 1 and 1,000,000 iterations.");

            Func<double, double> objective = x =>
            {
                if (!IsFinite(x) || x < a || x > b)
                    throw new ArithmeticException("The extremum search produced a coordinate outside the interval. Rescale the coordinate.");
                var value = fx(x);
                if (!IsFinite(value))
                    throw new ArithmeticException("The function must return a finite value at every sampled point, including the endpoints.");
                return maximize ? -value : value;
            };

            var lowerValue = objective(a);
            if (a == b) return a;
            var upperValue = objective(b);
            var width = b - a;
            Func<double, double> coordinate = unit =>
            {
                if (!IsFinite(unit) || unit < 0 || unit > 1)
                    throw new ArithmeticException("The extremum search produced an invalid normalized coordinate.");
                if (unit == 0) return a;
                if (unit == 1) return b;
                // Clamp only rounding in the affine mapping, never expand the
                // domain supplied by the caller.
                return Math.Max(a, Math.Min(b, a + width * unit));
            };
            // Accord's stopping criterion includes sqrt(epsilon)*abs(x).
            // Normalize first so translating a narrow interval far from zero
            // cannot make the very first sample look converged.
            var normalizedTolerance = Math.Min(1, tolerance / width);
            var search = new BrentSearch(unit => objective(coordinate(unit)), 0, 1,
                normalizedTolerance, iterations);
            if (!search.Minimize())
                throw new InvalidOperationException("The extremum search did not converge within the iteration budget. Check the function, interval and tolerance.");

            var result = coordinate(search.Solution);
            var valueAtResult = search.Value;
            // Brent searches the interior. Compare the supplied endpoints so a
            // monotone function has an exact boundary result on the closed interval.
            if (lowerValue <= valueAtResult)
            {
                result = a;
                valueAtResult = lowerValue;
            }
            if (upperValue < valueAtResult) result = b;
            return result;
        }

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
