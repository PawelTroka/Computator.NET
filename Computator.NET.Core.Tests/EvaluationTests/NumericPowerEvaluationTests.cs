using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture]
    public class NumericPowerEvaluationTests
    {
        private sealed class FullSourceEvaluator : ExpressionsEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        [TestCase(false, ".5²", 0.25)]
        [TestCase(true, ".5²", 0.25)]
        [TestCase(false, "1_000²", 1000000)]
        [TestCase(true, "1_000²", 1000000)]
        [TestCase(false, "2e-3²", 0.000004)]
        [TestCase(true, "2e-3²", 0.000004)]
        public void DesktopCompilationUsesTheWholeNumericBase(bool fullSource, string source, double expected)
        {
            ExpressionsEvaluator evaluator = fullSource ? new FullSourceEvaluator() : new ExpressionsEvaluator();
            var function = evaluator.Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function.Evaluate(0.0), Is.EqualTo(expected).Within(2e-13).Percent);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CustomDefinitionsPreserveIdentifiersEndingInDigits(bool fullSource)
        {
            const string definitions = "static real Probe(real value2) { return value2² + .5²; }";
            ExpressionsEvaluator evaluator = fullSource ? new FullSourceEvaluator() : new ExpressionsEvaluator();
            var function = evaluator.Evaluate("Probe(x)", definitions, CalculationsMode.Real);
            NUnit.Framework.Assert.That(function.Evaluate(3.0), Is.EqualTo(9.25));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NumericPowersComposeWithComplexPowers(bool fullSource)
        {
            ExpressionsEvaluator evaluator = fullSource ? new FullSourceEvaluator() : new ExpressionsEvaluator();
            var function = evaluator.Evaluate(".5²+z²", "", CalculationsMode.Complex);
            var value = function.Evaluate(new Complex(3, 4));
            NUnit.Framework.Assert.That((value - new Complex(-6.75, 24)).Magnitude, Is.LessThan(1e-12));
        }
    }
}
