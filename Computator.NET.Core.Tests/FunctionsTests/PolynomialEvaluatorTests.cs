using System;
using System.Text;
using System.Numerics;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    [NonParallelizable]
    public class PolynomialEvaluatorTests
    {
        private readonly bool _legacy;
        public PolynomialEvaluatorTests(bool legacy) { _legacy = legacy; }

        private sealed class LegacyExpressionsEvaluator : ExpressionsEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        private sealed class LegacyScriptEvaluator : ScriptEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        [TestCase("Polynomial.Evaluate(x,1,-3,2)", 3.0)]
        [TestCase("new Polynomial(1,-3,2).Differentiate().Evaluate(x)", 5.0)]
        public void RealExpressionsUseTheSharedPolynomialAlias(string expression, double expected)
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyExpressionsEvaluator() : new ExpressionsEvaluator();
            Func<double, double> function = evaluator.Evaluate(expression, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(2), Is.EqualTo(expected).Within(1e-12));
        }

        [Test]
        public void ComplexExpressionsUseRealPolynomialCoefficients()
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyExpressionsEvaluator() : new ExpressionsEvaluator();
            Func<Complex, Complex> function = evaluator.Evaluate(
                "new Polynomial(1,-3,2).Evaluate(z)", "", CalculationsMode.Complex);
            NUnit.Framework.Assert.That(function(new Complex(2, 3)), Is.EqualTo(new Complex(-15, 15)));
        }

        [Test]
        public void ScriptsCompilePolynomialConstructionDivisionAndGeneralRoots()
        {
            var previousDirectory = Environment.CurrentDirectory;
            try
            {
                ScriptEvaluator evaluator = _legacy ? new LegacyScriptEvaluator() : new ScriptEvaluator();
                var output = new StringBuilder();
                evaluator.Evaluate(@"
                    var parts = Polynomial.DivideRemainder(new Polynomial(1,0,1),new Polynomial(1,1));
                    write(parts.Item1.Evaluate(3)); write("";"");
                    write(parts.Item2.Evaluate(3)); write("";"");
                    write(new Polynomial(-6,11,-6,1).Roots().Length);
                ", "").Evaluate(text => output.Append(text));
                NUnit.Framework.Assert.That(output.ToString(), Is.EqualTo("2;2;3"));
            }
            finally { Environment.CurrentDirectory = previousDirectory; }
        }
    }
}
