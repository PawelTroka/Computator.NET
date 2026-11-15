using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using Computator.NET.Engine.Charting;
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
        public const int MinimumCurveSamples = CurveSampler.MinimumSamples;
        public const int MaximumCurveSamples = CurveSampler.MaximumSamples;
        public const int MinimumComplexGridSize = 9;
        public const int MaximumComplexGridSize = 257;

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
            // Preserve the desktop evaluator's arithmetic-hole convention while
            // sharing the same numerical sampling with the portable host.
            double Evaluate(double x)
            {
                try { return function.Evaluate(x); }
                catch (CalculationException error) when (error.InnerException is ArithmeticException)
                { return double.NaN; }
            }
            var samples = CurveSampler.Sample(Evaluate, xMin, xMax, yMin, yMax, sampleCount, cancellation);
            return new CurveSamples(samples.X, samples.Y, samples.FiniteSamples);
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
