using System;
using System.IO;
using System.Linq;
using Computator.NET.Core.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// Shared by the Windows compiler suite and native Mono acceptance. The fixture
// emits real assemblies and exercises the production compiler, including its
// process-wide metadata cache, without loading the external models at runtime.
internal static class CompilerReferencePathContract
{
    public static void DistinctReferences(string layout)
    {
        WithReferences(layout, (first, second, distinct) =>
        {
            Emit(first, "FirstModel", 17);
            if (distinct) Emit(second, "SecondModel", 29);
            var expression = distinct ? "FirstModel.Value + SecondModel.Value" : "FirstModel.Value";
            Equal(Compile(expression, first, second), distinct ? 46 : 17);
        });
    }

    public static void IsolatedMetadataCache(string layout)
    {
        WithReferences(layout, (first, second, distinct) =>
        {
            // Identical public names with different constants expose silent
            // reuse of another path's metadata, not just a missing-type error.
            Emit(first, "Model", 17);
            if (distinct) Emit(second, "Model", 29);
            Equal(Compile("Model.Value", first), 17);
            Equal(Compile("Model.Value", second), distinct ? 29 : 17);
            Equal(Compile("Model.Value", first), 17);
            Equal(Compile("Model.Value", second), distinct ? 29 : 17);
        });
    }

    public static void EquivalentReferences(string layout)
    {
        WithReferences(layout, (first, second, distinct) =>
        {
            Emit(first, "Model", 17);
            if (distinct) File.Copy(first, second);
            // The same image through multiple names remains valid. Include a
            // repeated path and a spelling that normalizes to that same path.
            var normalizedAlias = Path.Combine(Path.GetDirectoryName(first), ".", Path.GetFileName(first));
            Equal(Compile("Model.Value", first, first, normalizedAlias, second), 17);
        });
    }

    private static void WithReferences(string layout, Action<string, string, bool> verify)
    {
        var root = Path.Combine(Path.GetTempPath(), "Computator-reference-paths-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            string first, second;
            if (layout == "file")
            {
                first = Path.Combine(root, "model.dll");
                second = Path.Combine(root, "MODEL.dll");
            }
            else if (layout == "directory")
            {
                Directory.CreateDirectory(Path.Combine(root, "models"));
                Directory.CreateDirectory(Path.Combine(root, "MODELS"));
                first = Path.Combine(root, "models", "model.dll");
                second = Path.Combine(root, "MODELS", "model.dll");
            }
            else throw new ArgumentException("Unknown reference-path layout.", nameof(layout));

            // Probe the actual fixture volume, not an assumed OS default. A Mac
            // (or Windows directory) can use either kind of filesystem. Never
            // overwrite the first image when the second spelling is its alias.
            File.WriteAllText(first, "path probe");
            var distinct = !File.Exists(second);
            Console.WriteLine("Compiler reference {0} paths: {1}.", layout,
                distinct ? "distinct case-sensitive files" : "aliases of one case-insensitive file");
            verify(first, second, distinct);
        }
        finally { Directory.Delete(root, true); }
    }

    private static void Emit(string path, string type, int value)
    {
        var compilation = CSharpCompilation.Create("ReferenceFixture" + Guid.NewGuid().ToString("N"),
            new[] { CSharpSyntaxTree.ParseText("public static class " + type + " { public const int Value = " + value + "; }") },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var result = compilation.Emit(path);
        if (!result.Success)
            throw new InvalidOperationException("Could not emit reference fixture: " +
                string.Join(Environment.NewLine, result.Diagnostics.Select(diagnostic => diagnostic.ToString())));
    }

    private static int Compile(string expression, params string[] references)
    {
        using (var compiler = new NativeCompiler())
        {
            foreach (var reference in references) compiler.AddDll(reference);
            var program = compiler.Compile("public static class Probe { public static int Run() => " + expression + "; }");
            return (int)program.GetType("Probe").GetMethod("Run").Invoke(null, null);
        }
    }

    private static void Equal(int actual, int expected)
    {
        if (actual != expected)
            throw new InvalidOperationException("Compiled model returned " + actual + "; expected " + expected + ".");
    }
}
