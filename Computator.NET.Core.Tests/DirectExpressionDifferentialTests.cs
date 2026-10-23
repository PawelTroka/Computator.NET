using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
{
    [TestFixture, NonParallelizable]
    public class DirectExpressionDifferentialTests
    {
        private sealed class OriginalCompilerEvaluator : ExpressionsEvaluator
        { protected override bool UseDirectExpressions => false; }
        private sealed class SpecializedEvaluator : ExpressionsEvaluator { }
        private sealed class OptedInEvaluator : ExpressionsEvaluator
        { protected override bool UseDirectExpressions => true; }
        private sealed class MissingReferenceEvaluator : ExpressionsEvaluator
        {
            internal MissingReferenceEvaluator() => NativeCompiler.AddDll(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")+".dll"));
        }
        private static bool IsDirect(BaseFunction f) => ((Delegate)typeof(BaseFunction).GetField("_function", BindingFlags.Instance|BindingFlags.NonPublic).GetValue(f)).Method.Module.Assembly.GetName().Name.StartsWith("Computator.Direct.");

        [Test]
        public void SpecializedEvaluatorsRetainTheirCompilerUnlessExplicitlyOptedIn()
        {
            NUnit.Framework.Assert.That(IsDirect(new SpecializedEvaluator().Evaluate("sin(x)", "", CalculationsMode.Real)), Is.False);
            NUnit.Framework.Assert.That(IsDirect(new OptedInEvaluator().Evaluate("sin(x)", "", CalculationsMode.Real)), Is.True);
            var error = NUnit.Framework.Assert.Throws<EvaluationException>(() => new MissingReferenceEvaluator().Evaluate("sin(x)", "", CalculationsMode.Real));
            NUnit.Framework.Assert.That(error.InnerException, Is.TypeOf<FileNotFoundException>());
        }

        private static string Expression(Random random, int depth)
        {
            if (depth == 0) return new[] { "x", "0.0", "0.125", "0.5", "1.0", "2.0" }[random.Next(6)];
            int choice = random.Next(7);
            // Separate the right operand syntactically. Concatenating a binary
            // minus and a unary minus produces the C# decrement token, not a
            // subtraction followed by negation. Those invalid original cases
            // remain explicitly covered below rather than being discarded.
            if (choice < 4) return "("+Expression(random,depth-1)+"+-*/"[choice]+"("+Expression(random,depth-1)+"))";
            if (choice == 4) return "-("+Expression(random,depth-1)+")";
            return (choice == 5 ? "sin(" : "cos(")+Expression(random,depth-1)+")";
        }
        public static IEnumerable<TestCaseData> GeneratedPrograms()
        {
            var random = new Random(191);
            for (int index = 0; index < 128; index++)
                yield return new TestCaseData(Expression(random, 3)).SetName("Direct numeric differential program "+index);
        }

        // These are all ten failing inputs from the original seed-191 corpus.
        // Neither compiler may silently reinterpret a decrement as subtraction.
        // Keep complete error locations and identifiers, not only an exception.
        [TestCase("((cos(1.0)/(0.0+0.125))*((0.5/x)--(0.125)))")]
        [TestCase("cos((-(x)--(0.5)))")]
        [TestCase("(((2.0-0.125)--(0.0))+((x*1.0)*-(x)))")]
        [TestCase("(cos(-(x))*((2.0+0.0)--(2.0)))")]
        [TestCase("(sin((2.0*1.0))--((0.125-x)))")]
        [TestCase("(((0.0*x)--(0.5))*cos((0.125+1.0)))")]
        [TestCase("(((x-0.125)-(0.5-x))--((0.125+2.0)))")]
        [TestCase("(((0.0*0.125)--(0.5))-(-(0.5)*(0.5/2.0)))")]
        [TestCase("(((0.125-1.0)--(1.0))+cos((1.0+2.0)))")]
        [TestCase("sin(((0.5-x)--(x)))")]
        public void OriginalInvalidGeneratedInputsKeepFullCompilerDiagnostics(string source)
        {
            var expected = NUnit.Framework.Assert.Throws<CompilationException>(() =>
                new OriginalCompilerEvaluator().Evaluate(source, "", CalculationsMode.Real));
            var actual = NUnit.Framework.Assert.Throws<CompilationException>(() =>
                new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real));
            NUnit.Framework.Assert.That(actual.HasMainCodeErrors, Is.True, source);
            foreach (var place in expected.Errors.Keys)
                NUnit.Framework.Assert.That(actual.Errors[place].Select(error =>
                        (error.ErrorNumber, error.Line, error.Column, error.IsWarning)).ToArray(),
                    Is.EqualTo(expected.Errors[place].Select(error =>
                        (error.ErrorNumber, error.Line, error.Column, error.IsWarning)).ToArray()), source);
        }

        [TestCaseSource(nameof(GeneratedPrograms))]
        public void GeneratedProgramsRetainExactRuntimeResultsAndCanonicalSource(string source)
        {
            var program = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(IsDirect(program), Is.True, source);
            var original = new OriginalCompilerEvaluator();
            Func<double,double> actual = program;
            Func<double,double> expected = original.Evaluate(source, "", CalculationsMode.Real);
            foreach (double x in new[] { -3.0, -.125, -0.0, 0.0, .125, 3.0, double.Epsilon, double.MaxValue, double.PositiveInfinity, double.NegativeInfinity, double.NaN })
            {
                double a=actual(x), b=expected(x);
                if (double.IsNaN(b)) NUnit.Framework.Assert.That(double.IsNaN(a), Is.True, source);
                else NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(a), Is.EqualTo(BitConverter.DoubleToInt64Bits(b)), source);
            }
            // Canonical CsCode is explicitly documented as an equivalent C#
            // expression, not a second TSL input (which would promote division again).
            NUnit.Framework.Assert.That(program.TslCode, Is.EqualTo(source));
            NUnit.Framework.Assert.That(program.CsCode, Is.Not.Empty);
        }
    }
}
