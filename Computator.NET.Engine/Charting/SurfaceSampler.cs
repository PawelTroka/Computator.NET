namespace Computator.NET.Engine.Charting;

/// <summary>
/// Bounded, platform-independent numerical samples for the existing browser
/// Plotly surface renderer. Z is row-major: Z[yIndex][xIndex].
/// </summary>
public static class SurfaceSampler
{
    public const int MinimumGridSize = 9;
    public const int MaximumGridSize = 129;

    public static void Validate(double xMin, double xMax, double yMin, double yMax, int gridSize)
    {
        if (gridSize < MinimumGridSize || gridSize > MaximumGridSize)
            throw new ArgumentOutOfRangeException(nameof(gridSize),
                $"Surface grid size must be from {MinimumGridSize} to {MaximumGridSize}.");
        if (!FiniteSpan(xMin, xMax) || !FiniteSpan(yMin, yMax))
            throw new ArgumentException("Surface bounds must be finite increasing intervals with finite, positive spans.");
        Axis(xMin, xMax, gridSize);
        Axis(yMin, yMax, gridSize);
    }

    public static SurfaceGrid Sample(Func<double, double, double> function, double xMin, double xMax,
        double yMin, double yMax, int gridSize, CancellationToken cancellation = default)
    {
        if (function == null) throw new ArgumentNullException(nameof(function));
        Validate(xMin, xMax, yMin, yMax, gridSize);
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
                try { value = function(x[column], y[row]); }
                // Arithmetic singularities leave holes; unrelated failures
                // remain errors rather than silently becoming zeros.
                catch (ArithmeticException) { continue; }
                if (!IsFinite(value)) continue;
                z[row][column] = value;
                finite++;
            }
        }
        return new SurfaceGrid(x, y, z, finite);
    }

    private static double[] Axis(double minimum, double maximum, int count)
    {
        var axis = new double[count];
        for (var index = 0; index < count; index++)
        {
            var ratio = index / (double)(count - 1);
            axis[index] = index == 0 ? minimum : index == count - 1 ? maximum
                : (1 - ratio) * minimum + ratio * maximum;
            if (!IsFinite(axis[index]) || (index > 0 && axis[index] <= axis[index - 1]))
                throw new ArgumentException("Surface bounds are too close together for the requested sampling precision.");
        }
        return axis;
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    private static bool FiniteSpan(double min, double max) =>
        IsFinite(min) && IsFinite(max) && min < max && IsFinite(max - min) && max - min > 0;
}

public sealed class SurfaceGrid(double[] x, double[] y, double?[][] z, int finiteSamples)
{
    public string Kind => "surface";
    public double[] X { get; } = x;
    public double[] Y { get; } = y;
    public double?[][] Z { get; } = z;
    public int FiniteSamples { get; } = finiteSamples;
    public int MissingSamples => X.Length * Y.Length - FiniteSamples;
}
