using System;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    public class DiscontinuousFunctionsNaNTests
    {
        [Test]
        public void HostFunctionsAndAliasesDoNotTurnNaNIntoAnOrdinaryResult()
        {
            NUnit.Framework.Assert.That(ElementaryFunctions.Heaviside(double.NaN), Is.NaN);
            NUnit.Framework.Assert.That(ElementaryFunctions.H(double.NaN), Is.NaN);
            NUnit.Framework.Assert.That(ElementaryFunctions.DiracDelta(double.NaN), Is.NaN);
            NUnit.Framework.Assert.That(ElementaryFunctions.δ(double.NaN), Is.NaN);

            NUnit.Framework.Assert.That(ElementaryFunctions.KroneckerDelta(double.NaN, 2), Is.NaN);
            NUnit.Framework.Assert.That(ElementaryFunctions.KroneckerDelta(2, double.NaN), Is.NaN);
            NUnit.Framework.Assert.That(ElementaryFunctions.KroneckerDelta(double.NaN, double.NaN), Is.NaN);
            NUnit.Framework.Assert.That(ElementaryFunctions.δij(double.NaN, 2), Is.NaN);
        }

        [Test]
        public void ExistingFiniteAndInfiniteInputConventionsRemainUnchanged()
        {
            NUnit.Framework.Assert.That(ElementaryFunctions.Heaviside(-1), Is.EqualTo(0));
            NUnit.Framework.Assert.That(ElementaryFunctions.Heaviside(double.NegativeInfinity), Is.EqualTo(0));
            NUnit.Framework.Assert.That(ElementaryFunctions.Heaviside(-0.0), Is.EqualTo(0.5));
            NUnit.Framework.Assert.That(ElementaryFunctions.Heaviside(0.0), Is.EqualTo(0.5));
            NUnit.Framework.Assert.That(ElementaryFunctions.Heaviside(1), Is.EqualTo(1));
            NUnit.Framework.Assert.That(ElementaryFunctions.Heaviside(double.PositiveInfinity), Is.EqualTo(1));

            NUnit.Framework.Assert.That(ElementaryFunctions.DiracDelta(0), Is.EqualTo(double.PositiveInfinity));
            NUnit.Framework.Assert.That(ElementaryFunctions.DiracDelta(1), Is.EqualTo(0));
            NUnit.Framework.Assert.That(ElementaryFunctions.DiracDelta(double.PositiveInfinity), Is.EqualTo(0));
            NUnit.Framework.Assert.That(ElementaryFunctions.KroneckerDelta(2, 2), Is.EqualTo(1));
            NUnit.Framework.Assert.That(ElementaryFunctions.KroneckerDelta(2, 3), Is.EqualTo(0));
        }

        [TestCase("Heaviside(x)")]
        [TestCase("H(x)")]
        [TestCase("DiracDelta(x)")]
        [TestCase("δ(x)")]
        [TestCase("KroneckerDelta(x, 2)")]
        [TestCase("KroneckerDelta(2, x)")]
        [TestCase("δij(x, 2)")]
        public void GeneratedExpressionFunctionsPropagateNaN(string expression)
        {
            // ExpressionsEvaluator uses the separately emitted ElementaryFunctions.ToCode.
            var compiled = new ExpressionsEvaluator().Evaluate(expression, string.Empty, CalculationsMode.Real);
            NUnit.Framework.Assert.That(compiled(double.NaN), Is.NaN, expression);
        }

        [TestCase("Heaviside(x)", -1.0, 0.0)]
        [TestCase("Heaviside(x)", 0.0, 0.5)]
        [TestCase("Heaviside(x)", 1.0, 1.0)]
        [TestCase("DiracDelta(x)", 1.0, 0.0)]
        [TestCase("KroneckerDelta(x, 2)", 2.0, 1.0)]
        [TestCase("KroneckerDelta(x, 2)", 3.0, 0.0)]
        public void GeneratedExpressionFunctionsRetainNormalResults(string expression, double input, double expected)
        {
            var compiled = new ExpressionsEvaluator().Evaluate(expression, string.Empty, CalculationsMode.Real);
            NUnit.Framework.Assert.That(compiled(input), Is.EqualTo(expected));
        }
    }
}
