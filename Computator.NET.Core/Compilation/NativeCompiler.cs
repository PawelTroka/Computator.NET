using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Computator.NET.Localization;
using Computator.NET.Core.Compilation.TslHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NLog;

namespace Computator.NET.Core.Compilation
{
    /// <summary>
    /// Compiles the C# produced by TSL without an installed csc.exe or SDK.
    /// Scripts retain the application's full trust; this is not a sandbox.
    /// </summary>
    public class NativeCompiler : Component
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ConcurrentDictionary<string, PortableExecutableReference> ReferenceCache =
            new ConcurrentDictionary<string, PortableExecutableReference>(StringComparer.OrdinalIgnoreCase);
        private static readonly Lazy<string[]> RuntimeReferences = new Lazy<string[]>(FindRuntimeReferences);
        private readonly object _referenceLock = new object();
        private readonly List<string> _references = new List<string>
        {
            "Meta.Numerics.dll", "MathNet.Numerics.dll", "Accord.Math.dll", "Accord.dll"
        };

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int MainCodeStarOffsetLine { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int MainCodeEndOffsetLine { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int CustomFunctionsStartOffsetLine { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int CustomFunctionsEndOffsetLine { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsScripting { get; set; }

        public Assembly Compile(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            string[] references;
            lock (_referenceLock) references = _references.ToArray();

            var metadata = RuntimeReferences.Value.Concat(references.Select(ResolveReference))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(path => ReferenceCache.GetOrAdd(path, file => MetadataReference.CreateFromFile(file)));
            var tree = CSharpSyntaxTree.ParseText(input,
                new CSharpParseOptions(LanguageVersion.CSharp14), encoding: Encoding.UTF8);
            var compilation = CSharpCompilation.Create("Computator.TSL." + Guid.NewGuid().ToString("N"),
                new[] { tree }, metadata,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release));
            compilation = FunctionPowerResolver.Resolve(compilation);

            using (var output = new MemoryStream())
            {
                var result = compilation.Emit(output);
                if (!result.Success)
                {
                    var exception = CreateCompilationException(result.Diagnostics);
                    Logger.Error(exception, exception.Message);
                    throw exception;
                }
                // Unique identities still preserve #39. Existing callers keep their
                // program alive through its Assembly/Type/Delegate references; once
                // those are released, the runtime can collect the whole context.
                return CollectibleProgramLoader.LoadProgram(output);
            }
        }

        public void AddDll(string dllPath)
        {
            if (string.IsNullOrWhiteSpace(dllPath))
                throw new ArgumentException("An assembly name or path is required.", nameof(dllPath));
            lock (_referenceLock) _references.Add(dllPath);
        }

        private static string[] FindRuntimeReferences()
        {
            var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
            if (!string.IsNullOrEmpty(trusted))
                return trusted.Split(Path.PathSeparator).Where(File.Exists).ToArray();

            var paths = new List<string>
            {
                typeof(object).Assembly.Location,
                typeof(Uri).Assembly.Location,
                typeof(Enumerable).Assembly.Location,
                typeof(System.Numerics.Complex).Assembly.Location,
                typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location,
                Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51").Location
            };
            var facades = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "Facades");
            if (Directory.Exists(facades)) paths.AddRange(Directory.EnumerateFiles(facades, "*.dll"));
            return paths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private static string ResolveReference(string reference)
        {
            if (Path.IsPathRooted(reference))
            {
                if (File.Exists(reference)) return Path.GetFullPath(reference);
                throw new FileNotFoundException("A TSL compilation reference was not found.", reference);
            }

            var runtime = RuntimeEnvironment.GetRuntimeDirectory();
            var directories = new[] { AppContext.BaseDirectory,
                Path.GetDirectoryName(typeof(NativeCompiler).Assembly.Location), runtime,
                Path.Combine(runtime, "Facades"), Path.Combine(runtime, "WPF") };
            foreach (var directory in directories)
            {
                var path = Path.Combine(directory, reference);
                if (File.Exists(path)) return Path.GetFullPath(path);
            }
            var name = Path.GetFileNameWithoutExtension(reference);
            var loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly =>
                !assembly.IsDynamic && string.Equals(assembly.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
            if (loaded != null && !string.IsNullOrEmpty(loaded.Location)) return loaded.Location;
            return Assembly.Load(new AssemblyName(name)).Location;
        }

        private CompilationException CreateCompilationException(IEnumerable<Diagnostic> diagnostics)
        {
            var exception = new CompilationException();
            foreach (var diagnostic in diagnostics.Where(d =>
                d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning))
            {
                var position = diagnostic.Location.GetLineSpan().StartLinePosition;
                var line = diagnostic.Location.IsInSource ? position.Line + 1 : 0;
                var column = diagnostic.Location.IsInSource ? position.Character + 1 : 0;
                var place = GetPlaceForLine(line);
                var offset = place == CompilationErrorPlace.MainCode ? MainCodeStarOffsetLine
                    : place == CompilationErrorPlace.CustomFunctions ? CustomFunctionsStartOffsetLine : 0;
                exception.Errors[place].Add(new CompilerError("", line - offset, column,
                    diagnostic.Id, diagnostic.GetMessage())
                {
                    IsWarning = diagnostic.Severity == DiagnosticSeverity.Warning
                });
            }

            var message = new StringBuilder($"{Strings.Syntax_error}{Environment.NewLine}{Strings.Details}");
            if (exception.HasMainCodeErrors)
                AppendErrors(message, IsScripting ? Strings.Script_errors : Strings.Expression_errors,
                    exception.Errors[CompilationErrorPlace.MainCode]);
            if (exception.HasCustomFunctionsErrors)
                AppendErrors(message, Strings.Custom_functions_errors,
                    exception.Errors[CompilationErrorPlace.CustomFunctions]);
            if (exception.HasInternalErrors)
                AppendErrors(message, Strings.Internal_errors, exception.Errors[CompilationErrorPlace.Internal]);
            return new CompilationException(message.ToString()) { Errors = exception.Errors };
        }

        private CompilationErrorPlace GetPlaceForLine(int line)
        {
            if (line > 0 && MainCodeStarOffsetLine > 0 &&
                line >= MainCodeStarOffsetLine && line <= MainCodeEndOffsetLine)
                return CompilationErrorPlace.MainCode;
            if (line > 0 && CustomFunctionsStartOffsetLine > 0 &&
                line >= CustomFunctionsStartOffsetLine && line <= CustomFunctionsEndOffsetLine)
                return CompilationErrorPlace.CustomFunctions;
            return CompilationErrorPlace.Internal;
        }

        private static void AppendErrors(StringBuilder message, string heading, IEnumerable<CompilerError> errors)
        {
            message.Append($"{Environment.NewLine} {heading}:");
            foreach (var error in errors)
                message.Append($"{Environment.NewLine}  ({Strings.NativeCompiler_Line}: {error.Line} " +
                    $"{Strings.NativeCompiler_Column}: {error.Column}): " +
                    $"{(error.IsWarning ? Strings.NativeCompiler_warning : Strings.NativeCompiler_error)}" +
                    $"{error.ErrorNumber}: {error.ErrorText}");
        }
    }
}
