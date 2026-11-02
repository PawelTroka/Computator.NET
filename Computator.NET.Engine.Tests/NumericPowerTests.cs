using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class NumericPowerTests
{
    [TestCase(".5²", 0.25)]
    [TestCase(".125³", 0.001953125)]
    [TestCase("1_000²", 1000000)]
    [TestCase("1__000²", 1000000)]
    [TestCase("1.2_5²", 1.5625)]
    [TestCase("1_000.25²", 1000500.0625)]
    [TestCase("1e3²", 1000000)]
    [TestCase("2E3²", 4000000)]
    [TestCase("2e+3²", 4000000)]
    [TestCase("2e-3²", 0.000004)]
    [TestCase(".5e2²", 2500)]
    [TestCase("1e1_0²", 1e20)]
    [TestCase("1_000e-2²", 100)]
    [TestCase(".25⁻¹", 4)]
    [TestCase("1_024⁽¹˸²⁾", 32)]
    public void PostfixPowerUsesTheCompleteDecimalLiteral(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression),
            Is.EqualTo(expected).Within(2e-13).Percent);
    }

    [TestCase("value2")]
    [TestCase("value123")]
    [TestCase("x1_000")]
    [TestCase("_123")]
    [TestCase("α2")]
    [TestCase("e3")]
    public void DigitsInsideIdentifiersRemainPartOfTheOperand(string name)
    {
        var script = "real " + name + " = 3; write(" + name + "²);";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("9"));
    }

    [TestCase("-.5²", -0.25)]
    [TestCase("(-.5)²", 0.25)]
    [TestCase("-2e3²", -4000000)]
    [TestCase("(-2e3)²", 4000000)]
    [TestCase("2²+3²", 13)]
    [TestCase(".5²+.25²", 0.3125)]
    [TestCase("2*.5²", 0.5)]
    [TestCase("1/.5²", 4)]
    [TestCase("(.5+.25)²", 0.5625)]
    [TestCase(".5⁽ʸ⁺¹⁾", 0.125)]
    public void UnarySignsGroupingAndSymbolicExponentsKeepTheirPrecedence(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, y: 2), Is.EqualTo(expected));
    }

    [Test]
    public void ImplicitCoefficientsStillMultiplyIdentifierPowers()
    {
        const string script = """
            real x=3, e3x=4, f=5;
            write(2x²); write(":");
            write(1e3x²); write(":");
            write(2f²); write(":");
            write(2e³);
            """;
        var expected = "18:16:50:" + (2 * Math.Pow(Math.E, 3)).ToString(System.Globalization.CultureInfo.InvariantCulture);
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(expected));
    }

    [Test]
    public void LiteralPowersWorkInCustomDefinitionsAndComplexExpressions()
    {
        const string definitions = "static real Scale(real value2) { return .5² + value2² + 2e-3²; }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Scale(3)", customFunctions: definitions),
            Is.EqualTo(9.250004));
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateComplex(".5²+z²", new System.Numerics.Complex(3, 4)),
            Is.EqualTo(new System.Numerics.Complex(-6.75, 24)).Using<System.Numerics.Complex>((a, b) =>
                System.Numerics.Complex.Abs(a - b) < 1e-13));
    }

    [Test]
    public void LiteralTextAndCommentsAreNotRewritten()
    {
        const string script = "write(\".5² 1_000² value2² 2e-3²\"); /* .5² 1_000² */";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(".5² 1_000² value2² 2e-3²"));
    }

    [Test]
    public void TranslationContainsTheWholeOperandOnce()
    {
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(".5²+1_000²+2e-3²+value2²"),
            Is.EqualTo("pow(.5,2)+pow(1_000,2)+pow(2e-3,2)+pow(value2,2)"));
    }
}
