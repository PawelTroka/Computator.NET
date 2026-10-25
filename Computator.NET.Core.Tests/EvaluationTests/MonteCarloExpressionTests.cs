using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture]
    public class MonteCarloExpressionTests
    {
        [TestCase("Integral.monteCarloMethod(t => 2.0, 0, 1, 1024)", 2.0)]
        [TestCase("Integral.monteCarloMethod(t => -3.0, -2, 5, 7)", -21.0)]
        [TestCase("Integral.monteCarloMethod(t => -3.0, 5, -2, 7)", 21.0)]
        public void DesktopExpressionUsesCorrectedMonteCarlo(string source, double expected)
        {
            var expression = new ExpressionsEvaluator().Evaluate(source, string.Empty, CalculationsMode.Real);

            NUnit.Framework.Assert.That(expression.Evaluate(0.0), Is.EqualTo(expected));
        }
    }
}
