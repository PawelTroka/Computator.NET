using System.Collections.Generic;
using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    public class PrefixedIntegerEvaluationTests
    {
        private readonly bool _fullSource;
        public PrefixedIntegerEvaluationTests(bool fullSource) { _fullSource = fullSource; }

        private sealed class FullSourceEvaluator : ExpressionsEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }
        private sealed class FullSourceScriptEvaluator : ScriptEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }

        [TestCase("0x10", 16)]
        [TestCase("0B1010", 10)]
        [TestCase("1/0x10", 0.0625)]
        [TestCase("1/-0b10UL", -0.5)]
        [TestCase("0x10²", 256)]
        [TestCase("0x10(x+1)", 48)]
        public void DesktopExpressionsUseTheWholePrefixedLiteral(string source, double expected)
        {
            ExpressionsEvaluator evaluator = _fullSource ? new FullSourceEvaluator() : new ExpressionsEvaluator();
            var function = evaluator.Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function.Evaluate(2.0), Is.EqualTo(expected).Within(1e-12));
        }

        [Test]
        public void DesktopScriptsPreserveTypesAndNeverReadSimilarlyNamedVariables()
        {
            const string source = """
                var x10=7; var b11=9;
                write(0x10); write(":"); write(0b11); write(":");
                write((0xFFFFFFFF).GetType().Name); write(":");
                write(0xFFFF_FFFF_FFFF_FFFFUL == ulong.MaxValue); write(":");
                var minimum=-0x8000000000000000;
                write(minimum.GetType().Name); write(":"); write(minimum == long.MinValue);
                """;
            ScriptEvaluator evaluator = _fullSource ? new FullSourceScriptEvaluator() : new ScriptEvaluator();
            var output = new List<string>();
            evaluator.Evaluate(source, "").Evaluate(output.Add);
            NUnit.Framework.Assert.That(string.Concat(output), Is.EqualTo("16:3:UInt32:True:Int64:True"));
        }

        [Test]
        public void CustomFunctionsAndComplexExpressionsShareTheSyntax()
        {
            const string definitions = "static real Probe(real x) { return 0x10(x+1) + 1/0b10; }";
            ExpressionsEvaluator evaluator = _fullSource ? new FullSourceEvaluator() : new ExpressionsEvaluator();
            var function = evaluator.Evaluate("Probe(2)+0b10*z", definitions, CalculationsMode.Complex);
            NUnit.Framework.Assert.That(function.Evaluate(new Complex(1, 2)), Is.EqualTo(new Complex(50.5, 4)));
        }
    }
}
