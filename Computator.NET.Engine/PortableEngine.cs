using System.Numerics;
using System.Reflection;
#if !NETFRAMEWORK
using System.Runtime.Loader;
#endif
using System.Text;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.TslHandlers;
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
        if (input == null) throw new ArgumentNullException(nameof(input));
        if (customFunctions == null) throw new ArgumentNullException(nameof(customFunctions));
        if (input.Length > MaximumSourceCharacters || customFunctions.Length > MaximumSourceCharacters)
            throw new ArgumentException("Expression/script and custom definitions are each limited to 65,536 characters.");
        var compiler = new TslCompiler();
        var source = "using System; using System.Numerics; using Computator.NET.Engine; using Computator.NET.Core.NumericalCalculations; " +
            "using static Computator.NET.Engine.ScalarFunctions; using static Computator.NET.Core.Constants.MathematicalConstants; " +
            "using static Computator.NET.Core.Functions.MatrixFunctions;\n" +
            TslCompiler.TypesAliases + "\npublic static class Program {\n#line 1 \"custom-functions.tslf\"\n" +
            compiler.TransformToCSharp(customFunctions, string.Empty, deferSymbolicFunctionPowers: true) + "\n#line default\n" + prefix +
            compiler.TransformToCSharp(input, customFunctions, deferSymbolicFunctionPowers: true) + suffix + "\n}";
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14), encoding: Encoding.UTF8);
        var compilation = CSharpCompilation.Create("Computator.Portable." + Guid.NewGuid().ToString("N"), new[] { tree }, References.Value,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        compilation = FunctionPowerResolver.Resolve(compilation);
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
#if NETFRAMEWORK
        // Framework/Mono cannot unload individual generated assemblies. Keep the
        // same compiler and numerical types; assemblies live until process exit.
        var assembly = Assembly.Load(stream.ToArray());
        var method = assembly.GetType("Program", throwOnError: true)!.GetMethod("Run", BindingFlags.Static | BindingFlags.Public)!;
        return new Compiled<T>((T)Delegate.CreateDelegate(typeof(T), method));
#else
        var context = new AssemblyLoadContext("Computator.Program." + Guid.NewGuid().ToString("N"), isCollectible: true);
        try
        {
            var assembly = context.LoadFromStream(stream);
            var method = assembly.GetType("Program", throwOnError: true)!.GetMethod("Run", BindingFlags.Static | BindingFlags.Public)!;
            return new Compiled<T>(context, method.CreateDelegate<T>());
        }
        catch { context.Unload(); throw; }
#endif
    }
    private static MetadataReference[] CreateReferences()
    {
        var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        IEnumerable<string> runtime;
        if (!string.IsNullOrWhiteSpace(trusted)) runtime = trusted!.Split(Path.PathSeparator);
#if NETFRAMEWORK
        else
        {
            var paths = new List<string>
            {
                typeof(object).Assembly.Location, typeof(Uri).Assembly.Location,
                typeof(Enumerable).Assembly.Location, typeof(Complex).Assembly.Location,
                typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location,
                Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51").Location,
                Assembly.Load("Accord").Location
            };
            var facades = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "Facades");
            if (Directory.Exists(facades)) paths.AddRange(Directory.EnumerateFiles(facades, "*.dll"));
            runtime = paths;
        }
#else
        else throw new PlatformNotSupportedException("A modern .NET runtime with platform reference metadata is required.");
#endif
        return runtime.Concat(new[] { typeof(PortableEngine).Assembly.Location,
                typeof(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix).Assembly.Location,
                typeof(Accord.Math.Integration.RombergMethod).Assembly.Location })
            .Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToArray();
    }
#if NETFRAMEWORK
    private sealed class Compiled<T>(T callback) : IDisposable where T : Delegate
    {
        public T Invoke { get; private set; } = callback;
        public void Dispose() { Invoke = null!; }
    }
#else
    private sealed class Compiled<T>(AssemblyLoadContext context, T callback) : IDisposable where T : Delegate
    {
        public T Invoke { get; private set; } = callback;
        public void Dispose() { Invoke = null!; context.Unload(); }
    }
#endif
}
public sealed record EngineDiagnostic(string Code, string File, int Line, int Column, string Message);
public sealed class EngineCompilationException(IReadOnlyList<EngineDiagnostic> diagnostics)
    : Exception(string.Join(Environment.NewLine, diagnostics.Select(error => $"{error.File}({error.Line},{error.Column}): {error.Code}: {error.Message}")))
{
    public IReadOnlyList<EngineDiagnostic> Diagnostics { get; } = diagnostics;
}
