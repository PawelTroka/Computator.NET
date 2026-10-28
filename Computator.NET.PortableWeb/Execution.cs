using System.Globalization;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;
using Computator.NET.Engine;

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
}

internal sealed record WorkerReply(int Status, string? Result = null, string? Detail = null,
    IReadOnlyList<EngineDiagnostic>? Diagnostics = null);

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

    public static void Validate(ExecutionRequest request)
    {
        if (!Operations.Contains(request.Operation)) throw new ArgumentException("Unknown operation.");
        if (string.IsNullOrWhiteSpace(request.Expression) || request.Expression.Length > PortableEngine.MaximumSourceCharacters ||
            request.CustomFunctions == null || request.CustomFunctions.Length > PortableEngine.MaximumSourceCharacters)
            throw new ArgumentException("Expression/script and custom definitions are each limited to 65,536 characters; an expression is required.");
        if (!new[] { request.X, request.Y, request.A, request.B, request.Epsilon }.All(double.IsFinite))
            throw new ArgumentException("Numeric parameters must be finite.");
        if (request.Method?.Length > 256) throw new ArgumentException("Unknown numerical method.");
        switch (request.Operation)
        {
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
            Validate(request);
            var engine = new PortableEngine();
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
        { return new WorkerReply(400, Detail: exception.Message, Diagnostics: exception.Diagnostics); }
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
