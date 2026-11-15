using Computator.NET.Engine.Charting;

namespace Computator.NET.Engine;

public sealed partial class PortableEngine
{
    /// <summary>Compile an explicit real f(x) once and return bounded numerical curve samples.</summary>
    public CurveSamples SampleCurve(string expression, double xMin, double xMax, double yMin, double yMax,
        int sampleCount = 1025, string customFunctions = "", CancellationToken cancellation = default)
    {
        CurveSampler.Validate(xMin, xMax, yMin, yMax, sampleCount);
        cancellation.ThrowIfCancellationRequested();
        using var program = CompileScalar(expression, customFunctions, requireExplicitExpression: true);
        return CurveSampler.Sample(program.Invoke, xMin, xMax, yMin, yMax, sampleCount, cancellation);
    }
}
