using System.Numerics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Computator.NET.Core.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Computator.NET.Engine;

/// <summary>Headless managed execution. Trusted code only; this is not a sandbox.</summary>
public sealed partial class PortableEngine
{
    public const int MaximumSourceCharacters = 65536;
    private static readonly Lazy<MetadataReference[]> References = new(CreateReferences);

    public double EvaluateReal(string expression, double x = 0, double y = 0, string customFunctions = "")
    {
        using var program = Compile<Func<double, double, double>>(expression, customFunctions,
            "public static double Run(double x, double y) { return\n#line 1 \"expression.tsl\"\n", ";\n#line default\n}");
        return program.Invoke(x, y);
    }
    public Complex EvaluateComplex(string expression, Complex z, string customFunctions = "")
    {
        using var program = Compile<Func<Complex, Complex>>(expression, customFunctions,
            "public static complex Run(complex z) { return\n#line 1 \"expression.tsl\"\n", ";\n#line default\n}");
        return program.Invoke(z);
    }
    public string ExecuteScript(string script, string customFunctions = "")
    {
        using var program = Compile<Action<Action<string>>>(script, customFunctions,
            "public static void Run(Action<string> output) { Action<object> write = value => output(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? \"\");\n#line 1 \"script.tsl\"\n",
            "\n#line default\n}");
        var result = new StringBuilder();
        program.Invoke(text =>
        {
            lock (result)
            {
                if (text.Length > MaximumSourceCharacters - result.Length) throw new InvalidOperationException("Script output exceeds the configured limit.");
                result.Append(text);
            }
        });
        lock (result) return result.ToString();
    }
    private static Compiled<T> Compile<T>(string input, string customFunctions, string prefix, string suffix) where T : Delegate
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(customFunctions);
        if (input.Length > MaximumSourceCharacters || customFunctions.Length > MaximumSourceCharacters)
            throw new ArgumentException("Expression/script and custom definitions are each limited to 65,536 characters.");
        var compiler = new TslCompiler();
        var source = "using System; using System.Numerics; using Computator.NET.Engine; using Computator.NET.Core.NumericalCalculations; " +
            "using static Computator.NET.Engine.ScalarFunctions; using static Computator.NET.Core.Constants.MathematicalConstants; " +
            "using static Computator.NET.Core.Functions.MatrixFunctions;\n" +
            TslCompiler.TypesAliases + "\npublic static class Program {\n#line 1 \"custom-functions.tslf\"\n" +
            compiler.TransformToCSharp(customFunctions) + "\n#line default\n" + prefix + compiler.TransformToCSharp(input) + suffix + "\n}";
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14), encoding: Encoding.UTF8);
        var compilation = CSharpCompilation.Create("Computator.Portable." + Guid.NewGuid().ToString("N"), new[] { tree }, References.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        if (!result.Success)
        {
            var errors = result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Select(diagnostic =>
            {
                var line = diagnostic.Location.GetMappedLineSpan();
                return new EngineDiagnostic(diagnostic.Id, line.Path, line.StartLinePosition.Line + 1,
                    line.StartLinePosition.Character + 1, diagnostic.GetMessage());
            }).ToArray();
            throw new EngineCompilationException(errors);
        }
        stream.Position = 0;
        var context = new AssemblyLoadContext("Computator.Program." + Guid.NewGuid().ToString("N"), isCollectible: true);
        try
        {
            var assembly = context.LoadFromStream(stream);
            var method = assembly.GetType("Program", throwOnError: true)!.GetMethod("Run", BindingFlags.Static | BindingFlags.Public)!;
            return new Compiled<T>(context, method.CreateDelegate<T>());
        }
        catch { context.Unload(); throw; }
    }
    private static MetadataReference[] CreateReferences()
    {
        var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrWhiteSpace(trusted)) throw new PlatformNotSupportedException("A modern .NET runtime with platform reference metadata is required.");
        return trusted.Split(Path.PathSeparator).Concat(new[] { typeof(PortableEngine).Assembly.Location,
                typeof(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).Assembly.Location,
                typeof(Accord.Math.Integration.RombergMethod).Assembly.Location })
            .Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToArray();
    }
    private sealed class Compiled<T>(AssemblyLoadContext context, T callback) : IDisposable where T : Delegate
    {
        public T Invoke { get; private set; } = callback;
        public void Dispose() { Invoke = null!; context.Unload(); }
    }
}
public sealed record EngineDiagnostic(string Code, string File, int Line, int Column, string Message);
public sealed class EngineCompilationException(IReadOnlyList<EngineDiagnostic> diagnostics)
    : Exception(string.Join(Environment.NewLine, diagnostics.Select(error => $"{error.File}({error.Line},{error.Column}): {error.Code}: {error.Message}")))
{
    public IReadOnlyList<EngineDiagnostic> Diagnostics { get; } = diagnostics;
}
