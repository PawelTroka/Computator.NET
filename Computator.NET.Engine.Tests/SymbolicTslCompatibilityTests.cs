using System.Globalization;
using System.Numerics;
using Computator.NET.Engine.Symbolic;
using NUnit.Framework;
using Check = NUnit.Framework.Assert;

namespace Computator.NET.Engine.Tests;

/// <summary>
/// Public-contract checks using the independent numerical TSL compiler as a
/// consumer of symbolic answers. Expected values come from arithmetic/BCL math,
/// not from the symbolic normalizer or formatter under test.
/// </summary>
[TestFixture]
public sealed class SymbolicTslCompatibilityTests
{
    private readonly SymbolicEngine symbolic = new();
    private readonly PortableEngine numerical = new();

    private static IEnumerable<TestCaseData> ScalarResults()
    {
        yield return new("simplify", "2·x³+3·x²", 2d, 0d, 28d);
        yield return new("expand", "(x+y)³", 2d, 3d, 125d);
        yield return new("factor", "x²-1", 3d, 0d, 8d);
        yield return new("differentiate", "sin(x²)", .5, 0d, Math.Cos(.25));
        yield return new("differentiate", "3·x³+sin(x²)", 2d, 0d, 36 + 4 * Math.Cos(4));
        yield return new("differentiate", "sin(PI·x)", 0d, 0d, Math.PI);
        yield return new("determinant", "[[x²,0],[0,3·x]]", 2d, 0d, 24d);
        yield return new("trace", "[[x²,0],[0,2·x]]", 3d, 0d, 15d);
        yield return new("simplify", "-x²", 3d, 0d, -9d);
        yield return new("simplify", "(-x)²", 3d, 0d, 9d);
        yield return new("simplify", "(-x)³", 2d, 0d, -8d);
        yield return new("simplify", "x⁻²", 2d, 0d, .25);
        yield return new("simplify", "(x/2)⁻²", 4d, 0d, .25);
        yield return new("simplify", "(x/y)²", 6d, 2d, 9d);
        yield return new("simplify", "x/(y+1)", 6d, 2d, 2d);
        yield return new("simplify", "x-(y-1)", 6d, 2d, 5d);
        yield return new("simplify", "x⁽ʸ⁺¹⁾", 2d, 3d, 16d);
        yield return new("simplify", "x¹˸²", 4d, 0d, 2d);
        yield return new("simplify", "x⁰ᱸ⁵", 4d, 0d, 2d);
        yield return new("simplify", "x  ²   ˸  ʸ ˙  ²", 4d, 2d, 16d);
        yield return new("simplify", "x²y³", 2d, 3d, 108d);
        yield return new("simplify", "cos⁽ʸ⁺¹⁾(x)", .25, 2d, Math.Pow(Math.Cos(.25), 3));
        yield return new("simplify", "cosʸ(x)²", .25, 3d, Math.Pow(Math.Cos(.25), 6));
        yield return new("simplify", "sin³(PI/6)²", 0d, 0d, 1d / 64);
        yield return new("simplify", "2cosʸ(x)", .25, 2d, 2 * Math.Pow(Math.Cos(.25), 2));
        yield return new("simplify", "sin²(x)+cos²(x)", 2.3, 0d, 1d);
        yield return new("simplify", "2ˡᵒᵍ⁽⁸ʹ²⁾", 0d, 0d, 8d);
        yield return new("simplify", "2Ꞌ⁻³Ꞌ", 0d, 0d, 8d);
        yield return new("simplify", "log(x,2)", 8d, 0d, 3d);
        yield return new("simplify", "log(2,x)", 8d, 0d, 1d / 3);
        yield return new("simplify", "log(log(x,2),2)", 256d, 0d, 3d);
        yield return new("simplify", "log²(x,2)", 8d, 0d, 9d);
        yield return new("simplify", "PI·x+e", 2d, 0d, 2 * Math.PI + Math.E);
        yield return new("simplify", "(x+1)/3", 2d, 0d, 1d);
    }

    [TestCaseSource(nameof(ScalarResults))]
    public void ReturnedScalarTslRunsInBothPublicCalculationPaths(
        string operation, string expression, double x, double y, double expected)
    {
        var result = symbolic.Execute(new SymbolicRequest { Operation = operation, Expression = expression });
        Check.That(result.Status, Is.EqualTo("exact"), expression);
        var tolerance = 2e-12 * (1 + Math.Abs(expected));
        Check.That(numerical.EvaluateReal(result.Result, x, y), Is.EqualTo(expected).Within(tolerance), result.Result);

        var reused = symbolic.Execute(new SymbolicRequest
        {
            Operation = "evaluate", Expression = result.Result, Precision = 30,
            Substitutions = new Dictionary<string, string>
            {
                ["x"] = x.ToString("R", CultureInfo.InvariantCulture),
                ["y"] = y.ToString("R", CultureInfo.InvariantCulture)
            }
        });
        Check.That(reused.Status, Is.EqualTo("approximate"));
        Check.That(double.Parse(reused.Result, CultureInfo.InvariantCulture),
            Is.EqualTo(expected).Within(tolerance), result.Result);
    }

    [Test]
    public void PublicDerivativeUsesActualSuperscriptsAndMiddleDots()
    {
        var result = symbolic.Execute(new SymbolicRequest { Operation = "differentiate", Expression = "2·x³" });
        Check.That(result.Result.Replace(" ", ""), Is.EqualTo("6·x²"));
        Check.That(result.Result, Does.Not.Contain("*").And.Not.Contain("^"));
        Check.That(result.Latex, Is.Not.Empty);
        Check.That(numerical.EvaluateReal(result.Result, 3), Is.EqualTo(54));
    }

    [Test]
    public void AnUnmappableExponentRetainsItsVariableInExecutablePowNotation()
    {
        var result = symbolic.Execute(new SymbolicRequest { Operation = "simplify", Expression = "pow(x,q)" });
        Check.That(result.Result, Does.Contain("pow(").And.Contain("q").And.Not.Contain("ⱽ"));
        var rebound = symbolic.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = result.Result,
            Substitutions = new Dictionary<string, string> { ["q"] = "3" }
        });
        Check.That(numerical.EvaluateReal(rebound.Result, 2), Is.EqualTo(8));
    }

    [Test]
    public void AnAntiderivativeRemainsReusableAfterAssigningItsConstant()
    {
        var integral = symbolic.Execute(new SymbolicRequest { Operation = "integrate", Expression = "2·x" });
        var rebound = symbolic.Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = integral.Result,
            Substitutions = new Dictionary<string, string> { ["C"] = "7" }
        });
        Check.That(numerical.EvaluateReal(rebound.Result, 3), Is.EqualTo(16));
        var derivative = symbolic.Execute(new SymbolicRequest { Operation = "differentiate", Expression = integral.Result });
        Check.That(numerical.EvaluateReal(derivative.Result, 3), Is.EqualTo(6));
    }

    [Test]
    public void TaylorOutputPreservesFractionalCoefficientsInNumericalTsl()
    {
        var polynomial = symbolic.Execute(new SymbolicRequest { Operation = "taylor", Expression = "exp(x)", Order = 4 });
        foreach (var x in new[] { -1d, -.25, 0, .5, 2 })
        {
            var expected = 1 + x + x * x / 2 + x * x * x / 6 + Math.Pow(x, 4) / 24;
            Check.That(numerical.EvaluateReal(polynomial.Result, x), Is.EqualTo(expected).Within(1e-12), polynomial.Result);
        }
    }

    [TestCase(8d, 2d, 3d)]
    [TestCase(2d, 8d, 1d / 3)]
    [TestCase(100d, 10d, 2d)]
    public void PortableLogOverloadKeepsTheEstablishedDesktopArgumentOrder(double value, double nbase, double expected)
    {
        Check.That(numerical.EvaluateReal("log(x,y)", value, nbase), Is.EqualTo(expected).Within(1e-12));
        Check.That(ScalarFunctions.log(value, nbase), Is.EqualTo(expected).Within(1e-12));
    }

    [TestCase(8d, 0d, 2d)]
    [TestCase(-2d, 3d, 10d)]
    public void ComplexLogOverloadPreservesTheDesktopBranchAndBase(double real, double imaginary, double nbase)
    {
        var value = new Complex(real, imaginary);
        var expected = Complex.Log(value, nbase);
        var expression = "log(z," + nbase.ToString("R", CultureInfo.InvariantCulture) + ")";
        Check.That((numerical.EvaluateComplex(expression, value) - expected).Magnitude, Is.LessThan(1e-12));
    }
}
