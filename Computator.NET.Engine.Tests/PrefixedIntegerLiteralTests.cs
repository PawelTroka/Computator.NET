using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PrefixedIntegerLiteralTests
{
    [TestCase("0x10", 16)]
    [TestCase("0XfF", 255)]
    [TestCase("0b1010", 10)]
    [TestCase("0B1010", 10)]
    [TestCase("0x_FF", 255)]
    [TestCase("0x__FF", 255)]
    [TestCase("0xFF_FF", 65535)]
    [TestCase("0b_1010_0101", 165)]
    [TestCase("0b__10", 2)]
    [TestCase("0x10U", 16)]
    [TestCase("0x10l", 16)]
    [TestCase("0x10uL", 16)]
    [TestCase("0b10Lu", 2)]
    [TestCase("0xFFFF_FFFF", 4294967295)]
    [TestCase("0x8000_0000_0000_0000UL", 9223372036854775808d)]
    public void CompletePrefixedLiteralsRetainTheirValue(string source, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(source), Is.EqualTo(expected));

    [TestCase("1/0x10", 0.0625)]
    [TestCase("1/0b10", 0.5)]
    [TestCase("1/-0x10", -0.0625)]
    [TestCase("1/+0B10UL", 0.5)]
    [TestCase("1/-0x8000000000000000", -1.0842021724855044e-19)]
    [TestCase("1/(0x10)", 0.0625)]
    [TestCase("0x10/0b10", 8)]
    [TestCase("0x10²", 256)]
    [TestCase("-0x10²", -256)]
    [TestCase("(-0b10)²", 4)]
    [TestCase("0b10⁻¹", 0.5)]
    [TestCase("0x10ʸ", 256)]
    [TestCase("0x10(x+1)", 48)]
    [TestCase("2(0x10)", 32)]
    [TestCase("0b10*x", 4)]
    public void MathematicalDivisionPowersAndCoefficientsUseTheWholeLiteral(string source, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(source, 2, 2),
            Is.EqualTo(expected).Within(Math.Abs(expected) * 1e-13));

    [Test]
    public void SimilarlyNamedVariablesCannotSilentlyReplaceTheLiteral()
    {
        const string source = "var x10=7; var b11=9; write(0x10); write(\":\"); write(0b11);";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source), Is.EqualTo("16:3"));
    }

    [TestCase("write(0x10²);", "256")]
    [TestCase("write(0xF⁽¹⁺¹⁾);", "225")]
    [TestCase("var x=2; write(0x10²x);", "512")]
    [TestCase("write((0b10²)*(3));", "12")]
    [TestCase("var x=2; write(0x10 /* coefficient */ (x+1));", "48")]
    [TestCase("write(|0x10|);", "16")]
    public void ScriptNotationUsesTheSameCompleteOperand(string source, string expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source), Is.EqualTo(expected));

    [Test]
    public void UnsupportedUngroupedPoweredCoefficientRemainsACompilationError() =>
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => new PortableEngine().ExecuteScript("write(0b10²(3));"));

    [Test]
    public void ExactIntegersAndUnsignedTypesSurviveScriptCompilation()
    {
        const string source = """
            const ulong mask = 0xFFFF_FFFF_FFFF_FFFFUL;
            write(mask == ulong.MaxValue); write(":");
            write(0x20_0000_0000_0001L); write(":");
            write((0xFFFFFFFF).GetType().Name); write(":");
            write((0x1U).GetType().Name); write(":");
            write((0b1L).GetType().Name); write(":");
            write((0B1UL).GetType().Name); write(":");
            write(~0x0U); write(":");
            write((0b1010 & 0x3) << 0b10);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source),
            Is.EqualTo("True:9007199254740993:UInt32:UInt32:Int64:UInt64:4294967295:8"));
    }

    [TestCase("-0x80000000", "Int32", "-2147483648")]
    [TestCase("-0b1000_0000_0000_0000_0000_0000_0000_0000", "Int32", "-2147483648")]
    [TestCase("-0x8000000000000000", "Int64", "-9223372036854775808")]
    [TestCase("-0x8000000000000000L", "Int64", "-9223372036854775808")]
    [TestCase("-0x80000000U", "Int64", "-2147483648")]
    public void DirectSignedMinimaKeepTheirNativeIntegerType(string source, string type, string expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "var value=" + source + "; write(value.GetType().Name); write(\":\"); write(value);"),
            Is.EqualTo(type + ":" + expected));

    [TestCase("-(0x8000000000000000)")]
    [TestCase("-0x8000000000000000UL")]
    public void InvalidUnsignedNegationIsNotTurnedIntoASignedLiteral(string source) =>
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => new PortableEngine().ExecuteScript("write(" + source + ");"));

    [Test]
    public void SignedMinimumPowerKeepsUnaryPrecedence() =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("-0x80000000²"),
            Is.EqualTo(-4611686018427387904d));

    [Test]
    public void ConstantsWorkInCustomFunctionsArraysAndPatterns()
    {
        const string definitions = """
            enum Mask : uint { First = 0x1, All = 0xFFFF_FFFFU }
            static real Probe(real x) { return 0x10(x+1) + 1/0b10; }
            """;
        const string source = """
            var values = new[] { 0x10, 0b10 };
            write(values[0b1]); write(":");
            write(Probe(2)); write(":");
            write(Mask.All == (Mask)uint.MaxValue); write(":");
            switch(values[0]) { case 0x10: write("matched"); break; }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source, definitions),
            Is.EqualTo("2:48.5:True:matched"));
    }

    [Test]
    public void ComplexExpressionsAndNumericalMethodsShareTheLiteralSyntax()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateComplex("0x10+0b10*z", new Complex(1, 2)),
            Is.EqualTo(new Complex(18, 4)));
        NUnit.Framework.Assert.That(engine.Integrate("x/0x10", 0, 2, 2), Is.EqualTo(0.125).Within(1e-14));
    }

    [Test]
    public void LiteralsAndCommentsRemainUntouched()
    {
        const string source = """
            write("0x10 0b10 1/0x10 0x10²");
            // 0x10 and 0b10 are not executable here.
            write($":{0b10}");
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source),
            Is.EqualTo("0x10 0b10 1/0x10 0x10²:2"));
    }

    [TestCase("2x", 6)]
    [TestCase("0x", 0)]
    [TestCase("2e", 5.43656365691809)]
    [TestCase("1e3(x)", 3000)]
    [TestCase(".5x", 1.5)]
    [TestCase("1/1_000", 0.001)]
    public void ExistingDecimalAndIdentifierCoefficientsKeepTheirMeaning(string source, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(source, 3),
            Is.EqualTo(expected).Within(1e-13));

    [Test]
    public void IncompletePrefixesAndIdentifierTailsKeepTheExistingMultiplicationConvention()
    {
        const string source = """
            var xname=7; var banana=8; var x10value=9; var b10value=10; var x_=11;
            write(0xname); write(0banana); write(0x10value); write(0b10value); write(0x_);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source), Is.EqualTo("00000"));
    }

    [TestCase("x10000000000000000", "0x10000000000000000")]
    [TestCase("b10000000000000000000000000000000000000000000000000000000000000000", "0b10000000000000000000000000000000000000000000000000000000000000000")]
    [TestCase("x1_", "0x1_")]
    public void MalformedLiteralCannotSilentlyReadASimilarlyNamedVariable(string name, string literal) =>
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => new PortableEngine().ExecuteScript(
            "var " + name + "=7; write(" + literal + ");"));

    [TestCase("0b12")]
    [TestCase("0b102")]
    [TestCase("0x1G")]
    [TestCase("0x1_")]
    [TestCase("0x1UL2")]
    [TestCase("0x10000000000000000")]
    public void MalformedPrefixedNumbersDoNotBecomeValidNumbers(string source) =>
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => new PortableEngine().EvaluateReal(source));
}
