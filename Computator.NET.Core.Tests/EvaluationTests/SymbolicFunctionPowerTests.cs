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

    [TestCase("System.Math.Cosʸ(x)")]
    [TestCase("System.Math.Cos⁽ʸ⁾(x)")]
    public void AutomaticallyRoutedQualifiedPowerUsesBothCoordinates(string source)
    {
        var mode = new ModeDeterminer(new TslCompiler()).DetermineMode(source);
        NUnit.Framework.Assert.That(mode, Is.EqualTo(CalculationsMode.Fxy));
        var function = new ExpressionsEvaluator().Evaluate(source, "", mode);
        NUnit.Framework.Assert.That(function.Evaluate(0.75, 1.25),
            Is.EqualTo(Math.Pow(Math.Cos(0.75), 1.25)).Within(1e-12));
    }

    [TestCase("System.Numerics.Complex.Cosᶻ(0)")]
    [TestCase("System.Numerics.Complex.Cos⁽ᶻ⁾(0)")]
    public void AutomaticallyRoutedQualifiedPowerUsesItsExponentOnlyComplexCoordinate(string source)
    {
        var mode = new ModeDeterminer(new TslCompiler()).DetermineMode(source);
        NUnit.Framework.Assert.That(mode, Is.EqualTo(CalculationsMode.Complex));
        var z = new Complex(0.7, -0.4);
        var function = new ExpressionsEvaluator().Evaluate(source, "", mode);
        NUnit.Framework.Assert.That(Complex.Abs(function.Evaluate(z) - Complex.Pow(Complex.Cos(0), z)),
            Is.LessThan(1e-12));
    }

    [TestCase("λʸ(x)", "static real λ(real value) { return value + 2; }")]
    [TestCase("Helpers.λʸ(x)", "static class Helpers { public static real λ(real value) { return value + 2; } }")]
    public void AutomaticRoutingBindsCustomFunctionsBeforeFindingExponentCoordinates(string source, string customFunctions)
    {
        var mode = new ModeDeterminer(new TslCompiler()).DetermineMode(source, customFunctions);
        NUnit.Framework.Assert.That(mode, Is.EqualTo(CalculationsMode.Fxy));
        var function = new ExpressionsEvaluator().Evaluate(source, customFunctions, mode);
        NUnit.Framework.Assert.That(function.Evaluate(1.0, 3.0), Is.EqualTo(27.0));
    }

    [TestCase("cosᶻ(0)", "static real cosᶻ(real value) { return value + 7; }")]
    [TestCase("Helpers.cosᶻ(0)", "static class Helpers { public static real cosᶻ(real value) { return value + 7; } }")]
    [TestCase("@cosᶻ(0)", "static real cosᶻ(real value) { return value + 7; }")]
    public void ExactCustomNamesDoNotInventCoordinatesDuringAutomaticRouting(string source, string customFunctions)
    {
        var mode = new ModeDeterminer(new TslCompiler()).DetermineMode(source, customFunctions);
        NUnit.Framework.Assert.That(mode, Is.EqualTo(CalculationsMode.Real));
        NUnit.Framework.Assert.That(new ExpressionsEvaluator().Evaluate(source, customFunctions, mode).Evaluate(1.0),
            Is.EqualTo(7.0));
    }

    [Test]
    public void RoutingDoesNotExecuteCustomInitializersOrFunctions()
    {
        const string customFunctions = "static class Helpers { " +
            "static Helpers() { throw new System.InvalidOperationException(); } " +
            "public static real λ(real value) { throw new System.InvalidOperationException(); } }";
        NUnit.Framework.Assert.That(new ModeDeterminer(new TslCompiler()).DetermineMode("Helpers.λʸ(x)", customFunctions),
            Is.EqualTo(CalculationsMode.Fxy));
    }

    [Test]
    public void AutomaticRoutingPreservesExactDynamicReceiverNames()
    {
        const string customFunctions = "class Helpers { public real Probeʸ(real value) { return value + 7; } } " +
            "static dynamic receiver = new Helpers();";
        const string source = "receiver.Probeʸ(0)";
        var mode = new ModeDeterminer(new TslCompiler()).DetermineMode(source, customFunctions);
        NUnit.Framework.Assert.That(mode, Is.EqualTo(CalculationsMode.Real));
        NUnit.Framework.Assert.That(new ExpressionsEvaluator().Evaluate(source, customFunctions, mode).Evaluate(1.0),
            Is.EqualTo(7.0));
    }

    [Test]
    public void ImplicitPowerRoutingKeepsTheThirdCoordinateReal()
    {
        const string source = "x+y+System.Math.Cosᶻ(0)=5";
        var mode = new ModeDeterminer(new TslCompiler()).DetermineMode(source);
        NUnit.Framework.Assert.That(mode, Is.EqualTo(CalculationsMode.Fxy));
        NUnit.Framework.Assert.That(new ExpressionsEvaluator().Evaluate(source, "", mode).Evaluate(1.0, 3.0, 2.0),
            Is.EqualTo(0.0));
    }

    [TestCase("System.Math.Cosʸ(")]
    [TestCase("System.Math.Cosʸ(x) /*")]
    [TestCase("x = y = System.Math.Cosʸ(x)")]
    public void IncompleteInputStillAllowsModeAnalysis(string source) =>
        NUnit.Framework.Assert.DoesNotThrow(() => new ModeDeterminer(new TslCompiler()).DetermineMode(source));
}
