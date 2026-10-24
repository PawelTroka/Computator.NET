using System.Numerics;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture]
    public class AccessPowerEvaluationTests
    {
        private sealed class FullSourceEvaluator : ExpressionsEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        [TestCase(false, "System.Math.PI²", 9.869604401089358)]
        [TestCase(true, "System.Math.PI²", 9.869604401089358)]
        [TestCase(false, "new[]{3.0,4.0}[0]²", 9)]
        [TestCase(true, "new[]{3.0,4.0}[0]²", 9)]
        [TestCase(false, "-new complex(3,4).Magnitude²", -25)]
        [TestCase(true, "-new complex(3,4).Magnitude²", -25)]
        public void DesktopEvaluationUsesTheCompleteOperand(bool fullSource, string source, double expected)
        {
            ExpressionsEvaluator evaluator = fullSource ? new FullSourceEvaluator() : new ExpressionsEvaluator();
            var function = evaluator.Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function.Evaluate(0.0), Is.EqualTo(expected).Within(1e-12));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ComplexMembersRetainAutomaticRoutingAndEvaluateInBothCompilationPaths(bool fullSource)
        {
            const string source = "z.Real²+z.Imaginary²";
            var mode = new ModeDeterminer(new TslCompiler()).DetermineMode(source);
            NUnit.Framework.Assert.That(mode, Is.EqualTo(CalculationsMode.Complex));
            ExpressionsEvaluator evaluator = fullSource ? new FullSourceEvaluator() : new ExpressionsEvaluator();
            var value = evaluator.Evaluate(source, "", mode).Evaluate(new Complex(3, 4));
            NUnit.Framework.Assert.That((value - new Complex(25, 0)).Magnitude, Is.LessThan(1e-12));
        }
    }
}
