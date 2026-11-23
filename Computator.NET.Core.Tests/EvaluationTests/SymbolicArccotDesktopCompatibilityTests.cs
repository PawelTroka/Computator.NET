using System;
using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using Computator.NET.Engine.Symbolic;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture]
    public class SymbolicArccotDesktopCompatibilityTests
    {
        [TestCase("arccot(x)", -2d)]
        [TestCase("arccot(x)", -1d)]
        [TestCase("arccot(x)", 0d)]
        [TestCase("arccot(x)", 2d)]
        [TestCase("arcctg(x)", -1d)]
        [TestCase("arcctg²(x)", -1d)]
        public void OriginalAndReturnedExpressionsUseTheSameNativeRealBranch(string expression, double x)
        {
            var evaluator = new ExpressionsEvaluator();
            var symbolic = new SymbolicEngine().Execute(new SymbolicRequest { Expression = expression });
            var original = evaluator.Evaluate(expression, "", CalculationsMode.Real).Evaluate(x);
            var reused = evaluator.Evaluate(symbolic.Result, "", CalculationsMode.Real).Evaluate(x);
            var angle = Math.Atan2(1, x);
            var expected = expression.Contains("²") ? angle * angle : angle;
            NUnit.Framework.Assert.That(original, Is.EqualTo(expected).Within(1e-12));
            NUnit.Framework.Assert.That(reused, Is.EqualTo(expected).Within(1e-12), symbolic.Result);
        }

        [TestCase(-1d, 1d)]
        [TestCase(-1d, -1d)]
        [TestCase(1d, 1d)]
        [TestCase(0d, 2d)]
        [TestCase(0d, -2d)]
        public void OriginalAndReturnedExpressionsUseTheSameNativeComplexBranch(double real, double imaginary)
        {
            var z = new Complex(real, imaginary);
            var expected = Math.PI / 2 - Complex.Atan(z);
            var evaluator = new ExpressionsEvaluator();
            foreach (var expression in new[] { "arccot(z)", "arcctg(z)" })
            {
                var symbolic = new SymbolicEngine().Execute(new SymbolicRequest { Expression = expression });
                var original = evaluator.Evaluate(expression, "", CalculationsMode.Complex).Evaluate(z);
                var reused = evaluator.Evaluate(symbolic.Result, "", CalculationsMode.Complex).Evaluate(z);
                NUnit.Framework.Assert.That((original - expected).Magnitude, Is.LessThan(1e-12));
                NUnit.Framework.Assert.That((reused - expected).Magnitude, Is.LessThan(1e-12), symbolic.Result);
            }
        }
    }
}
