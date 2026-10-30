using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.OdeSolvers;

namespace Computator.NET.Core.NumericalCalculations
{
    /// <summary>Fixed-step initial-value solvers for real ordinary differential equations.</summary>
    public static class DifferentialEquation
    {
        /// <summary>
        /// Solve y'=derivative(t,y), y(start)=y0, returning samples values including both endpoints.
        /// This is classical RK4, without adaptive error estimation or stiff-system support.
        /// </summary>
        public static double[] rungeKutta4(Func<double, double, double> derivative, double y0,
            double start, double end, int samples = 101)
        {
            if (derivative == null) throw new ArgumentNullException(nameof(derivative));
            ValidateGrid(start, end, samples);
            if (!IsFinite(y0)) throw new ArgumentOutOfRangeException(nameof(y0), "The initial value must be finite.");
            if (start == end)
            {
                var constant = new double[samples];
                for (var i = 0; i < samples; i++) constant[i] = y0;
                return constant;
            }

            var result = RungeKutta.FourthOrder(y0, start, end, samples, (time, state) =>
            {
                RequireFinite(time);
                RequireFinite(state);
                var slope = derivative(time, state);
                RequireFinite(slope);
                return slope;
            });
            foreach (var state in result) RequireFinite(state);
            return result;
        }

        /// <summary>
        /// Solve a real first-order system. State vectors and callback results are copied so the
        /// caller's initial state, earlier samples and reused derivative buffers stay independent.
        /// </summary>
        public static Vector<double>[] rungeKutta4(Func<double, Vector<double>, Vector<double>> derivative,
            Vector<double> y0, double start, double end, int samples = 101)
        {
            if (derivative == null) throw new ArgumentNullException(nameof(derivative));
            if (y0 == null) throw new ArgumentNullException(nameof(y0));
            ValidateGrid(start, end, samples);
            if (y0.Count == 0) throw new ArgumentException("The initial state must have at least one component.", nameof(y0));
            foreach (var component in y0)
                if (!IsFinite(component)) throw new ArgumentOutOfRangeException(nameof(y0), "Every initial component must be finite.");
            if (start == end)
            {
                var constant = new Vector<double>[samples];
                for (var i = 0; i < samples; i++) constant[i] = y0.Clone();
                return constant;
            }

            var result = RungeKutta.FourthOrder(y0.Clone(), start, end, samples, (time, state) =>
            {
                RequireFinite(time);
                foreach (var component in state) RequireFinite(component);
                var slope = derivative(time, state.Clone());
                if (slope == null || slope.Count != y0.Count)
                    throw new InvalidOperationException("The derivative must return a vector with the same number of components as the initial state.");
                foreach (var component in slope) RequireFinite(component);
                return slope.Clone();
            });
            foreach (var state in result)
                foreach (var component in state) RequireFinite(component);
            return result;
        }

        private static void ValidateGrid(double start, double end, int samples)
        {
            if (samples < 2 || samples > 1000001)
                throw new ArgumentOutOfRangeException(nameof(samples), "Use between 2 and 1,000,001 samples, including the initial and final values.");
            if (!IsFinite(start)) throw new ArgumentOutOfRangeException(nameof(start), "The start coordinate must be finite.");
            if (!IsFinite(end)) throw new ArgumentOutOfRangeException(nameof(end), "The end coordinate must be finite.");
            if (start == end) return;
            var step = (end - start) / (samples - 1);
            if (!IsFinite(step) || step == 0 || start + step / 2 == start || end - step / 2 == end)
                throw new ArgumentException("The interval must have a finite, representable RK4 half-step. Rescale the time coordinate or use fewer samples.");
        }

        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        private static void RequireFinite(double value)
        {
            if (!IsFinite(value))
                throw new ArithmeticException("The ODE calculation produced a nonfinite state or derivative. Check the equation, interval and step size.");
        }
    }
}
