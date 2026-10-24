using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class NumericLiteralTranslationTests
{
    [TestCase("1_000", 1000)]
    [TestCase("1__000", 1000)]
    [TestCase("12_345_678", 12345678)]
    [TestCase("1_000.25", 1000.25)]
    [TestCase("1.000_25", 1.00025)]
    [TestCase("1_000e-2", 10)]
    [TestCase("1e1_0", 1e10)]
    [TestCase("1/1_000", 0.001)]
    [TestCase("1/-1_000", -0.001)]
    [TestCase("1/1__000", 0.001)]
    [TestCase("1/1_000.25", 1.0 / 1000.25)]
    [TestCase("1/1_000e2", 0.00001)]
    [TestCase("1/1e1_0", 1e-10)]
    public void DigitSeparatorsDoNotChangeTheNumericValue(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.EqualTo(expected));
    }

    [TestCase("1_000x", 250)]
    [TestCase("1_000(x+1)", 1250)]
    [TestCase("1e3(x)", 250)]
    [TestCase("2E-3(x)", 0.0005)]
    [TestCase("2e+3(x+1)", 2500)]
    [TestCase("1_000.25e-2(x)", 2.500625)]
    [TestCase("1e1_0(x)", 2.5e9)]
    [TestCase(".5x", 0.125)]
    [TestCase(".5e2(x)", 12.5)]
    [TestCase(".5(x+1)", 0.625)]
    [TestCase("1/1_000x", 0.00025)]
    [TestCase("1/2e3(x)", 0.000125)]
    public void ImplicitMultiplicationUsesTheWholeDecimalCoefficient(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 0.25), Is.EqualTo(expected));
    }

    [Test]
    public void SimilarlyNamedVariablesCannotSilentlyReplacePartsOfNumbers()
    {
        const string script = """
            var _000 = 7;
            var e3x = 9;
            var x = 0.25;
            write(1_000);
            write(":");
            write(1/1_000);
            write(":");
            write(1e3(x));
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("1000:0.001:250"));
    }

    [Test]
    public void CustomFunctionsAndScriptOutputRetainSeparatedNumbers()
    {
        const string definitions = "static double Scale(double x) { return 1/1_000 + 2e3(x); }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Scale(x)", 0.25, customFunctions: definitions),
            Is.EqualTo(500.001));
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(Scale(.25));", definitions),
            Is.EqualTo("500.001"));
    }

    [TestCase("2x", 0.5)]
    [TestCase("2(x+1)", 2.5)]
    [TestCase("2e", 2 * System.Math.E)]
    [TestCase("1/2", 0.5)]
    [TestCase("1/-2", -0.5)]
    [TestCase("1/-2e-3", -500)]
    [TestCase("1/(1+1)", 0.5)]
    [TestCase("(x+1)x", 0.3125)]
    public void ExistingDecimalAndImplicitMultiplicationRulesRemain(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 0.25), Is.EqualTo(expected));
    }

    [Test]
    public void OrdinaryIdentifiersStringsAndCommentsAreNotNumericLiterals()
    {
        const string script = """
            var value1_000 = 3;
            var _factor = 4;
            // These stay ordinary names, and literal text stays literal.
            write(value1_000 + 2_factor);
            write(":1_000 1/1_000 1e3x .5x");
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script),
            Is.EqualTo("11:1_000 1/1_000 1e3x .5x"));
    }
}
