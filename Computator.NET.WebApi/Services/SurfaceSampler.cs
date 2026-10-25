using System;
using System.Threading;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;

namespace Computator.NET.WebApi.Services
{
    /// <summary>
    /// Bounded numeric data for an interactive browser surface. This class never
    /// initializes a Windows chart or ships compiled expressions to the client.
    /// Z is row-major: Z[yIndex][xIndex]. Missing/non-real points are null.
    /// </summary>
    public static class SurfaceSampler
    {
        public static SurfaceGrid Sample(Function function, double xMin, double xMax,
            double yMin, double yMax, int gridSize, CancellationToken cancellation)
        {
            if (function == null || function.FunctionType != FunctionType.Real3D)
                throw new ArgumentException("Only explicit real f(x,y) functions can be sampled.", nameof(function));
            if (gridSize < 9 || gridSize > ExecutionPolicy.MaximumSurfaceGridSize)
                throw new ArgumentOutOfRangeException(nameof(gridSize));
            if (!double.IsFinite(xMin) || !double.IsFinite(xMax) || !double.IsFinite(yMin) ||
                !double.IsFinite(yMax) || xMin >= xMax || yMin >= yMax)
                throw new ArgumentException("Surface bounds must be finite increasing intervals.");

            var x = Axis(xMin, xMax, gridSize);
            var y = Axis(yMin, yMax, gridSize);
            var z = new double?[gridSize][];
            var finite = 0;
            for (var row = 0; row < gridSize; row++)
            {
                cancellation.ThrowIfCancellationRequested();
                z[row] = new double?[gridSize];
                for (var column = 0; column < gridSize; column++)
                {
                    cancellation.ThrowIfCancellationRequested();
                    double value;
                    try { value = function.Evaluate(x[column], y[row]); }
                    // Arithmetic singularities leave holes in the mesh; other
                    // runtime errors (including invalid user code) remain errors.
                    catch (CalculationException error) when (error.InnerException is ArithmeticException)
                    { continue; }
                    catch (ArithmeticException) { continue; }
                    if (!double.IsFinite(value)) continue;
                    z[row][column] = value;
                    finite++;
                }
            }
            return new SurfaceGrid(x, y, z, finite);
        }

        private static double[] Axis(double minimum, double maximum, int count)
        {
            var values = new double[count];
            for (var index = 0; index < count; index++)
            {
                // Weighted interpolation avoids overflowing (maximum - minimum).
                var ratio = index / (double)(count - 1);
                values[index] = index == 0 ? minimum : index == count - 1 ? maximum
                    : (1 - ratio) * minimum + ratio * maximum;
            }
            return values;
        }
    }

    public sealed class SurfaceGrid
    {
        public string Kind => "surface";
        public double[] X { get; }
        public double[] Y { get; }
        public double?[][] Z { get; }
        public int FiniteSamples { get; }
        public int MissingSamples => X.Length * Y.Length - FiniteSamples;

        public SurfaceGrid(double[] x, double[] y, double?[][] z, int finiteSamples)
        { X = x; Y = y; Z = z; FiniteSamples = finiteSamples; }
    }
}
