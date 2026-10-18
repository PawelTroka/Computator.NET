using System;
using System.Numerics;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using NUnit.Framework;
using CalculationsMode = Computator.NET.DataTypes.CalculationsMode;

namespace Computator.NET.Core.Tests.EvaluationTests;

[TestFixture]
public class SymbolicFunctionPowerTests
{
    [TestCase("cosᶻ(z)")]
    [TestCase("cos⁽ᶻ⁾(z)")]
    [TestCase("cos(z)ᶻ")]
    public void DesktopComplexEvaluationUsesPrincipalPower(string source)
    {
        var z = new Complex(0.7, -0.4);
        var actual = new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Complex).Evaluate(z);
        NUnit.Framework.Assert.That(Complex.Abs(actual - Complex.Pow(Complex.Cos(z), z)), Is.LessThan(1e-12));
    }

    [TestCase("sqrtˣ(4)", 8)]
    [TestCase("System.Math.Sqrt⁽ˣ⁾(4)", 8)]
    [TestCase("sqrt⁽ˣ⁺¹⁾(4)", 16)]
    [TestCase("sqrt⁻²(4)", 0.25)]
    [TestCase("sqrt(sqrtˣ(16))²", 64)]
    public void DesktopRealEvaluationUsesSharedPowerResolution(string source, double expected) =>
        NUnit.Framework.Assert.That(new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real).Evaluate(3.0),
            Is.EqualTo(expected).Within(1e-12));

    [TestCase("ψⁿ(1,1)", 1)]
    [TestCase("ψⁿ⁽ˣ⁾(1,1)", 3)]
    public void TheActualBuiltInSuperscriptNameIsPreserved(string source, int power) =>
        NUnit.Framework.Assert.That(new ExpressionsEvaluator().Evaluate(source, "", CalculationsMode.Real).Evaluate(3.0),
            Is.EqualTo(Math.Pow(Math.PI * Math.PI / 6, power)).Within(1e-12));

    [Test]
    public void ExistingCustomNameWinsOverPowerShorthand() =>
        NUnit.Framework.Assert.That(new ExpressionsEvaluator().Evaluate("cosˣ(4)",
            "static real cosˣ(real x) { return 7; }",
            CalculationsMode.Real).Evaluate(3.0), Is.EqualTo(7));

    [TestCase("cosᶻ(z)", CalculationsMode.Complex)]
    [TestCase("cosᶻ(0)", CalculationsMode.Complex)]
    [TestCase("cos⁽ᶻ⁺¹⁾(0)", CalculationsMode.Complex)]
    [TestCase("sqrtʸ(4)", CalculationsMode.Fxy)]
    [TestCase("ψⁿ(x)", CalculationsMode.Real)]
    public void DesktopModeDetectionRetainsExponentInputs(string source, CalculationsMode expected) =>
        NUnit.Framework.Assert.That(new ModeDeterminer(new TslCompiler()).DetermineMode(source), Is.EqualTo(expected));
}
