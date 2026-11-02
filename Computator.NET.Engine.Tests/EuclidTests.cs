using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class EuclidTests
{
    [TestCase("Euclid.GreatestCommonDivisor(-84,30)", 6)]
    [TestCase("Euclid.GreatestCommonDivisor(42,0)", 42)]
    [TestCase("Euclid.GreatestCommonDivisor(0,0)", 0)]
    [TestCase("Euclid.GreatestCommonDivisor(new integer[] {84,30,18})", 6)]
    [TestCase("Euclid.LeastCommonMultiple(-12,18)", 36)]
    [TestCase("Euclid.LeastCommonMultiple(0,18)", 0)]
    [TestCase("Euclid.LeastCommonMultiple(6,10,15)", 30)]
    [TestCase("Euclid.Modulus(-7,3)", 2)]
    [TestCase("Euclid.Modulus(7,-3)", -2)]
    [TestCase("Euclid.Modulus(-7.5,3.0)", 1.5)]
    [TestCase("Euclid.Remainder(-7,3)", -1)]
    [TestCase("Euclid.Remainder(7,-3)", 1)]
    [TestCase("Euclid.IsEven(-2) ? 1 : 0", 1)]
    [TestCase("Euclid.IsOdd(-3) ? 1 : 0", 1)]
    [TestCase("Euclid.IsPowerOfTwo(1024) ? 1 : 0", 1)]
    [TestCase("Euclid.IsPowerOfTwo(0) ? 1 : 0", 0)]
    [TestCase("Euclid.IsPerfectSquare(144) ? 1 : 0", 1)]
    [TestCase("Euclid.IsPerfectSquare(145) ? 1 : 0", 0)]
    [TestCase("Euclid.IsPerfectSquare(-1) ? 1 : 0", 0)]
    [TestCase("Euclid.PowerOfTwo(10)", 1024)]
    [TestCase("Euclid.PowerOfTwo((integer)40)", 1099511627776)]
    [TestCase("Euclid.CeilingToPowerOfTwo(9)", 16)]
    [TestCase("Euclid.CeilingToPowerOfTwo(0)", 0)]
    [TestCase("Euclid.Log2(1000)", 9)]
    public void QualifiedHelpersEvaluateInRealExpressions(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.EqualTo(expected));

    [Test]
    public void ScriptResultsKeepIntegersAboveTheExactDoubleRange()
    {
        var output = new PortableEngine().ExecuteScript(
            "integer result = Euclid.GreatestCommonDivisor(9007199254740993,0); write(result);");
        NUnit.Framework.Assert.That(output, Is.EqualTo("9007199254740993"));
    }

    [Test]
    public void ExtendedGcdSuppliesBezoutCoefficientsThroughOutArguments()
    {
        const string script = """
            integer u, v;
            integer divisor = Euclid.ExtendedGreatestCommonDivisor(240,46,out u,out v);
            write(divisor); write(":"); write(240*u+46*v);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("2:2"));
    }

    [Test]
    public void BigIntegerOverloadsAvoidFixedWidthOverflow()
    {
        const string script = """
            var scale = System.Numerics.BigInteger.One << 80;
            var divisor = Euclid.GreatestCommonDivisor(3*scale,5*scale);
            var multiple = Euclid.LeastCommonMultiple(3*scale,5*scale);
            System.Numerics.BigInteger u, v;
            var extended = Euclid.ExtendedGreatestCommonDivisor(3*scale,5*scale,out u,out v);
            write(divisor); write(":"); write(multiple);
            write(":"); write(3*scale*u+5*scale*v == extended);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script),
            Is.EqualTo("1208925819614629174706176:18133887294219437620592640:True"));
    }

    [Test]
    public void BigIntegerCalculatorResultUsesTheDocumentedExplicitConversion()
    {
        const string expression = "((real)Euclid.GreatestCommonDivisor(System.Numerics.BigInteger.One<<80,System.Numerics.BigInteger.Zero))";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression),
            Is.EqualTo(Math.ScaleB(1, 80)));
    }

    [Test]
    public void AliasIsAvailableInsideCustomFunctionDefinitions()
    {
        const string definitions = "static integer period(integer a, integer b) { return Euclid.LeastCommonMultiple(a,b); }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("period(12,18)", customFunctions: definitions),
            Is.EqualTo(36));
    }

    [TestCase("Euclid.PowerOfTwo(-1)")]
    [TestCase("Euclid.PowerOfTwo(31)")]
    [TestCase("Euclid.PowerOfTwo((integer)63)")]
    [TestCase("Euclid.CeilingToPowerOfTwo(1073741825)")]
    public void UpstreamPowerBoundsRemainVisible(string expression) =>
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => new PortableEngine().EvaluateReal(expression));

    [Test]
    public void IntegerZeroDivisorIsAnErrorAndFloatingZeroDivisorIsNaN()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.Throws<DivideByZeroException>(() => engine.EvaluateReal("Euclid.Modulus(7,0)"));
        NUnit.Framework.Assert.That(engine.EvaluateReal("Euclid.Modulus(7.0,0.0)"), Is.NaN);
    }
}
