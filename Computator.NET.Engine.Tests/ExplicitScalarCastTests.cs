using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ExplicitScalarCastTests
{
    [TestCase("(double)x", 3.75)]
    [TestCase("(real)x", 3.75)]
    [TestCase("(int)x", 3)]
    [TestCase("(integer)x", 3)]
    [TestCase("(natural)x", 3)]
    [TestCase("(float)x", 3.75)]
    [TestCase("(byte)x", 3)]
    [TestCase("(sbyte)x", 3)]
    [TestCase("(short)x", 3)]
    [TestCase("(ushort)x", 3)]
    [TestCase("(uint)x", 3)]
    [TestCase("(long)x", 3)]
    [TestCase("(ulong)x", 3)]
    [TestCase("(char)x", 3)]
    [TestCase("(double)(decimal)x", 3.75)]
    [TestCase("(real)(integer)x", 3)]
    [TestCase("(double)(int)(short)x", 3)]
    [TestCase("(int)(x+1)", 4)]
    [TestCase("(double)(x+1)", 4.75)]
    [TestCase("(double)-x", -3.75)]
    [TestCase("(real)(-x)", -3.75)]
    [TestCase("(int)+x", 3)]
    [TestCase("(int)x/2", 1.5)]
    [TestCase("(int)x²", 14)]
    [TestCase("((int)x)²", 9)]
    [TestCase("(real)/* cast */x", 3.75)]
    public void ExplicitScalarCastsRetainTheirOperandAndPrecedence(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 3.75), Is.EqualTo(expected));
    }

    [TestCase("1/(double)2", 0.5)]
    [TestCase("1/(real)2", 0.5)]
    [TestCase("1/(float)2", 0.5)]
    [TestCase("1/(int)2.75", 0)]
    [TestCase("1.0/(int)2.75", 0.5)]
    [TestCase("1/(integer)2.75", 0)]
    [TestCase("1.0/(natural)2.75", 0.5)]
    [TestCase("1/(double)(int)2.75", 0.5)]
    [TestCase("1/-(double)2", -0.5)]
    [TestCase("1/+(double)2", 0.5)]
    [TestCase("1/(double)-2", -0.5)]
    [TestCase("1/(double)+2", 0.5)]
    [TestCase("1/- (double)-2", 0.5)]
    [TestCase("1/(real)(x+0.25)", 0.25)]
    [TestCase("1/((int)2.75)", 0.5)]
    [TestCase("1/(double)/* cast */2", 0.5)]
    public void DivisionRecognizesACastInsteadOfPromotingItsTypeName(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 3.75), Is.EqualTo(expected));
    }

    [TestCase("(x)y", 6)]
    [TestCase("(x+1)y", 9)]
    [TestCase("(x+1)(y+1)", 12)]
    [TestCase("sin(x)y", 3 * 0.9092974268256817)]
    [TestCase("1/(2)", 0.5)]
    [TestCase("1/(x+2)", 0.25)]
    [TestCase("1/-(2)", -0.5)]
    public void OrdinaryImplicitMultiplicationAndParenthesizedDivisionRemain(string expression, double expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, 2, 3), Is.EqualTo(expected).Within(1e-14));
    }

    [Test]
    public void CastDivisorsWorkInScriptsAndCustomFunctions()
    {
        const string definitions = "static real Reciprocal(real value) { return 1/(real)(integer)value; }";
        const string script = "real x=3.75; write(1/(double)2); write(\":\"); write(Reciprocal(x));";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, definitions), Is.EqualTo("0.5:0.3333333333333333"));
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Reciprocal(x)", 4.75, customFunctions: definitions), Is.EqualTo(0.25));
    }

    [Test]
    public void CastOperandWithSideEffectsIsEvaluatedOnce()
    {
        const string definitions = "static int Calls; static real Next() { Calls++; return 2.75; }";
        const string script = "write(1/(double)(int)Next()); write(\":\"); write(Calls);";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, definitions), Is.EqualTo("0.5:1"));
    }

    [TestCase("(complex)2+z", 3, 2)]
    [TestCase("(complex)(2+3)", 5, 0)]
    [TestCase("1/(complex)2", 0.5, 0)]
    public void ComplexAliasCastsUseTheSameSharedTranslation(string expression, double real, double imaginary)
    {
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateComplex(expression, new System.Numerics.Complex(1, 2)),
            Is.EqualTo(new System.Numerics.Complex(real, imaginary)));
    }

    [Test]
    public void CastNotationInsideLiteralTextRemainsUnchanged()
    {
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(\"(real)x 1/(double)2\");"),
            Is.EqualTo("(real)x 1/(double)2"));
    }

    [Test]
    public void AFunctionArgumentNamedLikeAnAliasStillUsesImplicitMultiplication()
    {
        const string definitions = "static real Probe(real value) { return value+1; }";
        const string script = "real real=2; write(Probe(real)3);";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, definitions), Is.EqualTo("9"));
    }

    [Test]
    public void AliasCastHasTheSameMeaningAtTheStartOfAnExpressionAndInsideACall()
    {
        const string definitions = "static real real=2;";
        var engine = new PortableEngine();
        NUnit.Framework.Assert.That(engine.EvaluateReal("(real)3", customFunctions: definitions), Is.EqualTo(3));
        NUnit.Framework.Assert.That(engine.ExecuteScript("write((real)3);", definitions), Is.EqualTo("3"));
        NUnit.Framework.Assert.That(engine.EvaluateReal("real*3", customFunctions: definitions), Is.EqualTo(6));
    }

    [TestCase("1/(real)", "0.5")]
    [TestCase("1/(real)-1", "-0.5")]
    [TestCase("1/(real)+1", "1.5")]
    [TestCase("1/(real).CompareTo(1)", "1")]
    [TestCase("1/(real)!=0", "True")]
    [TestCase("1/(real) is double", "True")]
    [TestCase("1/(real) as object", "0.5")]
    [TestCase("1/(real)!", "0.5")]
    [TestCase("1/(real)!+1", "1.5")]
    [TestCase("1/(real)! is double", "True")]
    [TestCase("(1/(real)!, 1).Item1", "0.5")]
    public void AGroupedVariableNamedLikeAnAliasStillPromotesIntegerDivision(string expression, string expected)
    {
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("var real=2; write(" + expression + ");"), Is.EqualTo(expected));
    }

    [TestCase("(double)x")]
    [TestCase("(real)(integer)x")]
    [TestCase("1/(double)2")]
    [TestCase("1/-(real)(int)x")]
    [TestCase("return (double)x;")]
    [TestCase("return(double)x;")]
    [TestCase("return(real)x;")]
    public void ScalarCastTranslationDoesNotInsertArithmeticIntoTheType(string source)
    {
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(source), Is.EqualTo(source));
    }

    [TestCase("return(real)x;", 2.75)]
    [TestCase("return(int)x;", 2)]
    [TestCase("return(x)2;", 5.5)]
    public void ReturnWithoutWhitespacePreservesCastsAndMathematicalProducts(string statement, double expected)
    {
        var definitions = "static real Probe(real x) { " + statement + " }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("Probe(x)", 2.75, customFunctions: definitions), Is.EqualTo(expected));
    }

    [Test]
    public void AnEscapedReturnCallableStillAllowsImplicitMultiplication()
    {
        const string definitions = "static real @return(real x) { return x+1; }";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("real real=2; write(@return(real)3);", definitions), Is.EqualTo("9"));
    }
}
