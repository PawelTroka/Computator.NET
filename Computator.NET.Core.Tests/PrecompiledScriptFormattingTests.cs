using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.Builtins;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.Functions;
using Computator.NET.DataTypes.SettingsTypes;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Complex = System.Numerics.Complex;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Core.Tests
{
    [TestFixture, NonParallelizable, Apartment(ApartmentState.STA)]
    public class PrecompiledScriptFormattingTests
    {
        private sealed class LegacyScriptEvaluator : ScriptEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        // Reproduce the current implementation's compact helper types while
        // leaving all formatting bodies in the generated program. A subclass
        // retains the original script prelude through the signature hook.
        private sealed class BaselineFormattingScriptEvaluator : ScriptEvaluator
        {
            protected override string GetAdditionalObjectsCode(bool usingPrecompiledBuiltins)
            {
                if (usingPrecompiledBuiltins && FunctionType == FunctionType.Scripting)
                    return (Computator.NET.Core.Evaluation.NumericalExtensions.ToCode + PrecompiledScriptObjects.ToCode)
                        .Replace("Properties.Settings.Default.NumericalOutputNotation",
                            "Computator.NET.DataTypes.SettingsTypes.NumericalOutputNotationType." + Settings.Default.NumericalOutputNotation);
                return base.GetAdditionalObjectsCode(usingPrecompiledBuiltins);
            }
        }

        private sealed class DisplayProbe
        {
            public int Calls { get; private set; }
            public override string ToString() { Calls++; return "arbitrary object: żółć"; }
        }

        private NumericalOutputNotationType _notation;
        private CultureInfo _culture;
        private string _workingDirectory;

        [OneTimeSetUp]
        public void Initialize() => GSLInitializer.Initialize();

        [SetUp]
        public void SaveSettings()
        {
            _notation = Settings.Default.NumericalOutputNotation;
            _culture = CultureInfo.CurrentCulture;
            _workingDirectory = Environment.CurrentDirectory;
        }

        [TearDown]
        public void RestoreSettings()
        {
            Settings.Default.NumericalOutputNotation = _notation;
            CultureInfo.CurrentCulture = _culture;
            Environment.CurrentDirectory = _workingDirectory;
        }

        [Test]
        public void FormattingSourcesPreserveTheOriginalSignaturesConstantsAndOtherBodies()
        {
            string objects = OriginalObjects();
            var extensions = Class(objects, "ScriptingExtensions");
            var original = Class("class __Source {" + ScriptingFunctions.ToCode + "}", "__Source");
            var objectMethod = original.Members.OfType<MethodDeclarationSyntax>()
                .Single(method => method.Identifier.ValueText == "objectToString");
            var formattingMethods = extensions.Members.OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ValueText == "ToMathString").ToArray();
            NUnit.Framework.Assert.That(formattingMethods.Length, Is.EqualTo(4));
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.ScriptFunctions, Is.EqualTo(Hash(ScriptingFunctions.ToCode)));
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.ScriptObjectFormatting, Is.EqualTo(Hash(objectMethod.ToString())));
            NUnit.Framework.Assert.That(PrecompiledBuiltinSourceHashes.ScriptExtensionsFormatting,
                Is.EqualTo(Hash(string.Join("\0", formattingMethods.Select(method => method.ToString())))));

            foreach (string source in new[] { PrecompiledScriptFormatting.MathematicalFunctions,
                         PrecompiledScriptFormatting.EngineeringFunctions })
            {
                var actual = Class("class __Source {" + source + "}", "__Source");
                NUnit.Framework.Assert.That(actual.Members.Count, Is.EqualTo(original.Members.Count));
                for (int index = 0; index < original.Members.Count; index++)
                {
                    var expectedMember = original.Members[index];
                    var actualMember = actual.Members[index];
                    if (expectedMember is MethodDeclarationSyntax method && method.Identifier.ValueText == "objectToString")
                        NUnit.Framework.Assert.That(SyntaxFactory.AreEquivalent(method.WithBody(null),
                            ((MethodDeclarationSyntax)actualMember).WithBody(null)), Is.True);
                    else
                        NUnit.Framework.Assert.That(SyntaxFactory.AreEquivalent(expectedMember, actualMember), Is.True,
                            "The non-formatting prelude remains program-local: " + expectedMember.ToString());
                }
            }

            foreach (string source in new[] { PrecompiledScriptObjects.MathematicalObjects,
                         PrecompiledScriptObjects.EngineeringObjects })
            {
                var actual = Class(source, "ScriptingExtensions");
                NUnit.Framework.Assert.That(actual.Members.Count, Is.EqualTo(extensions.Members.Count));
                for (int index = 0; index < extensions.Members.Count; index++)
                {
                    var expected = (MethodDeclarationSyntax)extensions.Members[index];
                    var found = (MethodDeclarationSyntax)actual.Members[index];
                    NUnit.Framework.Assert.That(SyntaxFactory.AreEquivalent(expected.WithBody(null), found.WithBody(null)), Is.True);
                    if (expected.Identifier.ValueText != "ToMathString")
                        NUnit.Framework.Assert.That(SyntaxFactory.AreEquivalent(expected, found), Is.True,
                            "The six small extension bodies are unchanged: " + expected.Identifier.ValueText);
                }
            }
        }

        [TestCase(NumericalOutputNotationType.MathematicalNotation)]
        [TestCase(NumericalOutputNotationType.EngineeringNotation)]
        public void MatricesVectorsScalarsNullAndArbitraryObjectsMatchTheOriginalFormatter(NumericalOutputNotationType notation)
        {
            Settings.Default.NumericalOutputNotation = notation;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");
            const string custom = "public static object suppliedValue;";
            var actual = new ScriptEvaluator().Evaluate("write(suppliedValue);", custom);
            var expected = new LegacyScriptEvaluator().Evaluate("write(suppliedValue);", custom);
            AssertUsesFormatting(actual);
            object[] values =
            {
                Matrix<double>.Build.DenseOfArray(new[,] { { 3e20, double.Epsilon }, { -2.5, double.PositiveInfinity } }),
                Vector<double>.Build.DenseOfArray(new[] { double.NaN, -0.0, 3e20 }),
                Matrix<Complex>.Build.DenseOfArray(new[,] { { Complex.ImaginaryOne, new Complex(3e20, -2.5) } }),
                Vector<Complex>.Build.DenseOfArray(new[] { Complex.Zero, new Complex(double.NaN, double.PositiveInfinity) }),
                null, 3e20, double.NegativeInfinity, 1.232223e-21m,
                new Complex(0, -1), "a string", new DisplayProbe()
            };
            foreach (object value in values)
            {
                SetValue(actual, value);
                SetValue(expected, value);
                NUnit.Framework.Assert.That(Run(actual), Is.EqualTo(Run(expected)), value?.GetType().FullName ?? "null");
            }
            NUnit.Framework.Assert.That(((DisplayProbe)values.Last()).Calls, Is.EqualTo(2));
            SetValue(actual, null);
            NUnit.Framework.Assert.That(Run(actual), Is.EqualTo(new[] { string.Empty }));
            SetValue(actual, 3e20);
            NUnit.Framework.Assert.That(Run(actual), Is.EqualTo(new[] {
                notation == NumericalOutputNotationType.MathematicalNotation ? "3·10²⁰" : "3E+20" }));
        }

        [TestCase(NumericalOutputNotationType.MathematicalNotation)]
        [TestCase(NumericalOutputNotationType.EngineeringNotation)]
        public void AllFourFormattingOverloadsRemainUsableAsExtensionMethodsAndMethodGroups(NumericalOutputNotationType notation)
        {
            Settings.Default.NumericalOutputNotation = notation;
            const string source = @"
                System.Func<double,string> realFormat=ScriptingExtensions.ToMathString;
                System.Func<decimal,string> decimalFormat=ScriptingExtensions.ToMathString;
                System.Func<System.Numerics.Complex,string> complexFormat=ScriptingExtensions.ToMathString;
                System.Func<string,string> stringFormat=ScriptingExtensions.ToMathString;
                double realValue=3e20;
                decimal decimalValue=decimal.Parse(""1.232223e-21"",System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture);
                var complexValue=new System.Numerics.Complex(0,1);
                string stringValue=""3E+20"";
                write(realFormat(realValue)); write(realValue.ToMathString());
                write(decimalFormat(decimalValue)); write(decimalValue.ToMathString());
                write(complexFormat(complexValue)); write(complexValue.ToMathString());
                write(stringFormat(stringValue)); write(stringValue.ToMathString());";
            var actual = new ScriptEvaluator().Evaluate(source, "");
            AssertUsesFormatting(actual);
            var output = Run(actual);
            NUnit.Framework.Assert.That(output, Is.EqualTo(Run(new LegacyScriptEvaluator().Evaluate(source, ""))));
            if (notation == NumericalOutputNotationType.MathematicalNotation)
                NUnit.Framework.Assert.That(output, Is.EqualTo(new[] { "3·10²⁰", "3·10²⁰", "1.232223·10⁻²¹", "1.232223·10⁻²¹",
                    "i", "i", "3·10²⁰", "3·10²⁰" }));
            else
                NUnit.Framework.Assert.That(output.Take(4), Is.EqualTo(new[] { "3E+20", "3E+20", "1.232223E-21", "1.232223E-21" }));
        }

        [Test]
        public void CustomFormatterOverloadsAndOriginalPrivateConstantsKeepTheirBindings()
        {
            Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;
            const string custom = @"
                public static string objectToString(int value) { return ""custom-object:""+value; }
                public static string ToMathString(double value) { return ""custom-number""; }";
            const string source = "double value=3e20; write(objectToString(7)); write(ToMathString(value)); " +
                "write(value.ToMathString()); write(maxWidth); write(maxPerColumnOrRow);";
            var actual = new ScriptEvaluator().Evaluate(source, custom);
            AssertUsesFormatting(actual);
            var output = new[] { "custom-object:7", "custom-number", "3·10²⁰", "80", "999999" };
            NUnit.Framework.Assert.That(Run(actual), Is.EqualTo(output));
            NUnit.Framework.Assert.That(Run(new LegacyScriptEvaluator().Evaluate(source, custom)), Is.EqualTo(output));
        }

        [Test]
        public void LocalFormatterNamesDoNotAlterBindingsInsideTheFixedFormatter()
        {
            Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;
            const string source = @"
                int maxWidth=11; int maxPerColumnOrRow=12;
                System.Func<object,string> objectToString=item=>""local-object"";
                System.Func<double,string> ToMathString=item=>""local-number"";
                double value=3e20;
                write(maxWidth); write(maxPerColumnOrRow); write(objectToString(null));
                write(ToMathString(value)); write(value);";
            var actual = new ScriptEvaluator().Evaluate(source, "");
            AssertUsesHelpers(actual);
            var output = new[] { "11", "12", "local-object", "local-number", "3·10²⁰" };
            NUnit.Framework.Assert.That(Run(actual), Is.EqualTo(output));
            NUnit.Framework.Assert.That(Run(new LegacyScriptEvaluator().Evaluate(source, "")), Is.EqualTo(output));
        }

        [TestCase("maxWidth")]
        [TestCase("maxPerColumnOrRow")]
        public void DuplicatePrivateConstantNamesRemainCompilationErrors(string name)
        {
            string custom = "private const int " + name + "=17;";
            var actual = NUnit.Framework.Assert.Throws<CompilationException>(() => new ScriptEvaluator().Evaluate("write(1);", custom));
            var expected = NUnit.Framework.Assert.Throws<CompilationException>(() => new BaselineFormattingScriptEvaluator().Evaluate("write(1);", custom));
            NUnit.Framework.Assert.That(Diagnostics(actual), Is.EqualTo(Diagnostics(expected)));
            NUnit.Framework.Assert.That(actual.Errors.SelectMany(entry => entry.Value).Any(error => error.ErrorNumber == "CS0102"), Is.True);
        }

        [Test]
        public void ACustomSystemMemberRetainsOriginalNamespaceShadowingDiagnostics()
        {
            // Existing generated method bodies bind System.Environment and
            // System.Math in the containing class, where a custom member can
            // shadow the namespace. Extracting a body must not hide that error.
            const string custom = "public static string System;";
            var actual = NUnit.Framework.Assert.Throws<CompilationException>(() => new ScriptEvaluator().Evaluate("write(1);", custom));
            var expected = NUnit.Framework.Assert.Throws<CompilationException>(() => new BaselineFormattingScriptEvaluator().Evaluate("write(1);", custom));
            NUnit.Framework.Assert.That(Diagnostics(actual), Is.EqualTo(Diagnostics(expected)));
        }

        [Test]
        public void OneEvaluatorCapturesNotationSeparatelyForEachCompiledProgram()
        {
            var evaluator = new ScriptEvaluator();
            Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;
            var mathematical = evaluator.Evaluate("write(3e20);", "");
            Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.EngineeringNotation;
            var engineering = evaluator.Evaluate("write(3e20);", "");
            AssertUsesFormatting(mathematical);
            AssertUsesFormatting(engineering);
            Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.MathematicalNotation;
            NUnit.Framework.Assert.That(Run(engineering), Is.EqualTo(new[] { "3E+20" }));
            NUnit.Framework.Assert.That(Run(mathematical), Is.EqualTo(new[] { "3·10²⁰" }));
            Settings.Default.NumericalOutputNotation = NumericalOutputNotationType.EngineeringNotation;
            NUnit.Framework.Assert.That(Run(mathematical), Is.EqualTo(new[] { "3·10²⁰" }));
        }

        [Test]
        public void AnUnknownNotationRetainsTheOriginalCompilationFailure()
        {
            Settings.Default.NumericalOutputNotation = (NumericalOutputNotationType)123;
            var actual = NUnit.Framework.Assert.Throws<CompilationException>(() => new ScriptEvaluator().Evaluate("write(3e20);", ""));
            var expected = NUnit.Framework.Assert.Throws<CompilationException>(() => new BaselineFormattingScriptEvaluator().Evaluate("write(3e20);", ""));
            NUnit.Framework.Assert.That(Diagnostics(actual), Is.EqualTo(Diagnostics(expected)));
        }

        [Test]
        public void SharedMatrixFormattingDelegatesDoNotRetainDiscardedPrograms()
        {
            var reference = ReleasedFormattedProgram();
            for (int pass = 0; pass < 20 && reference.IsAlive; pass++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            NUnit.Framework.Assert.That(reference.IsAlive, Is.False);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference ReleasedFormattedProgram()
        {
            var function = new ScriptEvaluator().Evaluate("write(suppliedValue);", "public static object suppliedValue;");
            AssertUsesFormatting(function);
            SetValue(function, Matrix<double>.Build.DenseOfArray(new[,] { { 3e20, 1.0 } }));
            NUnit.Framework.Assert.That(Run(function).Single(), Is.Not.Empty);
            var context = AssemblyLoadContext.GetLoadContext(Program(function).Method.Module.Assembly);
            NUnit.Framework.Assert.That(context.IsCollectible, Is.True);
            return new WeakReference(context);
        }

        private static Delegate Program(BaseFunction function) => (Delegate)typeof(BaseFunction)
            .GetField("_function", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(function);
        private static void SetValue(BaseFunction function, object value) => Program(function).Method.DeclaringType
            .GetField("suppliedValue", BindingFlags.Public | BindingFlags.Static).SetValue(null, value);
        private static void AssertUsesHelpers(BaseFunction function) => NUnit.Framework.Assert.That(
            Program(function).Method.Module.Assembly.GetType("FunctionsCreatorNS.File", true).BaseType,
            Is.EqualTo(typeof(PrecompiledScriptFile)));
        private static void AssertUsesFormatting(BaseFunction function)
        {
            AssertUsesHelpers(function);
            NUnit.Framework.Assert.That(Program(function).Method.DeclaringType
                .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Any(type => type.Name.StartsWith("<>c", StringComparison.Ordinal)),
                Is.False, "Fixed formatting lambdas must not be emitted into each script program.");
        }
        private static List<string> Run(ScriptFunction function)
        {
            var output = new List<string>();
            function.Evaluate(output.Add);
            return output;
        }
        private static string[] Diagnostics(CompilationException error) => error.Errors.SelectMany(entry => entry.Value
                .Where(item => !item.IsWarning).Select(item => entry.Key + ":" + item.ErrorNumber + ":" + item.ErrorText +
                    ":" + item.Line + ":" + item.Column))
            .OrderBy(value => value, StringComparer.Ordinal).ToArray();
        private static string OriginalObjects() => (string)typeof(ScriptEvaluator).Assembly
            .GetType("Computator.NET.Core.Evaluation.ScriptingExtensionObjects", true)
            .GetField("ToCode", BindingFlags.Public | BindingFlags.Static).GetRawConstantValue();
        private static ClassDeclarationSyntax Class(string source, string name) => CSharpSyntaxTree.ParseText(source).GetRoot()
            .DescendantNodes().OfType<ClassDeclarationSyntax>().Single(type => type.Identifier.ValueText == name);
        private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }
}
