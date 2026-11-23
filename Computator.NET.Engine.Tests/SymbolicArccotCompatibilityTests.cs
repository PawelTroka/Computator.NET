using System.Globalization;
using System.Numerics;
using AngouriMath;
using Computator.NET.Engine.Symbolic;
using NUnit.Framework;
using Check = NUnit.Framework.Assert;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public sealed class SymbolicArccotCompatibilityTests
{
    private readonly SymbolicEngine engine = new();

    [Test]
    public void NativeRealBranchesAndTheirReturnedExpressionsAgreeWithTsl(
        [Values("arccot", "arcctg")] string name,
        [Values("real", "complex")] string domain)
    {
        var symbolic = engine.Execute(new SymbolicRequest { Expression = name + "(x)", Domain = domain });
        Check.That(symbolic.Status, Is.EqualTo("exact"));
        foreach (var x in new[] { -2d, -1d, 0d, 1d, 2d })
        {
            // This is the existing native TSL real branch, independently of CAS
            // normalization, simplification and formatting.
            var expected = new Complex(Math.Atan2(1, x), 0);
            AssertValue(name + "(x)", x.ToString("R", CultureInfo.InvariantCulture), expected, domain);
            AssertValue(symbolic.Result, x.ToString("R", CultureInfo.InvariantCulture), expected, domain);
        }
    }

    [TestCase(-1d, 1d)]
    [TestCase(-1d, -1d)]
    [TestCase(1d, 1d)]
    [TestCase(1d, -1d)]
    [TestCase(0d, .5)]
    [TestCase(0d, -.5)]
    [TestCase(0d, 2d)]
    [TestCase(0d, -2d)]
    public void NativeComplexBranchMatchesTheDesktopDefinition(double real, double imaginary)
    {
        var value = new Complex(real, imaginary);
        var expected = Math.PI / 2 - Complex.Atan(value);
        var replacement = ComplexText(real, imaginary);
        foreach (var name in new[] { "arccot", "arcctg" })
        {
            var symbolic = engine.Execute(new SymbolicRequest { Expression = name + "(x)" });
            AssertValue(name + "(x)", replacement, expected);
            AssertValue(symbolic.Result, replacement, expected);
        }
    }

    [Test]
    public void NegativeRealConstantIsThreeQuartersOfPiInBothDomains()
    {
        foreach (var domain in new[] { "real", "complex" })
        foreach (var expression in new[] { "arccot(-1)", "arcctg(-1)" })
        {
            var result = engine.Execute(new SymbolicRequest { Expression = expression, Domain = domain });
            Check.That(result.Status, Is.EqualTo("exact"));
            Check.That(result.Warnings, Is.Empty);
            var difference = MathS.FromString(SymbolicInput.Normalize(result.Result)) - 3 * MathS.pi / 4;
            Check.That(difference.Simplify(), Is.EqualTo((Entity)0), expression);
        }
    }

    [Test]
    public void FunctionPowersAndNestedCallsUseTheNativeBranchExactlyOnce()
    {
        var value = 3 * Math.PI / 4;
        AssertValue("2arccot²(-1)", "0", new Complex(2 * value * value, 0));
        AssertValue("arcctg³(-1)²", "0", new Complex(Math.Pow(value, 6), 0));
        AssertValue("arccot(arccot(-1))", "0", new Complex(Math.Atan2(1, value), 0));
        AssertValue("log(arccot(-1),2)", "0", new Complex(Math.Log(value, 2), 0));
    }

    [TestCase(-2d, 0d)]
    [TestCase(-1d, 0d)]
    [TestCase(0d, 0d)]
    [TestCase(1d, 0d)]
    [TestCase(2d, 0d)]
    [TestCase(-1d, 1d)]
    [TestCase(-1d, -1d)]
    [TestCase(1d, 1d)]
    [TestCase(1d, -1d)]
    [TestCase(0d, 2d)]
    [TestCase(0d, -2d)]
    public void BackendInverseCotangentOutputRetainsItsOwnBranchAndZeroValue(double real, double imaginary)
    {
        // A backend node can be produced by a CAS operation even when native
        // arccot input was normalized differently. Its formatter must not name
        // the distinct native function and change the value during reuse.
        var native = SymbolicOutput.Format(MathS.Arccotan(MathS.Var("x")));
        var value = new Complex(real, imaginary);
        var expected = value == Complex.Zero ? new Complex(Math.PI / 2, 0) : Complex.Atan(1 / value);
        AssertValue(native, ComplexText(real, imaginary), expected);
    }

    [TestCase("arccot()")]
    [TestCase("arccot(1,2)")]
    [TestCase("arcctg()")]
    [TestCase("arcctg(1,2)")]
    public void NativeInverseCotangentRejectsWrongArity(string expression) =>
        Check.Throws<ArgumentException>(() => engine.Execute(new SymbolicRequest { Expression = expression }));

    private void AssertValue(string expression, string replacement, Complex expected, string domain = "complex")
    {
        var result = engine.Execute(new SymbolicRequest
        {
            Operation = "evaluate", Expression = expression, Domain = domain, Precision = 30,
            Substitutions = new Dictionary<string, string> { ["x"] = replacement }
        });
        Check.That(result.Status, Is.EqualTo("approximate"), expression + " at " + replacement);
        var actual = (Entity.Number.Complex)MathS.FromString(SymbolicInput.Normalize(result.Result)).Evaled;
        var difference = new Complex(actual.RealPart.EDecimal.ToDouble(), actual.ImaginaryPart.EDecimal.ToDouble()) - expected;
        Check.That(difference.Magnitude, Is.LessThan(2e-12 * (1 + expected.Magnitude)), expression + " at " + replacement);
    }

    private static string ComplexText(double real, double imaginary) =>
        "(" + real.ToString("R", CultureInfo.InvariantCulture) + ") + (" +
        imaginary.ToString("R", CultureInfo.InvariantCulture) + ")·i";
}
