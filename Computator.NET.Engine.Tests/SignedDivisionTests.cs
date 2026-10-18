using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class SignedDivisionTests
{
    [TestCase("1/-2", -0.5)]
    [TestCase("1/+2", 0.5)]
    [TestCase("6/-4", -1.5)]
    [TestCase("1/ - 2", -0.5)]
    [TestCase("1/+\t2", 0.5)]
    [TestCase("1/+-2", -0.5)]
    [TestCase("1/-+2", -0.5)]
    [TestCase("1/- -2", 0.5)]
    [TestCase("1/+ +2", 0.5)]
    [TestCase("1/-2/-3", 1.0 / 6)]
    [TestCase("1/-(1+1)", -0.5)]
    [TestCase("1/+(1+1)", 0.5)]
    [TestCase("1/-(-(1+1))", 0.5)]
    [TestCase("1/- - (1+1)", 0.5)]
    [TestCase("1/(2/-3)", -1.5)]
    [TestCase("-2147483648/-1", 2147483648.0)]
    [TestCase("1/-(-2147483648)", 1.0 / 2147483648)]
    [TestCase("1/- /* sign separated from divisor */ 2", -0.5)]
    [TestCase("1/-0", double.NegativeInfinity)]
    [TestCase("1/-(0)", double.NegativeInfinity)]
    [TestCase("1/2", 0.5)]
    [TestCase("1/(1+1)", 0.5)]
    [TestCase("1/-2.5", -0.4)]
    [TestCase("1/-2e-3", -500.0)]
    [TestCase("1/+2E3", 0.0005)]
    [TestCase("1/(2e-3)", 500.0)]
    public void DivisionByNumericAndParenthesizedOperandsUsesFloatingPointArithmetic(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.EqualTo(expected).Within(1e-14));
    }

    [TestCase("write(6/-4); write(\":\"); write(1/+2);", "-1.5:0.5")]
    [TestCase("var x=3; write(1/-(--x)); write(\":\"); write(x);", "-0.5:2")]
    [TestCase("var x=3; write(1/--x); write(\":\"); write(x);", "0:2")]
    [TestCase("var x=3; write(1/--(x)); write(\":\"); write(x);", "0:2")]
    [TestCase("var x=3; write(1/++(x)); write(\":\"); write(x);", "0:4")]
    [TestCase("var x=3; write(1/-x); write(\":\"); write(x);", "0:3")]
    public void ScriptsPreserveIncrementDecrementAndVariableDivisorSemantics(string script, string expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(expected));
    }

    [TestCase("1/-2", -0.5)]
    [TestCase("1/+2", 0.5)]
    [TestCase("1/-(1+1)", -0.5)]
    [TestCase("1/+(1+1)", 0.5)]
    public void CustomDefinitionsUseTheSameSignedDivisionRules(string expression, double expected)
    {
        var result = new PortableEngine().EvaluateReal("Probe()", customFunctions:
            "static double Probe() { return " + expression + "; }");
        NUnit.Framework.Assert.That(result, Is.EqualTo(expected));
    }
}
