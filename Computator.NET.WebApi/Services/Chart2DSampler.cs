using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using Computator.NET.WebApi.Controllers;

namespace Computator.NET.WebApi.Services
{
    /// <summary>
    /// Mathematical samples for the browser's coordinate-based 2D renderer.
    /// Expressions remain in the existing server compiler/worker; clients never
    /// execute user code. Undefined/arithmetic singularities are explicit holes.
    /// </summary>
    public static class Chart2DSampler
    {
        public const int MinimumCurveSamples = 33;
        public const int MaximumCurveSamples = 4097;
        public const int MinimumComplexGridSize = 9;
        public const int MaximumComplexGridSize = 257;
        private const int MaximumDiscontinuityProbes = 8;

        public static void ValidateRequest(ExecutionRequest request, IDictionary<string, string[]> errors)
        {
            if (request.CurveSamples < MinimumCurveSamples || request.CurveSamples > MaximumCurveSamples)
                errors["curveSamples"] = new[] { $"Curve samples must be from {MinimumCurveSamples} to {MaximumCurveSamples}." };
            if (request.ComplexGridSize < MinimumComplexGridSize || request.ComplexGridSize > MaximumComplexGridSize)
                errors["complexGridSize"] = new[] { $"Complex grid size must be from {MinimumComplexGridSize} to {MaximumComplexGridSize}." };
            if (!FiniteSpan(request.XMin, request.XMax) || !FiniteSpan(request.YMin, request.YMax))
                errors["bounds"] = new[] { "Interactive chart bounds must be finite increasing intervals with finite, positive spans." };
        }

        public static CurveSamples SampleCurve(Function function, double xMin, double xMax,
            double yMin, double yMax, int sampleCount, CancellationToken cancellation)
        {
            if (function == null || function.FunctionType != FunctionType.Real2D)
                throw new ArgumentException("Only explicit real f(x) functions can be sampled.", nameof(function));
            if (sampleCount < MinimumCurveSamples || sampleCount > MaximumCurveSamples)
                throw new ArgumentOutOfRangeException(nameof(sampleCount));
            Bounds(xMin, xMax, yMin, yMax);
            cancellation.ThrowIfCancellationRequested();
            var axis = Axis(xMin, xMax, sampleCount);
            var values = new double?[sampleCount];
            for (var index = 0; index < sampleCount; index++)
                values[index] = RealValue(function, axis[index], cancellation);

            var x = new List<double>(2 * sampleCount - 1);
            var y = new List<double?>(2 * sampleCount - 1);
            var finite = 0;
            for (var index = 0; index < sampleCount; index++)
            {
                cancellation.ThrowIfCancellationRequested();
                x.Add(axis[index]); y.Add(values[index]);
                if (values[index].HasValue) finite++;
                if (index + 1 == sampleCount || !values[index].HasValue || !values[index + 1].HasValue) continue;
                if (HasUnresolvedJump(function, axis[index], values[index].Value,
                    axis[index + 1], values[index + 1].Value, yMax - yMin, cancellation))
                {
                    // A strictly ordered null point prevents connecting across a
                    // pole/jump without deleting either valid endpoint sample.
                    var midpoint = Midpoint(axis[index], axis[index + 1]);
                    if (midpoint > axis[index] && midpoint < axis[index + 1])
                    { x.Add(midpoint); y.Add(null); }
                }
            }
            return new CurveSamples(x.ToArray(), y.ToArray(), finite);
        }

        public static ComplexSamples SampleComplex(Function function, double xMin, double xMax,
            double yMin, double yMax, int gridSize, CancellationToken cancellation)
        {
            if (function == null || function.FunctionType != FunctionType.Complex)
                throw new ArgumentException("Only explicit complex f(z) functions can be sampled.", nameof(function));
            if (gridSize < MinimumComplexGridSize || gridSize > MaximumComplexGridSize)
                throw new ArgumentOutOfRangeException(nameof(gridSize));
            Bounds(xMin, xMax, yMin, yMax);
            cancellation.ThrowIfCancellationRequested();
            var x = Axis(xMin, xMax, gridSize);
            var y = Axis(yMin, yMax, gridSize);
            var real = new double?[gridSize][];
            var imaginary = new double?[gridSize][];
            var finite = 0;
            for (var row = 0; row < gridSize; row++)
            {
                real[row] = new double?[gridSize]; imaginary[row] = new double?[gridSize];
                for (var column = 0; column < gridSize; column++)
                {
                    cancellation.ThrowIfCancellationRequested();
                    Complex value;
                    try { value = function.Evaluate(new Complex(x[column], y[row])); }
                    catch (CalculationException error) when (error.InnerException is ArithmeticException) { continue; }
                    catch (ArithmeticException) { continue; }
                    if (!double.IsFinite(value.Real) || !double.IsFinite(value.Imaginary)) continue;
                    real[row][column] = value.Real; imaginary[row][column] = value.Imaginary;
                    finite++;
                }
            }
            return new ComplexSamples(x, y, real, imaginary, finite);
        }

        private static double? RealValue(Function function, double x, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            try
            {
                var value = function.Evaluate(x);
                return double.IsFinite(value) ? value : (double?)null;
            }
            catch (CalculationException error) when (error.InnerException is ArithmeticException) { return null; }
            catch (ArithmeticException) { return null; }
        }

        private static bool HasUnresolvedJump(Function function, double left, double leftValue,
            double right, double rightValue, double visibleSpan, CancellationToken cancellation)
        {
            // Bound extra evaluation to eight midpoint probes per base interval.
            // A smooth steep line has zero chord error and stays connected. For
            // nonlinear intervals, follow the half with the largest value jump;
            // a jump that does not contract as x shrinks indicates an unresolved
            // pole/discontinuity. This is a finite-resolution heuristic, not a
            // proof of continuity (arbitrarily narrow/high-frequency features can
            // still alias). A new viewport always obtains fresh samples.
            var originalJump = Distance(leftValue, rightValue);
            for (var depth = 0; depth < MaximumDiscontinuityProbes; depth++)
            {
                var midpoint = Midpoint(left, right);
                if (midpoint <= left || midpoint >= right) return false;
                var sampled = RealValue(function, midpoint, cancellation);
                if (!sampled.HasValue) return true;
                var value = sampled.Value;
                var chord = Midpoint(leftValue, rightValue);
                var error = Distance(value, chord);
                var tolerance = Math.Max(visibleSpan / 2000,
                    Math.Max(Math.Abs(leftValue), Math.Abs(rightValue)) * 1e-12);
                if (error <= tolerance) return false;
                var leftJump = Distance(leftValue, value);
                var rightJump = Distance(value, rightValue);
                if (leftJump >= rightJump) { right = midpoint; rightValue = value; }
                else { left = midpoint; leftValue = value; }
                var jump = Math.Max(leftJump, rightJump);
                if (jump < originalJump / 2 || jump < visibleSpan / 20) return false;
            }
            return true;
        }

        // Saturating distance avoids overflow while retaining ordering for finite
        // samples near the representable limits. Weighted means avoid overflowing
        // addition of two same-sign endpoints.
        private static double Distance(double a, double b) => Math.Min(double.MaxValue, Math.Abs(a - b));
        private static double Midpoint(double a, double b) => a * 0.5 + b * 0.5;
        private static bool FiniteSpan(double minimum, double maximum) =>
            double.IsFinite(minimum) && double.IsFinite(maximum) && minimum < maximum &&
            double.IsFinite(maximum - minimum) && maximum - minimum > 0;

        private static void Bounds(double xMin, double xMax, double yMin, double yMax)
        {
            if (!FiniteSpan(xMin, xMax) || !FiniteSpan(yMin, yMax))
                throw new ArgumentException("Interactive chart bounds must be finite increasing intervals with finite, positive spans.");
        }

        private static double[] Axis(double minimum, double maximum, int count)
        {
            var axis = new double[count];
            for (var index = 0; index < count; index++)
            {
                var ratio = index / (double)(count - 1);
                axis[index] = index == 0 ? minimum : index == count - 1 ? maximum
                    : (1 - ratio) * minimum + ratio * maximum;
                if (!double.IsFinite(axis[index]) || (index > 0 && axis[index] <= axis[index - 1]))
                    throw new ArgumentException("Chart bounds are too close together for the requested sampling precision.");
            }
            return axis;
        }
    }

    public sealed class CurveSamples
    {
        public string Kind => "curve";
        public double[] X { get; }
        public double?[] Y { get; }
        public int FiniteSamples { get; }
        public int MissingSamples => X.Length - FiniteSamples;
        public CurveSamples(double[] x, double?[] y, int finiteSamples)
        { X = x; Y = y; FiniteSamples = finiteSamples; }
    }

    /// <summary>Row-major values: Real[yIndex][xIndex], with both axes ascending.</summary>
    public sealed class ComplexSamples
    {
        public string Kind => "complex";
        public double[] X { get; }
        public double[] Y { get; }
        public double?[][] Real { get; }
        public double?[][] Imaginary { get; }
        public int FiniteSamples { get; }
        public int MissingSamples => X.Length * Y.Length - FiniteSamples;
        public ComplexSamples(double[] x, double[] y, double?[][] real, double?[][] imaginary, int finiteSamples)
        { X = x; Y = y; Real = real; Imaginary = imaginary; FiniteSamples = finiteSamples; }
    }
}
