using System.Globalization;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Computator.NET.Engine;
using Computator.NET.Engine.Charting;

namespace Computator.NET.PortableWeb;

internal sealed class ExecutionRequest
{
    public string Operation { get; set; } = "calculate";
    public string Expression { get; set; } = "";
    public string CustomFunctions { get; set; } = "";
    public string? Method { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double A { get; set; } = -1;
    public double B { get; set; } = 1;
    public uint N { get; set; } = 10000;
    public uint Order { get; set; } = 1;
    public double Epsilon { get; set; } = 1e-6;
    public double XMin { get; set; } = -5;
    public double XMax { get; set; } = 5;
    public double YMin { get; set; } = -5;
    public double YMax { get; set; } = 5;
    public int CurveSamples { get; set; } = 1025;
    public int GridSize { get; set; } = 65;
    public bool AllowEmpty2D { get; set; }
}

internal sealed record WorkerReply(int Status, string? Result = null, string? Detail = null,
    IReadOnlyList<EngineDiagnostic>? Diagnostics = null, CurveSamples? Curve = null,
    SurfaceGrid? Surface = null);

internal static class Execution
{
    public const int MaximumBodyBytes = 262144;
    public const string IntegralMethod = "Simpson's method";
    public const string DerivativeMethod = "Finite difference derivative";
    public const string RootMethod = "Brent's method";
    public static readonly string[] Operations = ["calculate", "script", "integral", "derivative", "function-root"];
    // Private pipe transport, never embedded in HTML. Avoid expanding valid
    // Unicode source into a much larger sequence of escaped code points.
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static void Validate(ExecutionRequest request, bool chart = false)
    {
        if (chart ? request.Operation != "chart" : !Operations.Contains(request.Operation))
            throw new ArgumentException("Unknown operation.");
        if (string.IsNullOrWhiteSpace(request.Expression) || request.Expression.Length > PortableEngine.MaximumSourceCharacters ||
            request.CustomFunctions == null || request.CustomFunctions.Length > PortableEngine.MaximumSourceCharacters)
            throw new ArgumentException("Expression/script and custom definitions are each limited to 65,536 characters; an expression is required.");
        if (!new[] { request.X, request.Y, request.A, request.B, request.Epsilon }.All(double.IsFinite))
            throw new ArgumentException("Numeric parameters must be finite.");
        if (request.Method?.Length > 256) throw new ArgumentException("Unknown numerical method.");
        switch (request.Operation)
        {
            case "chart":
                CurveSampler.Validate(request.XMin, request.XMax, request.YMin, request.YMax, request.CurveSamples);
                SurfaceSampler.Validate(request.XMin, request.XMax, request.YMin, request.YMax, request.GridSize);
                break;
            case "integral":
                RequireMethod(request, IntegralMethod);
                RequireInterval(request);
                if (request.N < 2 || request.N > 1000000 || request.N % 2 != 0)
                    throw new ArgumentException("Simpson integration requires an even count from 2 to 1,000,000.");
                break;
            case "derivative":
                RequireMethod(request, DerivativeMethod);
                if (request.Order < 1 || request.Order > 10)
                    throw new ArgumentException("Derivative order must be from 1 to 10.");
                break;
            case "function-root":
                RequireMethod(request, RootMethod);
                RequireInterval(request);
                if (request.N < 1 || request.N > 1000000 || request.Epsilon <= 0 || request.Epsilon > 1)
                    throw new ArgumentException("Root finding requires 1–1,000,000 iterations and a positive tolerance no greater than one.");
                break;
        }
    }

    private static void RequireMethod(ExecutionRequest request, string method)
    {
        if (request.Method != method) throw new ArgumentException($"This portable host supports only {method} for {request.Operation}.");
    }
    private static void RequireInterval(ExecutionRequest request)
    {
        if (request.A >= request.B) throw new ArgumentException("Expected finite increasing interval bounds.");
    }

    public static WorkerReply Run(ExecutionRequest request)
    {
        try
        {
            Validate(request, chart: request.Operation == "chart");
            var engine = new PortableEngine();
            if (request.Operation == "chart")
            {
                try
                {
                    var curve = engine.SampleCurve(request.Expression, request.XMin, request.XMax,
                        request.YMin, request.YMax, request.CurveSamples, request.CustomFunctions);
                    return curve.FiniteSamples == 0 && !request.AllowEmpty2D
                        ? new WorkerReply(422, Detail: "No finite curve samples in this chart area.")
                        : new WorkerReply(200, Curve: curve);
                }
                catch (EngineCompilationException curveError)
                {
                    // Only a failed real f(x) compilation can select f(x,y).
                    // Never re-evaluate user code after a runtime failure.
                    try
                    {
                        var surface = engine.SampleSurface(request.Expression, request.XMin, request.XMax,
                            request.YMin, request.YMax, request.GridSize, request.CustomFunctions);
                        return surface.FiniteSamples < 3
                            ? new WorkerReply(422, Detail: "The selected domain contains too few finite surface samples.")
                            : new WorkerReply(200, Surface: surface);
                    }
                    catch (EngineCompilationException surfaceError)
                    {
                        throw new EngineCompilationException(curveError.Diagnostics.Concat(surfaceError.Diagnostics)
                            .Distinct().ToArray());
                    }
                }
            }
            var result = request.Operation switch
            {
                "calculate" => Calculate(engine, request),
                "script" => engine.ExecuteScript(request.Expression, request.CustomFunctions),
                "integral" => Format(engine.Integrate(request.Expression, request.A, request.B, checked((int)request.N), request.CustomFunctions)),
                "derivative" => Format(engine.Differentiate(request.Expression, request.X, request.Order, request.CustomFunctions)),
                "function-root" => Format(engine.FindRoot(request.Expression, request.A, request.B, request.Epsilon, request.N, request.CustomFunctions)),
                _ => throw new ArgumentException("Unknown operation.")
            };
            return new WorkerReply(200, Result: result);
        }
        catch (EngineCompilationException exception)
        {
            var scope = request.Operation == "chart"
                ? "Portable charts support explicit real f(x) curves or f(x,y) surfaces. Complex functions and implicit equations are unavailable.\n"
                : "";
            return new WorkerReply(400, Detail: scope + exception.Message, Diagnostics: exception.Diagnostics);
        }
        catch (Exception exception)
        { return new WorkerReply(400, Detail: exception.Message); }
    }

    private static string Calculate(PortableEngine engine, ExecutionRequest request)
    {
        // Select by successful compilation, not a string/identifier heuristic.
        // A failed real compilation executes no user code. This also handles
        // complex-valued constants and custom functions without a literal z/i.
        try { return Format(engine.EvaluateReal(request.Expression, request.X, request.Y, request.CustomFunctions)); }
        catch (EngineCompilationException realError)
        {
            try { return engine.EvaluateComplex(request.Expression, new Complex(request.X, request.Y), request.CustomFunctions).ToString("R", CultureInfo.InvariantCulture); }
            catch (EngineCompilationException complexError)
            {
                throw new EngineCompilationException(realError.Diagnostics.Concat(complexError.Diagnostics).Distinct().ToArray());
            }
        }
    }
    private static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
