using System;
using System.Linq;
using System.Threading.Tasks;
using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.CompilationTests
{
    [TestFixture]
    public class RoslynMigrationTests
    {
        private const string ValidSource = "public static class Probe { public static int Run() => 8; }";

        [Test]
        public void CompilesCSharp14WithDynamicBinding()
        {
            using (var compiler = new NativeCompiler())
            {
                var assembly = compiler.Compile(@"public class Probe {
                    public int Value { get; set => field = value < 0 ? 0 : value; }
                    public static int Run() { dynamic item = new Probe { Value = 42 }; return item.Value; }
                }");
                NUnit.Framework.Assert.That(assembly.GetType("Probe").GetMethod("Run").Invoke(null, null), Is.EqualTo(42));
            }
        }

        [Test]
        public void ReferencesCurrentNumericalPackagesAndNetStandardFacades()
        {
            using (var compiler = new NativeCompiler())
            {
                var assembly = compiler.Compile(@"public static class Probe {
                    public static double Run() =>
                        MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.CreateIdentity(2).Determinant()
                        + Meta.Numerics.Functions.AdvancedMath.Gamma(3.0);
                }");
                NUnit.Framework.Assert.That(assembly.GetType("Probe").GetMethod("Run").Invoke(null, null), Is.EqualTo(3.0));
            }
        }

        [Test]
        public void RepeatedCompilationUsesDistinctAssemblies()
        {
            using (var compiler = new NativeCompiler())
            {
                var first = compiler.Compile(ValidSource);
                var second = compiler.Compile(ValidSource.Replace("8", "9"));
                NUnit.Framework.Assert.That(second.FullName, Is.Not.EqualTo(first.FullName));
                NUnit.Framework.Assert.That(first.GetType("Probe").GetMethod("Run").Invoke(null, null), Is.EqualTo(8));
                NUnit.Framework.Assert.That(second.GetType("Probe").GetMethod("Run").Invoke(null, null), Is.EqualTo(9));
            }
        }

        [Test]
        public void IndependentCompilationsCanRunConcurrently()
        {
            using (var compiler = new NativeCompiler())
                Parallel.For(0, 8, _ => NUnit.Framework.Assert.That(compiler.Compile(ValidSource), Is.Not.Null));
        }

        [TestCase(CompilationErrorPlace.MainCode)]
        [TestCase(CompilationErrorPlace.CustomFunctions)]
        public void DiagnosticsRetainEditorRelativeLinesAndCompilerCodes(CompilationErrorPlace place)
        {
            using (var compiler = new NativeCompiler())
            {
                if (place == CompilationErrorPlace.MainCode)
                {
                    compiler.MainCodeStarOffsetLine = 2;
                    compiler.MainCodeEndOffsetLine = 4;
                }
                else
                {
                    compiler.CustomFunctionsStartOffsetLine = 2;
                    compiler.CustomFunctionsEndOffsetLine = 4;
                }
                var exception = NUnit.Framework.Assert.Throws<CompilationException>(() => compiler.Compile(
                    "public class Probe\n{\n public int Run() => missing;\n}"));
                var error = exception.Errors[place].Single(e => !e.IsWarning);
                NUnit.Framework.Assert.That(error.ErrorNumber, Is.EqualTo("CS0103"));
                NUnit.Framework.Assert.That(error.Line, Is.EqualTo(1));
                NUnit.Framework.Assert.That(error.Column, Is.GreaterThan(0));
                NUnit.Framework.Assert.That(compiler.Compile(ValidSource), Is.Not.Null);
            }
        }

        [Test]
        public void WarningsDoNotPreventCompilation()
        {
            using (var compiler = new NativeCompiler())
                NUnit.Framework.Assert.That(compiler.Compile(
                    "public class Probe { public void Run() { int unused; } }"), Is.Not.Null);
        }

        [Test]
        public void RejectsEmptyReferenceNames()
        {
            using (var compiler = new NativeCompiler())
                NUnit.Framework.Assert.Throws<ArgumentException>(() => compiler.AddDll(" "));
        }
    }
}
