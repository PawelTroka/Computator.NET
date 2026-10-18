using System.Numerics;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Compilation.TslHandlers;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class SymbolicFunctionPowerBindingTests
{
    [TestCase("cosᶻ(z)")]
    [TestCase("System.Math.Cosʸ(x)")]
    [TestCase("ψⁿ(x)")]
    [TestCase("@cosʸ(x)")]
    public void BareSuperscriptNamesWaitForSemanticResolution(string source) =>
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(source, string.Empty, deferSymbolicFunctionPowers: true), Is.EqualTo(source));

    [TestCase("cos⁽ᶻ⁺¹⁾(z)", "pow(@cos(z),(z+1))")]
    [TestCase("cos⁻ʸ(x)", "pow(@cos(x),-y)")]
    [TestCase("System.Math.Cos⁽ʸ⁺¹⁾(x)", "pow(System.Math.@Cos(x),(y+1))")]
    [TestCase("ψⁿ⁽ʸ⁺¹⁾(x)", "pow(@ψⁿ(x),(y+1))")]
    public void ExplicitPrefixExponentsHaveUnambiguousCallBoundaries(string source, string expected) =>
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(source, string.Empty, deferSymbolicFunctionPowers: true), Is.EqualTo(expected));

    [TestCase("cosʸ(x)")]
    [TestCase("cos⁽ʸ⁾(x)")]
    [TestCase("System.Math.Cosʸ(x)")]
    [TestCase("cos(x)ʸ")]
    public void RealExponentRaisesTheFunctionResult(string expression)
    {
        const double x = 0.73, y = 1.4;
        var expected = Math.Pow(Math.Cos(x), y);
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, x, y),
            Is.EqualTo(expected).Within(1e-12));
    }

    [TestCase("2System.Math.Cosʸ(x)x")]
    [TestCase("2κύμαʸ(x)x")]
    public void DeferredQualifiedAndUnicodeCallsRetainImplicitMultiplication(string expression)
    {
        const double x = 0.73, y = 1.4;
        const string definitions = "static real κύμα(real value) { return Math.Cos(value); }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, x, y, definitions),
            Is.EqualTo(2 * Math.Pow(Math.Cos(x), y) * x).Within(1e-12));
    }

    [TestCase("real Helper(real cosʸ) { return cosʸ; }")]
    [TestCase("real Helper() { real cosʸ=3; return cosʸ; }")]
    public void ANameInAnUnrelatedLocalScopeDoesNotHideTheFunctionPower(string helper)
    {
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            helper + " real y=2; write(cosʸ(0));"), Is.EqualTo("1"));
    }

    [TestCase("cosᶻ(z)")]
    [TestCase("cos⁽ᶻ⁾(z)")]
    [TestCase("System.Numerics.Complex.Cosᶻ(z)")]
    [TestCase("cos(z)ᶻ")]
    public void ComplexExponentUsesTheExistingComplexPowerBranch(string expression)
    {
        var z = new Complex(0.7, 0.4);
        var expected = Complex.Pow(Complex.Cos(z), z);
        var actual = new PortableEngine().EvaluateComplex(expression, z);
        NUnit.Framework.Assert.That((actual - expected).Magnitude, Is.LessThanOrEqualTo(1e-12));
    }

    [Test]
    public void GroupedAndSignedExponentsUseTheirWholeExpression()
    {
        const double x = 0.73, y = 1.4;
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("cos⁽ʸ⁺¹⁾(x)", x, y),
            Is.EqualTo(Math.Pow(Math.Cos(x), y + 1)).Within(1e-12));
        NUnit.Framework.Assert.That(engine.EvaluateReal("cos⁻ʸ(x)", x, y),
            Is.EqualTo(Math.Pow(Math.Cos(x), -y)).Within(1e-12));
    }

    [Test]
    public void NestedCallsAndNumericPostfixPowersRetainTheirOrder()
    {
        var z = new Complex(0.7, 0.4);
        var expected = Complex.Pow(Complex.Pow(Complex.Cos(Complex.Pow(Complex.Sin(z), z)), z), 2);
        var actual = new PortableEngine().EvaluateComplex("cosᶻ(sinᶻ(z))²", z);
        NUnit.Framework.Assert.That((actual - expected).Magnitude, Is.LessThanOrEqualTo(1e-12));
    }

    [Test]
    public void SeveralSuperscriptLettersCanNameOneConstant()
    {
        const double x = 0.73;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("cosᴾᴵ(x)", x),
            Is.EqualTo(Math.Pow(Math.Cos(x), Math.PI)).Within(1e-12));
    }

    [Test]
    public void UnicodeFunctionAndExponentIdentifiersBindInTheirOwnScope()
    {
        const string definitions = "static real κύμα(real value) { return value+1; }";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "real α=3; write(κύμαᵅ(2));", definitions), Is.EqualTo("27"));
    }

    [Test]
    public void ExistingSuperscriptFunctionNamesWinOverPowerInterpretation()
    {
        const string definitions = """
            static real n=3;
            static real ψ(real value) { return value+1; }
            static real ψⁿ(real value) { return value+5; }
            """;
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("ψⁿ(x)", 2, 2, definitions), Is.EqualTo(7));
        NUnit.Framework.Assert.That(engine.EvaluateReal("ψⁿʸ(x)", 2, 2, definitions), Is.EqualTo(49));
        NUnit.Framework.Assert.That(engine.EvaluateReal("ψⁿ⁽ʸ⁺¹⁾(x)", 2, 2, definitions), Is.EqualTo(343));
    }

    [Test]
    public void AnExistingFullNameWithWrongArgumentsRetainsItsOverloadError()
    {
        const string definitions = """
            static real Probe(real value) { return value+1; }
            static real Probeʸ(string value) { return 7; }
            """;
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().EvaluateReal("Probeʸ(x)", 2, 3, definitions));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(diagnostic => diagnostic.Code == "CS1503"), Is.True);
    }

    [Test]
    public void AmbiguousIdentifierSplitsRequireExplicitGroupingOrPostfix()
    {
        const string definitions = """
            static real ab=2;
            static real b=3;
            static real Probe(real value) { return value+1; }
            static real Probeᵃ(real value) { return value+2; }
            """;
        var engine = new PortableEngine();
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            engine.EvaluateReal("Probeᵃᵇ(2)", customFunctions: definitions));
        NUnit.Framework.Assert.That(engine.EvaluateReal("Probe⁽ᵃᵇ⁾(2)", customFunctions: definitions), Is.EqualTo(9));
        NUnit.Framework.Assert.That(engine.EvaluateReal("Probeᵃ⁽ᵇ⁾(2)", customFunctions: definitions), Is.EqualTo(64));
        NUnit.Framework.Assert.That(engine.EvaluateReal("Probeᵃ(2)ᵇ", customFunctions: definitions), Is.EqualTo(64));
    }

    [TestCase("static real Probeᵃ(real first, real second) { return 11; }")]
    [TestCase("static real Probeᵃ(string value) { return 11; }")]
    public void AnInvalidAlternativeOverloadDoesNotMakeAValidPowerAmbiguous(string alternative)
    {
        var definitions = "static real ab=2; static real b=3; " +
            "static real Probe(real value) { return value+1; } " + alternative;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Probeᵃᵇ(2)", customFunctions: definitions),
            Is.EqualTo(9));
    }

    [Test]
    public void RewritingAReceiverPreservesItsExactMemberName()
    {
        const string definitions = """
            public class Receiver {
                public real Probe(real value) { return value+1; }
                public real Probeʸ(real value) { return 99; }
            }
            static Receiver Make() { return new Receiver(); }
            static Receiver pow(Receiver value, real exponent) { return value; }
            static real pow(real value, real exponent) { return Math.Pow(value, exponent); }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Makeʸ().Probeʸ(2)", y: 3,
            customFunctions: definitions), Is.EqualTo(99));
    }

    [TestCase("Probeʸ⁺¹(x)")]
    [TestCase("Probeʸ⁻¹(x)")]
    public void UngroupedMixedExponentsDoNotSilentlyReinterpretLegacyNames(string expression) =>
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().EvaluateReal(expression, 0.73, 3, "static real Probe(real value) { return value+1; }"));

    [Test]
    public void NumericPrefixOnAnExistingSuperscriptNameKeepsItsLegacyMeaning()
    {
        const string definitions = "static real cosʸ(real value) { return value+2; }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("cosʸ⁺¹(x)", 3, 4, definitions), Is.EqualTo(5));
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("cosʸ⁻¹(x)", 3, 4, definitions),
            Is.EqualTo(0.2).Within(1e-12));
    }

    [TestCase("cosⁿ(x)")]
    [TestCase("missingʸ(x)")]
    [TestCase("@cosʸ(x)")]
    [TestCase("System.Math.@Cosʸ(x)")]
    public void UnboundExponentsFunctionsAndEscapedNamesRemainErrors(string expression) =>
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().EvaluateReal(expression, 0.73, 3));

    [Test]
    public void EscapedExistingFunctionNamesRemainCallable()
    {
        const string definitions = "static real cosʸ(real value) { return value+2; }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("@cosʸ(x)", 3, 4, definitions), Is.EqualTo(5));
    }

    [Test]
    public void NonNumericExponentBindingsDoNotInventAValidPower()
    {
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => new PortableEngine().EvaluateReal(
            "cosᵗᵉˣᵗ(x)", 0.73, customFunctions: "static string text=\"three\";"));
    }

    [Test]
    public void CustomFunctionBodiesAndTheirCallersResolveInTheSameCompilation()
    {
        const string definitions = """
            static real Inner(real value) { return value+1; }
            static real Outer(real value) { real y=2; return Innerʸ(value); }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Outerʸ(x)", 2, 3, definitions), Is.EqualTo(729));
    }

    [Test]
    public void LocalTslFunctionsCanHaveASymbolicPower()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.ExecuteScript("var f(real value)=value+1; real y=3; write(fʸ(2));"),
            Is.EqualTo("27"));
    }

    [Test]
    public void DynamicCallsKeepRuntimeBindingToTheirCompleteName()
    {
        const string definitions = """
            public class Receiver {
                public real Probe(real value) { return value+1; }
                public real Probeʸ(real value) { return value+10; }
            }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "dynamic receiver=new Receiver(); real y=3; write(receiver.Probeʸ(2));", definitions), Is.EqualTo("12"));
    }

    [Test]
    public void ArgumentsFunctionAndExponentAreEvaluatedOnceInThatOrder()
    {
        const string definitions = """
            static string trace="";
            static real First() { trace+="a"; return 1; }
            static real Second() { trace+="b"; return 2; }
            static real Probe(real first, real second) { trace+="f"; return first+second; }
            static real y { get { trace+="e"; return 2; } }
            static string Trace() { return trace; }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(
            "write(Probeʸ(First(),Second())); write(Trace());", definitions), Is.EqualTo("9abfe"));
    }

    [TestCase("""write("cosʸ(x), cos⁽ʸ⁺¹⁾(x), ψⁿ(x)");""", "cosʸ(x), cos⁽ʸ⁺¹⁾(x), ψⁿ(x)")]
    [TestCase("""write(@"cosʸ(x) and cos⁽ʸ⁺¹⁾(x)");""", "cosʸ(x) and cos⁽ʸ⁺¹⁾(x)")]
    [TestCase("write(\"\"\"cosʸ(x), cos⁽ʸ⁺¹⁾(x)\"\"\");", "cosʸ(x), cos⁽ʸ⁺¹⁾(x)")]
    [TestCase("""real y=2; write($"cosʸ(x) {cosʸ(0)}");""", "cosʸ(x) 1")]
    [TestCase("""real y=2; write($@"cos⁽ʸ⁺¹⁾(x) {cos⁽ʸ⁺¹⁾(0)}");""", "cos⁽ʸ⁺¹⁾(x) 1")]
    [TestCase("real y=2; write($$\"\"\"cosʸ(x) {literal} {{cosʸ(0)}}\"\"\");", "cosʸ(x) {literal} 1")]
    [TestCase("/* missingᶻ(z) cos⁽ᶻ⁺¹⁾(z) */ real y=2;\nwrite(cosʸ(0)); // missingᶻ(z)", "1")]
    public void OnlyExecutablePartsOfLiteralsAndCommentsCanContainPowerNotation(string script, string expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(expected));

    [Test]
    public void UnresolvedCallsStillReportTheOriginalDocumentAndLine()
    {
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().ExecuteScript("real y=3;\nwrite(cosʸ(0));\nwrite(missingʸ(0));"));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(diagnostic =>
            diagnostic.File == "script.tsl" && diagnostic.Line == 3 && diagnostic.Code == "CS0103"), Is.True);
    }

    [Test]
    public void ResolutionDoesNotReuseDefinitionsFromAnEarlierRequest()
    {
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("cosʸ(0)", y: 3,
            customFunctions: "static real cosʸ(real value) { return 7; }"), Is.EqualTo(7));
        NUnit.Framework.Assert.That(engine.EvaluateReal("cosʸ(0)", y: 3), Is.EqualTo(1));
    }

    [Test]
    public void LiteralCaretRetainsCSharpExclusiveOrSemantics() =>
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(5 ^ 3);"), Is.EqualTo("6"));
}
