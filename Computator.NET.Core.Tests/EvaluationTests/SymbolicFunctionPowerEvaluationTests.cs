using System;
using System.Linq;
using System.Numerics;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.EvaluationTests
{
    [TestFixture]
    public class SymbolicFunctionPowerEvaluationTests
    {
        private const double EulerGamma = 0.5772156649015328606;
        private ExpressionsEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new ExpressionsEvaluator();
        }

        [TestCase("cosʸ(x)", 0.75, 1.25)]
        [TestCase("cosʸ(x)", 0.3, -2.5)]
        [TestCase("System.Math.Cosʸ(x)", 0.75, 1.25)]
        public void RealFunctionPowerUsesTheIndependentSecondCoordinate(string expression, double x, double y)
        {
            var function = _evaluator.Evaluate(expression, string.Empty, CalculationsMode.Fxy);

            NUnit.Framework.Assert.That(function.Evaluate(x, y),
                Is.EqualTo(Math.Pow(Math.Cos(x), y)).Within(1e-12));
        }

        [TestCase("cosᶻ(z)")]
        [TestCase("System.Numerics.Complex.Cosᶻ(z)")]
        public void ComplexFunctionPowerRaisesTheResultToTheComplexArgument(string expression)
        {
            var z = new Complex(0.7, 0.4);
            var function = _evaluator.Evaluate(expression, string.Empty, CalculationsMode.Complex);

            AssertComplexEqual(Complex.Pow(Complex.Cos(z), z), function.Evaluate(z));
        }

        [Test]
        public void GroupedSymbolicExponentIncludesTheEntireExpression()
        {
            var z = new Complex(0.7, 0.4);
            var function = _evaluator.Evaluate("cos⁽ᶻ⁺¹⁾(z)", string.Empty, CalculationsMode.Complex);

            AssertComplexEqual(Complex.Pow(Complex.Cos(z), z + 1), function.Evaluate(z));
        }

        [Test]
        public void SignedSymbolicExponentMeansAReciprocalPower()
        {
            const double x = 0.75;
            const double y = 1.25;
            var function = _evaluator.Evaluate("cos⁻ʸ(x)", string.Empty, CalculationsMode.Fxy);

            NUnit.Framework.Assert.That(function.Evaluate(x, y),
                Is.EqualTo(Math.Pow(Math.Cos(x), -y)).Within(1e-12));
        }

        [Test]
        public void NestedSymbolicPowersBindBothFunctionCalls()
        {
            var z = new Complex(0.7, 0.4);
            var function = _evaluator.Evaluate("cosᶻ(sinᶻ(z))", string.Empty, CalculationsMode.Complex);
            var inner = Complex.Pow(Complex.Sin(z), z);

            AssertComplexEqual(Complex.Pow(Complex.Cos(inner), z), function.Evaluate(z));
        }

        [Test]
        public void NestedPowersKeepTheDesktopSquareRootsDynamicResult()
        {
            const double x = 4;
            const double y = 3;
            var function = _evaluator.Evaluate("cosʸ(sqrtʸ(x))", string.Empty, CalculationsMode.Fxy);

            NUnit.Framework.Assert.That(function.Evaluate(x, y),
                Is.EqualTo(Math.Pow(Math.Cos(Math.Pow(Math.Sqrt(x), y)), y)).Within(1e-12));
        }

        [Test]
        public void ARealSquareRootCanReturnComplexBeforeItsSymbolicPower()
        {
            var z = new Complex(0.7, 0.4);
            var function = _evaluator.Evaluate("sqrtᶻ(-1)", string.Empty, CalculationsMode.Complex);

            AssertComplexEqual(Complex.Pow(Complex.Sqrt(-1), z), function.Evaluate(z));
        }

        [Test]
        public void UnicodeCustomFunctionCanHaveASymbolicPower()
        {
            var z = new Complex(0.7, 0.4);
            var function = _evaluator.Evaluate("λᶻ(z)",
                "static complex λ(complex value) { return value + 2; }", CalculationsMode.Complex);

            AssertComplexEqual(Complex.Pow(z + 2, z), function.Evaluate(z));
        }

        [TestCase("ψⁿ(x)", -EulerGamma)]
        [TestCase("ψⁿ(1,x)", Math.PI * Math.PI / 6)]
        public void ExistingSuperscriptFunctionKeepsItsOneAndTwoArgumentOverloads(string expression, double expected)
        {
            var function = _evaluator.Evaluate(expression, string.Empty, CalculationsMode.Real);

            // Psi(1) = -EulerGamma and Psi'(1) = pi squared / 6, independently
            // of the implementation used by the generated desktop function.
            NUnit.Framework.Assert.That(function.Evaluate(1.0), Is.EqualTo(expected).Within(1e-12));
        }

        [Test]
        public void ExistingSuperscriptFunctionCanStillHaveAPostfixSymbolicPower()
        {
            var z = new Complex(2, 0.5);
            var function = _evaluator.Evaluate("ψⁿ(x)ᶻ", string.Empty, CalculationsMode.Complex);

            AssertComplexEqual(Complex.Pow(1 - EulerGamma, z), function.Evaluate(z));
        }

        [Test]
        public void ExistingSuperscriptFunctionCanStillHaveAPrefixNumericPower()
        {
            var function = _evaluator.Evaluate("ψⁿ²(x)", string.Empty, CalculationsMode.Real);

            NUnit.Framework.Assert.That(function.Evaluate(1.0),
                Is.EqualTo(EulerGamma * EulerGamma).Within(1e-12));
        }

        [TestCase("cosᶻ(z)")]
        [TestCase("@cosᶻ(z)")]
        public void ExactCustomNameWinsOverTheFunctionPowerInterpretation(string expression)
        {
            var z = new Complex(0.7, 0.4);
            var function = _evaluator.Evaluate(expression,
                "static complex cosᶻ(complex value) { return value + 7; }", CalculationsMode.Complex);

            AssertComplexEqual(z + 7, function.Evaluate(z));
        }

        [Test]
        public void ExactCustomNameWithTheWrongOverloadProducesItsOwnCompilationError()
        {
            var exception = NUnit.Framework.Assert.Throws<CompilationException>(() =>
                _evaluator.Evaluate("cosᶻ(z)",
                    "static complex cosᶻ(real value) { return value + 7; }", CalculationsMode.Complex));

            NUnit.Framework.Assert.That(exception.Errors.Values.SelectMany(errors => errors)
                .Select(error => error.ErrorNumber), Does.Contain("CS1503"));
        }

        [Test]
        public void EscapedMissingNameIsNotReinterpretedAsAFunctionPower()
        {
            NUnit.Framework.Assert.Throws<CompilationException>(() =>
                _evaluator.Evaluate("@cosᶻ(z)", string.Empty, CalculationsMode.Complex));
        }

        [Test]
        public void DynamicArgumentsDoNotMakeAnOutOfScopeExponentValid()
        {
            const string customFunctions =
                "static complex Probe(complex value) { dynamic argument = value; return cosᶻ(argument); }";

            // The caller's z coordinate is not in scope inside this method.
            NUnit.Framework.Assert.Throws<CompilationException>(() =>
                _evaluator.Evaluate("Probe(z)", customFunctions, CalculationsMode.Complex));
        }

        [Test]
        public void MultipleValidNameAndExponentSplitsRemainAmbiguous()
        {
            const string customFunctions =
                "static complex cosᶻ(complex value) { return value + 1; } " +
                "static complex Probe(complex value) { complex y = value; complex zy = value; return cosᶻʸ(value); }";

            // Both cos(value) raised to zy and cosᶻ(value) raised to y would bind.
            NUnit.Framework.Assert.Throws<CompilationException>(() =>
                _evaluator.Evaluate("Probe(z)", customFunctions, CalculationsMode.Complex));
        }

        [Test]
        public void UngroupedMixedExponentDoesNotReinterpretTheNumericPowersBaseName()
        {
            // Numeric prefix notation identifies Probeᶻ as the exact custom name.
            // With no such function, Probe⁽ᶻ⁺¹⁾(z) expresses the compound power.
            NUnit.Framework.Assert.Throws<CompilationException>(() =>
                _evaluator.Evaluate("Probeᶻ⁺¹(z)", "static complex Probe(complex value) { return value+1; }", CalculationsMode.Complex));
        }

        [Test]
        public void NumericPrefixPowerOfAnExactCustomNameKeepsThatName()
        {
            var z = new Complex(0.7, 0.4);
            var function = _evaluator.Evaluate("cosᶻ²(z)",
                "static complex cosᶻ(complex value) { return value + 7; }", CalculationsMode.Complex);

            AssertComplexEqual(Complex.Pow(z + 7, 2), function.Evaluate(z));
        }

        [Test]
        public void RawCaretRetainsItsExclusiveOrSemantics()
        {
            var function = _evaluator.Evaluate("7^3", string.Empty, CalculationsMode.Real);

            NUnit.Framework.Assert.That(function.Evaluate(0.0), Is.EqualTo(4.0));
        }

        private static void AssertComplexEqual(Complex expected, Complex actual)
        {
            NUnit.Framework.Assert.That(actual.Real, Is.EqualTo(expected.Real).Within(1e-12));
            NUnit.Framework.Assert.That(actual.Imaginary, Is.EqualTo(expected.Imaginary).Within(1e-12));
        }
    }
}
