using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Computator.NET.Core.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.CompilationTests
{
    [TestFixture]
    public class MetadataReferenceSnapshotTests
    {
        private static readonly PortableExecutableReference RuntimeReference =
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private string _directory;

        [SetUp]
        public void CreateDirectory()
        {
            _directory = Path.Combine(Path.GetTempPath(), "Computator.ReferenceSnapshot." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void RemoveDirectory()
        {
            Directory.Delete(_directory, recursive: true);
        }

        [Test]
        public void CachedReferenceRemainsAnUnlockedSnapshotAfterFileReplacementAndCollection()
        {
            var path = Path.Combine(_directory, "Snapshot.dll");
            WriteAssembly(path, "public static class SnapshotValue { public const int Value = 42; }");
            using var compiler = new NativeCompiler();
            compiler.AddDll(path);
            const string source = "public static class Probe { public static int Run() => SnapshotValue.Value; }";
            NUnit.Framework.Assert.That(Run(compiler, source), Is.EqualTo(42));

            // FileShare.None is checked on Windows. Cached references must not
            // leave the input DLL mapped/locked or start observing later edits.
            using (var file = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                NUnit.Framework.Assert.That(file.Length, Is.GreaterThan(0));
            WriteAssembly(path, "public static class SnapshotValue { public const int Value = 99; }");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            NUnit.Framework.Assert.That(Run(compiler, source), Is.EqualTo(42));
            File.Delete(path);
            NUnit.Framework.Assert.That(File.Exists(path), Is.False);
        }

        [Test]
        public void ReferenceAddedAfterCompilationDoesNotLeakToAnotherCompiler()
        {
            var path = Path.Combine(_directory, "LateReference.dll");
            WriteAssembly(path, "public static class LateSnapshotValue { public const int Value = 73; }");
            const string source = "public static class Probe { public static int Run() => LateSnapshotValue.Value; }";
            using var first = new NativeCompiler();
            using var second = new NativeCompiler();
            NUnit.Framework.Assert.That(Run(first, "public static class Probe { public static int Run() => 1; }"), Is.EqualTo(1));
            NUnit.Framework.Assert.Throws<CompilationException>(() => first.Compile(source));
            first.AddDll(path);
            NUnit.Framework.Assert.That(Run(first, source), Is.EqualTo(73));
            NUnit.Framework.Assert.Throws<CompilationException>(() => second.Compile(source));
            second.AddDll(path);
            NUnit.Framework.Assert.That(Run(second, source), Is.EqualTo(73));
        }

        [Test]
        public void LinkedNetmoduleExportedTypesRemainAvailable()
        {
            var modulePath = Path.Combine(_directory, "Part.netmodule");
            WriteAssembly(modulePath,
                "public static class LinkedSnapshotValue { public const int Value = 37; }",
                OutputKind.NetModule);
            var assemblyPath = Path.Combine(_directory, "MultiModule.dll");
            WriteAssembly(assemblyPath, "public static class ManifestMarker { }",
                OutputKind.DynamicallyLinkedLibrary,
                MetadataReference.CreateFromFile(modulePath, MetadataReferenceProperties.Module));
            // Use a constant so execution need not load a multi-module assembly,
            // which modern .NET does not support. Importing its exported type is
            // still a supported compiler operation and must not silently change.
            NUnit.Framework.Assert.That(CompileLinkedConstant(assemblyPath), Is.EqualTo(37));
            // Roslyn's preserved fallback lazily maps linked modules. Keep this
            // test's snapshot out of the process-lifetime cache so the temporary
            // netmodule can be removed after its reference has been collected.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        [Test]
        public void MalformedReferenceKeepsCompilerOwnedDiagnostics()
        {
            var path = Path.Combine(_directory, "Invalid.dll");
            File.WriteAllText(path, "This is not a portable executable.");
            using var compiler = new NativeCompiler();
            compiler.AddDll(path);
            var error = NUnit.Framework.Assert.Throws<CompilationException>(() =>
                compiler.Compile("public static class Probe { public static int Run() => 1; }"));
            NUnit.Framework.Assert.That(ErrorCodes(error), Does.Contain("CS0009"));
        }

        [Test]
        public void ImportedGenericConstraintsAreChecked()
        {
            var path = Path.Combine(_directory, "Constraint.dll");
            WriteAssembly(path, "public class SnapshotConstraint<T> where T : class { }");
            using var compiler = new NativeCompiler();
            compiler.AddDll(path);
            NUnit.Framework.Assert.That(compiler.Compile(
                "public class Probe { public SnapshotConstraint<string> Value; }"), Is.Not.Null);
            var error = NUnit.Framework.Assert.Throws<CompilationException>(() => compiler.Compile(
                "public class Probe { public SnapshotConstraint<int> Value; }"));
            NUnit.Framework.Assert.That(ErrorCodes(error), Does.Contain("CS0452"));
        }

        [Test]
        public void ImportedAttributesRetainCompilerErrors()
        {
            var path = Path.Combine(_directory, "Attribute.dll");
            WriteAssembly(path,
                "[System.Obsolete(\"Snapshot attribute marker\", true)] public class SnapshotObsolete { }");
            using var compiler = new NativeCompiler();
            compiler.AddDll(path);
            var error = NUnit.Framework.Assert.Throws<CompilationException>(() => compiler.Compile(
                "public class Probe { public SnapshotObsolete Value; }"));
            NUnit.Framework.Assert.That(ErrorCodes(error), Does.Contain("CS0619"));
            NUnit.Framework.Assert.That(error.Errors.Values.SelectMany(errors => errors)
                .Any(item => item.ErrorText.Contains("Snapshot attribute marker")), Is.True);
        }

        private static int Run(NativeCompiler compiler, string source)
        {
            var assembly = compiler.Compile(source);
            return (int)assembly.GetType("Probe").GetMethod("Run").Invoke(null, null);
        }

        private static IEnumerable<string> ErrorCodes(CompilationException error) =>
            error.Errors.Values.SelectMany(errors => errors).Where(item => !item.IsWarning).Select(item => item.ErrorNumber);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CompileLinkedConstant(string path)
        {
            var create = typeof(NativeCompiler).GetMethod("CreateReferenceSnapshot", BindingFlags.NonPublic | BindingFlags.Static);
            var reference = (PortableExecutableReference)create.Invoke(null, new object[] { path });
            var compilation = CSharpCompilation.Create("LinkedProbe",
                new[] { CSharpSyntaxTree.ParseText("public static class Probe { public static int Run() => LinkedSnapshotValue.Value; }") },
                new MetadataReference[] { RuntimeReference, reference },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var output = new MemoryStream();
            var result = compilation.Emit(output);
            NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
            return (int)Assembly.Load(output.ToArray()).GetType("Probe").GetMethod("Run").Invoke(null, null);
        }

        private static void WriteAssembly(string path, string source,
            OutputKind kind = OutputKind.DynamicallyLinkedLibrary, params MetadataReference[] additionalReferences)
        {
            var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(path),
                new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14)) },
                new MetadataReference[] { RuntimeReference }.Concat(additionalReferences),
                new CSharpCompilationOptions(kind, optimizationLevel: OptimizationLevel.Release));
            using var output = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            var result = compilation.Emit(output);
            NUnit.Framework.Assert.That(result.Success, Is.True,
                string.Join(Environment.NewLine, result.Diagnostics));
        }
    }
}
