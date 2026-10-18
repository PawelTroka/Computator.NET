using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture]
    public class OrientedIntegralExpressionTests
    {
        [TestCase("rectangleMethod")]
        [TestCase("trapezoidalMethod")]
        [TestCase("simpsonMethod")]
        public void DesktopExpressionUsesCorrectGeneratedQuadrature(string method)
        {
            var expression = new ExpressionsEvaluator().Evaluate(
                $"Integral.{method}((real t) => 2*t+1, 3, -1, 4)", string.Empty, CalculationsMode.Real);

            NUnit.Framework.Assert.That(expression.Evaluate(0.0), Is.EqualTo(-12).Within(1e-12));
        }
    }
}
