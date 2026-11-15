namespace Computator.NET.Engine.Charting;

/// <summary>Bounded numerical data shared by the portable and Windows browser hosts.</summary>
public static class CurveSampler
{
    public const int MinimumSamples = 33;
    public const int MaximumSamples = 4097;
    private const int MaximumDiscontinuityProbes = 8;

    public static void Validate(double xMin, double xMax, double yMin, double yMax, int sampleCount)
    {
        if (sampleCount < MinimumSamples || sampleCount > MaximumSamples)
            throw new ArgumentOutOfRangeException(nameof(sampleCount),
                $"Curve samples must be from {MinimumSamples} to {MaximumSamples}.");
        if (!FiniteSpan(xMin, xMax) || !FiniteSpan(yMin, yMax))
            throw new ArgumentException("Interactive chart bounds must be finite increasing intervals with finite, positive spans.");
    }

    public static CurveSamples Sample(Func<double, double> function, double xMin, double xMax,
        double yMin, double yMax, int sampleCount, CancellationToken cancellation = default)
    {
        if (function == null) throw new ArgumentNullException(nameof(function));
        Validate(xMin, xMax, yMin, yMax, sampleCount);
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
            if (HasUnresolvedJump(function, axis[index], values[index]!.Value,
                axis[index + 1], values[index + 1]!.Value, yMax - yMin, cancellation))
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

    private static double? RealValue(Func<double, double> function, double x, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        try
        {
            var value = function(x);
            return IsFinite(value) ? value : (double?)null;
        }
        catch (ArithmeticException) { return null; }
    }

    private static bool HasUnresolvedJump(Func<double, double> function, double left, double leftValue,
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
    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    private static bool FiniteSpan(double minimum, double maximum) =>
        IsFinite(minimum) && IsFinite(maximum) && minimum < maximum &&
        IsFinite(maximum - minimum) && maximum - minimum > 0;

    private static double[] Axis(double minimum, double maximum, int count)
    {
        var axis = new double[count];
        for (var index = 0; index < count; index++)
        {
            var ratio = index / (double)(count - 1);
            axis[index] = index == 0 ? minimum : index == count - 1 ? maximum
                : (1 - ratio) * minimum + ratio * maximum;
            if (!IsFinite(axis[index]) || (index > 0 && axis[index] <= axis[index - 1]))
                throw new ArgumentException("Chart bounds are too close together for the requested sampling precision.");
        }
        return axis;
    }
}

public sealed class CurveSamples(double[] x, double?[] y, int finiteSamples)
{
    public string Kind => "curve";
    public double[] X { get; } = x;
    public double?[] Y { get; } = y;
    public int FiniteSamples { get; } = finiteSamples;
    public int MissingSamples => X.Length - FiniteSamples;
}
