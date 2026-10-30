using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class NumericLiteralPowerTests
{
    [TestCase("1_000²", 1000000.0)]
    [TestCase("1__000²", 1000000.0)]
    [TestCase("1_000.25²", 1000500.0625)]
    [TestCase("1.000_25²", 1.0005000625)]
    [TestCase(".5²", .25)]
    [TestCase(".5e2²", 2500.0)]
    [TestCase("1e3²", 1000000.0)]
    [TestCase("1E+3²", 1000000.0)]
    [TestCase("1e-3²", .000001)]
    [TestCase("1_0e1_0²", 1e22)]
    [TestCase("1.5e2²", 22500.0)]
    [TestCase("-1e3²", -1000000.0)]
    [TestCase("(-1e3)²", 1000000.0)]
    [TestCase("+.5²", .25)]
    [TestCase("-.5²", -.25)]
    [TestCase(".5⁻²", 4.0)]
    [TestCase("1_000⁻¹", .001)]
    [TestCase("1e3⁰", 1.0)]
    [TestCase("1_024⁰ᱸ⁵", 32.0)]
    [TestCase("1e3ʸ", 1000000000.0)]
    [TestCase(".5⁽ʸ⁺¹⁾", .0625)]
    [TestCase("1_000 ²", 1000000.0)]
    [TestCase("1_000²+.5²", 1000000.25)]
    [TestCase("(1_0²+.5²)²", 10050.0625)]
    [TestCase("1e3²x", 2000000.0)]
    [TestCase("2.5²", 6.25)]
    [TestCase("-2²", -4.0)]
    [TestCase("(-2)²", 4.0)]
    public void PowersUseTheWholeSupportedDecimalLiteral(string expression, double expected)
    {
        var actual = new PortableEngine().EvaluateReal(expression, 2, 3);
        NUnit.Framework.Assert.That(actual,
            Is.EqualTo(expected).Within(Math.Max(Math.Abs(expected) * 2e-15, double.Epsilon)));
    }

    [TestCase("x1")]
    [TestCase("value12")]
    [TestCase("value1_000")]
    [TestCase("_000")]
    [TestCase("δ2")]
    [TestCase("e3")]
    public void IdentifierDigitsAreNotSeparatePowerOperands(string name)
    {
        var definitions = "static real " + name + " = 3;";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(name + "²", customFunctions: definitions),
            Is.EqualTo(9));
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("2(" + name + "²)", customFunctions: definitions),
            Is.EqualTo(18));
    }

    [Test]
    public void ScriptVariablesAndCustomFunctionParametersRetainTheirNumericSuffixes()
    {
        const string definitions = "static real Square(real x1) { return x1²; }";
        const string script = "var value1 = 3.0; write(value1²); write(\":\"); write(Square(4));";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, definitions), Is.EqualTo("9:16"));
    }

    [Test]
    public void ScientificAndSeparatedLiteralsCannotCallSimilarlyNamedFunctions()
    {
        const string definitions = """
            static real epow(real x, real y) { return -1; }
            static real _pow(real x, real y) { return -2; }
            static real Scale() { return 1e3² + 1_000² + .5²; }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(Scale());", definitions),
            Is.EqualTo("2000000.25"));
    }

    [Test]
    public void ExistingEulerAndIdentifierCoefficientConventionsRemain()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("2e²"), Is.EqualTo(2 * Math.E * Math.E).Within(1e-13));
        NUnit.Framework.Assert.That(engine.EvaluateReal("2e-10e²"),
            Is.EqualTo(2 * Math.E - 10 * Math.E * Math.E).Within(1e-13));
        NUnit.Framework.Assert.That(engine.EvaluateReal("1e3x²", customFunctions: "static real e3x = 3;"),
            Is.EqualTo(9));
    }

    [Test]
    public void NumericPowersComposeWithComplexFunctionMemberAndIndexPowers()
    {
        const string definitions = "static real[] values = { 3, 4 }; static real Read() { return 2; }";
        const string expression = "1e1²+values[0]²+values[1]²+Read()²+z.Real²+z.Imaginary²";
        var actual = new PortableEngine().EvaluateComplex(expression, new Complex(3, 4), definitions);
        NUnit.Framework.Assert.That(actual, Is.EqualTo(new Complex(154, 0)));
    }

    [Test]
    public void NumericPowersRemainLiteralTextInStringsAndComments()
    {
        const string script = """
            // .5², 1e3², 1_000², x1²
            write(".5², 1e3², 1_000², x1²");
            write($" = {1e1²}");
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script),
            Is.EqualTo(".5², 1e3², 1_000², x1² = 100"));
    }
}
