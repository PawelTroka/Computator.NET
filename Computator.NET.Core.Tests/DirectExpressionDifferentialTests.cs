using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            if (choice < 4) return "("+Expression(random,depth-1)+"+-*/"[choice]+Expression(random,depth-1)+")";
            if (choice == 4) return "-("+Expression(random,depth-1)+")";
            return (choice == 5 ? "sin(" : "cos(")+Expression(random,depth-1)+")";
        }
        public static IEnumerable<TestCaseData> GeneratedPrograms()
        {
            var random = new Random(191);
            for (int index = 0; index < 128; index++)
                yield return new TestCaseData(Expression(random, 3)).SetName("Direct numeric differential program "+index);
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
