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

    /// <summary>Compile an explicit real f(x,y) once and sample a bounded surface on any OS.</summary>
    public SurfaceGrid SampleSurface(string expression, double xMin, double xMax, double yMin, double yMax,
        int gridSize = 65, string customFunctions = "", CancellationToken cancellation = default)
    {
        SurfaceSampler.Validate(xMin, xMax, yMin, yMax, gridSize);
        cancellation.ThrowIfCancellationRequested();
        using var program = Compile<Func<double, double, double>>(expression, customFunctions,
            "public static double Run(double x, double y) { return\n#line 1 \"expression.tsl\"\n",
            ";\n#line default\n}", requireExplicitExpression: true);
        return SurfaceSampler.Sample(program.Invoke, xMin, xMax, yMin, yMax, gridSize, cancellation);
    }
}
