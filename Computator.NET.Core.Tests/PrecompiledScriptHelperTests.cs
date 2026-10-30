using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.Functions;
using Computator.NET.DataTypes.SettingsTypes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using File = System.IO.File;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Core.Tests
{
    [TestFixture, NonParallelizable, Apartment(ApartmentState.STA)]
    public class PrecompiledScriptHelperTests
    {
        private sealed class LegacyScriptEvaluator : ScriptEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        private sealed class SpecializedScriptEvaluator : ScriptEvaluator { }
        private string _directory;
        private string _workingDirectory;

        [OneTimeSetUp]
        public void Initialize() => GSLInitializer.Initialize();

        [SetUp]
        public void Prepare()
        {
            _workingDirectory = Environment.CurrentDirectory;
            _directory = Path.Combine(Path.GetTempPath(), "Computator.ScriptHelpers." + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void Restore()
        {
            Environment.CurrentDirectory = _workingDirectory;
            Directory.Delete(_directory, recursive: true);
        }

        [Test]
        public void HelperSourcesAndRemainingFormattingMatchTheOriginalFragments()
        {
            var original = (string)typeof(ScriptEvaluator).Assembly
                .GetType("Computator.NET.Core.Evaluation.ScriptingExtensionObjects", true)
                .GetField("ToCode", BindingFlags.Public | BindingFlags.Static).GetRawConstantValue();
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.ScriptObjects, Is.EqualTo(Hash(original)));
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.ScriptTypeDeducer,
                Is.EqualTo(Hash(ClassSource(original, "TypeDeducer"))));
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.ScriptFile,
                Is.EqualTo(Hash(ClassSource(original, "File"))));
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.ScriptReadForm,
                Is.EqualTo(Hash(ClassSource(original, "ReadForm"))));
            NUnit.Framework.Assert.That(ClassSource(PrecompiledScriptObjects.ToCode, "ScriptingExtensions"),
                Is.EqualTo(ClassSource(original, "ScriptingExtensions")));
        }

        [Test]
        public void HelpersKeepDistinctProgramTypesAndProgramOwnedCallbackState()
        {
            var first = new ScriptEvaluator().Evaluate("write(1);", "");
            var second = new ScriptEvaluator().Evaluate("write(2);", "");
            foreach (var name in new[] { "TypeDeducer", "File", "ReadForm" })
            {
                var a = Helper(first, name);
                var b = Helper(second, name);
                NUnit.Framework.Assert.That(a, Is.Not.SameAs(b));
                NUnit.Framework.Assert.That(a.BaseType, Is.SameAs(b.BaseType));
                NUnit.Framework.Assert.That(a.BaseType.Assembly, Is.SameAs(typeof(ScriptEvaluator).Assembly));
            }
            NUnit.Framework.Assert.That(Helper(first, "ReadForm").IsSealed, Is.True);
            var creator = Program(first).Method.DeclaringType;
            const BindingFlags fields = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
            NUnit.Framework.Assert.That(creator.GetField("CONSOLE_OUTPUT", fields), Is.Not.Null);
            NUnit.Framework.Assert.That(creator.GetField("_chartFactory", fields), Is.Not.Null);
            NUnit.Framework.Assert.That(Run(first), Is.EqualTo(new[] { "1" }));
            NUnit.Framework.Assert.That(Run(second), Is.EqualTo(new[] { "2" }));
        }

        [Test]
        public void EveryTypeDeducerArityKeepsTheOriginalDelegateAndValue()
        {
            var script = new StringBuilder();
            var expected = new List<string>();
            for (int count = 0; count <= 8; count++)
            {
                string suffix = ((char)('A' + count)).ToString();
                string[] arguments = Enumerable.Range(0, count).Select(index => "arg" + (char)('A' + index)).ToArray();
                string value = count == 0 ? "7" : "7+" + string.Join("+", arguments);
                script.Append("System.Func<").Append(string.Join(",", Enumerable.Repeat("int", count + 1)))
                    .Append("> original").Append(suffix).Append("=(").Append(string.Join(",", arguments))
                    .Append(")=>").Append(value).Append("; var found").Append(suffix)
                    .Append("=TypeDeducer.Func(original").Append(suffix).Append(");")
                    .Append("write(System.Object.ReferenceEquals(original").Append(suffix).Append(",found").Append(suffix)
                    .Append(")); write(found").Append(suffix).Append("(")
                    .Append(string.Join(",", Enumerable.Range(1, count))).Append("));");
                expected.Add("True");
                expected.Add((7 + count * (count + 1) / 2).ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            var fast = new ScriptEvaluator().Evaluate(script.ToString(), "");
            AssertUsesHelpers(fast);
            NUnit.Framework.Assert.That(Run(fast), Is.EqualTo(expected));
            NUnit.Framework.Assert.That(Run(new LegacyScriptEvaluator().Evaluate(script.ToString(), "")), Is.EqualTo(expected));
        }

        [Test]
        public void FileReadWriteClearAndIndependentInstancesMatchTheLegacyProgram()
        {
            var expected = new[] { "alpha", "beta", "independent", "reset" };
            foreach (bool legacy in new[] { false, true })
            {
                string prefix = legacy ? "legacy" : "precompiled";
                string firstPath = Path.Combine(_directory, prefix + "-first.txt");
                string secondPath = Path.Combine(_directory, prefix + "-second.txt");
                string source = "var first=File(" + Literal(firstPath) + "); var second=File(" + Literal(secondPath) + ");" +
                    "try { first.clear(); second.clear(); first.writeln(\"alpha\"); first.write(\"beta\");" +
                    "second.write(\"independent\"); write(first.readln()); write(first.readAll()); write(second.readAll());" +
                    "first.clear(); first.write(\"reset\"); write(first.readAll()); } finally { first.close(); second.close(); }";
                var function = (legacy ? (ScriptEvaluator)new LegacyScriptEvaluator() : new ScriptEvaluator()).Evaluate(source, "");
                if (!legacy) AssertUsesHelpers(function);
                NUnit.Framework.Assert.That(Run(function), Is.EqualTo(expected));
                NUnit.Framework.Assert.That(File.ReadAllText(firstPath), Is.EqualTo("reset"));
                NUnit.Framework.Assert.That(File.ReadAllText(secondPath), Is.EqualTo("independent"));
                using var exclusive = new FileStream(firstPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
        }

        [Test]
        public void ReadFormConstructsEquivalentControlsAndDisposesWithoutShowingADialog()
        {
            var fast = new ScriptEvaluator().Evaluate("write(1);", "");
            var legacy = new LegacyScriptEvaluator().Evaluate("write(1);", "");
            AssertUsesHelpers(fast);
            var actual = (Form)Activator.CreateInstance(Helper(fast, "ReadForm"), "Polski: podaj wartość");
            var expected = (Form)Activator.CreateInstance(Helper(legacy, "ReadForm"), "Polski: podaj wartość");
            try
            {
                NUnit.Framework.Assert.That(actual.Visible, Is.False);
                NUnit.Framework.Assert.That(actual.Text, Is.EqualTo(expected.Text));
                NUnit.Framework.Assert.That(actual.FormBorderStyle, Is.EqualTo(expected.FormBorderStyle));
                NUnit.Framework.Assert.That(actual.StartPosition, Is.EqualTo(expected.StartPosition));
                NUnit.Framework.Assert.That(actual.Controls.Cast<Control>().Select(control => control.Name),
                    Is.EqualTo(expected.Controls.Cast<Control>().Select(control => control.Name)));
                NUnit.Framework.Assert.That(actual.Controls["readQuestion"].Text, Is.EqualTo("Polski: podaj wartość"));
                actual.Controls["textBox1"].Text = "42.5";
                expected.Controls["textBox1"].Text = "42.5";
                NUnit.Framework.Assert.That(Helper(fast, "ReadForm").GetProperty("Result").GetValue(actual),
                    Is.EqualTo(Helper(legacy, "ReadForm").GetProperty("Result").GetValue(expected)));
                NUnit.Framework.Assert.That(actual.AcceptButton.DialogResult, Is.EqualTo(DialogResult.OK));
            }
            finally
            {
                actual.Dispose();
                expected.Dispose();
            }
            NUnit.Framework.Assert.That(actual.IsDisposed, Is.True);
            NUnit.Framework.Assert.That(expected.IsDisposed, Is.True);
        }

        [TestCase("write(typeof(File).BaseType.FullName);", "", "System.Object")]
        [TestCase("write(TypeDeducer.Func(3));",
            "public class TypeDeducer { public static int Func(int value) { return value+1; } }", "4")]
        public void ReflectionAndCustomHelperTypesKeepTheOriginalRepresentation(string source, string custom, string expected)
        {
            var function = new ScriptEvaluator().Evaluate(source, custom);
            NUnit.Framework.Assert.That(Helper(function, "File").BaseType, Is.EqualTo(typeof(object)));
            NUnit.Framework.Assert.That(Run(function), Is.EqualTo(new[] { expected }));
            NUnit.Framework.Assert.That(Run(new LegacyScriptEvaluator().Evaluate(source, custom)), Is.EqualTo(new[] { expected }));
        }

        [Test]
        public void SpecializedEvaluatorsRetainTheirOriginalHelperSource()
        {
            var function = new SpecializedScriptEvaluator().Evaluate("write(3);", "");
            NUnit.Framework.Assert.That(Helper(function, "File").BaseType, Is.EqualTo(typeof(object)));
            NUnit.Framework.Assert.That(Run(function), Is.EqualTo(new[] { "3" }));
        }

        [Test]
        public void CustomScriptOverloadsKeepTheMergedMethodGroupAndProgramCallback()
        {
            const string custom = "public static void write(int value) { CONSOLE_OUTPUT(\"custom:\"+value); }";
            const string source = "write(7); write(\"text\");";
            var function = new ScriptEvaluator().Evaluate(source, custom);
            AssertUsesHelpers(function);
            NUnit.Framework.Assert.That(Run(function), Is.EqualTo(new[] { "custom:7", "text" }));
            NUnit.Framework.Assert.That(Run(new LegacyScriptEvaluator().Evaluate(source, custom)),
                Is.EqualTo(new[] { "custom:7", "text" }));
        }

        [TestCase("var file=File(\"unused\"); write(file.path);")]
        [TestCase("var form=new ReadForm(\"unused\"); form.InitializeComponent();")]
        public void PrivateHelperAccessRetainsOriginalErrorNamesAndLocations(string source)
        {
            var actual = NUnit.Framework.Assert.Throws<CompilationException>(() => new ScriptEvaluator().Evaluate(source, ""));
            var expected = NUnit.Framework.Assert.Throws<CompilationException>(() => new LegacyScriptEvaluator().Evaluate(source, ""));
            string[] Details(CompilationException error) => error.Errors.OrderBy(entry => entry.Key)
                .SelectMany(entry => entry.Value.Where(item => !item.IsWarning).Select(item =>
                    entry.Key + ":" + item.ErrorNumber + ":" + item.Line + ":" + item.Column + ":" + item.ErrorText)).ToArray();
            NUnit.Framework.Assert.That(Details(actual), Is.EqualTo(Details(expected)));
        }

        [Test]
        public void NotationSnapshotsAndCallbacksRemainIndependentAcrossPrograms()
        {
            var originalNotation = Settings.Default.NumericalOutputNotation;
            try
            {
                Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;
                var mathematical = new ScriptEvaluator().Evaluate("write(3e20);", "");
                var legacyMathematical = new LegacyScriptEvaluator().Evaluate("write(3e20);", "");
                Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.EngineeringNotation;
                var engineering = new ScriptEvaluator().Evaluate("write(3e20);", "");
                var legacyEngineering = new LegacyScriptEvaluator().Evaluate("write(3e20);", "");
                AssertUsesHelpers(mathematical);
                AssertUsesHelpers(engineering);
                var a = new List<string>();
                var b = new List<string>();
                mathematical.Evaluate(a.Add);
                engineering.Evaluate(b.Add);
                Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;
                mathematical.Evaluate(a.Add);
                engineering.Evaluate(b.Add);
                NUnit.Framework.Assert.That(a, Is.EqualTo(new[] { "3·10²⁰", "3·10²⁰" }));
                NUnit.Framework.Assert.That(b, Is.EqualTo(new[] { "3E+20", "3E+20" }));
                NUnit.Framework.Assert.That(Run(legacyMathematical), Is.EqualTo(new[] { "3·10²⁰" }));
                NUnit.Framework.Assert.That(Run(legacyEngineering), Is.EqualTo(new[] { "3E+20" }));
            }
            finally { Settings.Default.NumericalOutputNotation = originalNotation; }
        }

        [Test]
        public void PrecompiledHelperBasesDoNotRootDiscardedScriptPrograms()
        {
            var reference = ReleasedProgram();
            for (int pass = 0; pass < 20 && reference.IsAlive; pass++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            NUnit.Framework.Assert.That(reference.IsAlive, Is.False);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference ReleasedProgram()
        {
            var function = new ScriptEvaluator().Evaluate("var f=TypeDeducer.Func((int value)=>value+1); write(f(41));", "");
            AssertUsesHelpers(function);
            NUnit.Framework.Assert.That(Run(function), Is.EqualTo(new[] { "42" }));
            var context = AssemblyLoadContext.GetLoadContext(Program(function).Method.Module.Assembly);
            NUnit.Framework.Assert.That(context.IsCollectible, Is.True);
            return new WeakReference(context);
        }

        private static Delegate Program(BaseFunction function) => (Delegate)typeof(BaseFunction)
            .GetField("_function", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(function);
        private static Type Helper(BaseFunction function, string name) => Program(function).Method.Module.Assembly
            .GetType("FunctionsCreatorNS." + name, throwOnError: true);
        private static void AssertUsesHelpers(BaseFunction function) =>
            NUnit.Framework.Assert.That(Helper(function, "File").BaseType, Is.EqualTo(typeof(PrecompiledScriptFile)));
        private static List<string> Run(ScriptFunction function)
        {
            var output = new List<string>();
            function.Evaluate(output.Add);
            return output;
        }
        private static string Literal(string text) => "@\"" + text.Replace("\"", "\"\"") + "\"";
        private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
        private static string ClassSource(string text, string name) => CSharpSyntaxTree.ParseText(text).GetRoot()
            .DescendantNodes().OfType<ClassDeclarationSyntax>().Single(type => type.Identifier.ValueText == name).ToString();
    }
}
