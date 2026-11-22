using AngouriMath;
using AngouriMath.Core;
using Computator.NET.DataTypes.Text;
using Computator.NET.Engine.Symbolic;
using NUnit.Framework;
using Check = NUnit.Framework.Assert;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public sealed class SymbolicNotationTests
{
    [TestCase("x^2")]
    [TestCase("(-x)^y")]
    [TestCase("(x^2)^3")]
    [TestCase("x^(x^2)")]
    [TestCase("(x+y)^(-2)")]
    [TestCase("(1/3)^x")]
    [TestCase("(-1/3)^x")]
    [TestCase("1/(x*y)")]
    [TestCase("x*(y mod 3)")]
    [TestCase("x/(y/z)")]
    [TestCase("x-(y-z)")]
    [TestCase("x+(y-z)")]
    [TestCase("x-(y+z)")]
    [TestCase("1/3 + 2/5*i")]
    [TestCase("(1/3 + 2/5*i)^x")]
    [TestCase("sin(x)^2 + cos(x)^2")]
    [TestCase("log(2,x)")]
    [TestCase("log(log(2,x),log(3,y))")]
    [TestCase("log(2,x)^log(3,y)")]
    [TestCase("x^q")]
    [TestCase("x^n_1")]
    [TestCase("x^pi")]
    [TestCase("(x+1)!")]
    [TestCase("integral(sin(x^x),x)")]
    [TestCase("derivative(abs(x),x)")]
    [TestCase("limit(sin(x^x),x,0)")]
    [TestCase("{x^2,x*y,log(2,x),1/3}")]
    [TestCase("{ x : x^2=4 }")]
    [TestCase("[[x^2,log(2,x)],[x*y,pi]]")]
    [TestCase("piecewise(x^2 provided x>0,-x^2 provided x<=0)")]
    [TestCase("x^2 provided x!=0")]
    [TestCase("domain(x^2,RR)")]
    [TestCase("domain(log(2,x),RR)")]
    [TestCase("domain(x^2,Any)")]
    [TestCase("CC \\ { 0 }")]
    [TestCase("ZZ*")]
    [TestCase("ZZ+")]
    [TestCase("{ n in ZZ* : n<3 }")]
    [TestCase("card({ x^2,y^2 })")]
    [TestCase("powerset({ x^2,1 })")]
    [TestCase("domain(pi,RR)")]
    [TestCase("[-oo; pi]")]
    [TestCase("ComputatorNativeOutput_1*x^2 + ComputatorNativeOutput_2")]
    public void NativeFormattingPreservesTheMathematicalTree(string canonical)
    {
        var original = MathS.FromString(canonical, useCache: false);
        var native = SymbolicOutput.Format(original);
        var actual = MathS.FromString(SymbolicInput.Normalize(native), useCache: false);
        Check.That(actual.InnerSimplified, Is.EqualTo(original.InnerSimplified), canonical + " became " + native);
        Check.That(native, Does.Not.Contain(" * "));
        Check.That(native, Does.Not.Contain(" ^ "));
    }

    [TestCase("x^2", "x²")]
    [TestCase("x^(-2)", "x⁻²")]
    [TestCase("x^(n+1)", "x⁽ⁿ⁺¹⁾")]
    [TestCase("(x^2)^3", "(x²)³")]
    [TestCase("(-2)^x", "(-2)ˣ")]
    [TestCase("x^n_1", "pow(x, n_1)")]
    [TestCase("x^q", "pow(x, q)")]
    [TestCase("x^(y^2)", "pow(x, y²)")]
    [TestCase("log(2,x)", "log(x, 2)")]
    [TestCase("(x+1)!", "factorial(x + 1)")]
    [TestCase("pi*x^2", "PI · x²")]
    [TestCase("+oo", "∞")]
    [TestCase("-oo", "-∞")]
    public void NativeOutputUsesTheSharedEditorAlphabetAndSafeFallbacks(string canonical, string expected) =>
        Check.That(SymbolicOutput.Format(MathS.FromString(canonical, useCache: false)), Is.EqualTo(expected));

    [TestCase("x  ²   ˸  ʸ ˙  ²", "x^(2/y*2)")]
    [TestCase("2² ⁺ ¹", "2^(2+1)")]
    [TestCase("x⁽ʸ ⁺ ¹⁾", "x^(y+1)")]
    [TestCase("xᶜᵒˢ⁽ˣ⁾", "x^cos(x)")]
    [TestCase("x⁰ᱸ⁵", "x^(1/2)")]
    [TestCase("xꞋʸꞋ", "x^abs(y)")]
    [TestCase("xˡᵒᵍ⁽⁸ʹ²⁾", "x^log(2,8)")]
    [TestCase("sin ² (x)", "sin(x)^2")]
    [TestCase("2cosʸ(x)", "2*cos(x)^y")]
    [TestCase("cosʸ(x)²", "(cos(x)^y)^2")]
    [TestCase("cos²(x)^3", "(cos(x)^2)^3")]
    [TestCase("sinʸ(cosˣ(x))²", "(sin(cos(x)^x)^y)^2")]
    [TestCase("1e3sin²(x)", "1000*sin(x)^2")]
    [TestCase("1_000log10²(x)", "1000*log(10,x)^2")]
    [TestCase("log²(8,2)", "log(2,8)^2")]
    [TestCase("log(log(256,2),2)", "log(2,log(2,256))")]
    [TestCase("log²(log(256,2),2)", "log(2,log(2,256))^2")]
    [TestCase("logˡᵒᵍ⁽⁸ʹ²⁾(log(256,2),2)", "log(2,log(2,256))^log(2,8)")]
    [TestCase("tg(x)+ctg(x)+arctg(x)+arcctg(x)", "tan(x)+cotan(x)+arctan(x)+arccotan(x)")]
    [TestCase("tgh(x)+ctgh(x)+artgh(x)+arctgh(x)", "tanh(x)+coth(x)+atanh(x)+acoth(x)")]
    [TestCase("PI + π + ℯ", "pi+pi+e")]
    [TestCase("Infinity", "+oo")]
    public void NativeInputDecodesWholeExponentRunsAndNestedCallArgumentsOnce(string native, string canonical) =>
        Check.That(MathS.FromString(SymbolicInput.Normalize(native), useCache: false).InnerSimplified,
            Is.EqualTo(MathS.FromString(canonical, useCache: false).InnerSimplified));

    [TestCase("root(-8,3)", "-2")]
    [TestCase("root(-8,-3)", "-1/2")]
    [TestCase("root(16,2)", "4")]
    [TestCase("root(-16,2)", "4*i")]
    [TestCase("root(root(-64,3),2)", "2*i")]
    public void RootNotationRetainsTslRealOddRoots(string native, string canonical)
    {
        var actual = new SymbolicEngine().Execute(new SymbolicRequest { Expression = native });
        Check.That(MathS.FromString(SymbolicInput.Normalize(actual.Result), useCache: false).InnerSimplified,
            Is.EqualTo(MathS.FromString(canonical, useCache: false).InnerSimplified));
        Check.That(actual.Status, Is.EqualTo("exact"));
    }

    [Test]
    public void EveryMappedExponentCharacterUsesTheSharedMapping()
    {
        const string exponent = "(a+b)*c/d+log(8,2)+abs(y)+0.5";
        var native = "x" + SpecialSymbols.AsciiToSuperscript(exponent);
        var expected = MathS.FromString("x^((a+b)*c/d+log(2,8)+abs(y)+1/2)", useCache: false);
        Check.That(MathS.FromString(SymbolicInput.Normalize(native), useCache: false).InnerSimplified, Is.EqualTo(expected.InnerSimplified));
    }

    [Test]
    public void ApproximateComplexPartsKeepTheirRequestedDigits()
    {
        using var downcast = MathS.Settings.DowncastingEnabled.Set(false);
        var original = MathS.FromString("0.333333333333333333333333333333-0.142857142857142857142857142857*i", useCache: false).Evaled;
        var text = SymbolicOutput.Format(original);
        Check.That(text, Does.Contain("0.333333333333333333333333333333"));
        Check.That(text, Does.Contain("0.142857142857142857142857142857 · i"));
        var recovered = (Entity.Number.Complex)MathS.FromString(SymbolicInput.Normalize(text), useCache: false).Evaled;
        var expected = (Entity.Number.Complex)original;
        Check.That(recovered.RealPart.EDecimal.CompareTo(expected.RealPart.EDecimal), Is.Zero);
        Check.That(recovered.ImaginaryPart.EDecimal.CompareTo(expected.ImaginaryPart.EDecimal), Is.Zero);
    }

    [Test]
    public void ExpandingNestedRootNotationIsBoundedBeforeTheCasParser()
    {
        var expression = string.Concat(Enumerable.Repeat("root(", 20)) + "x" + string.Concat(Enumerable.Repeat(",3)", 20));
        Check.Throws<ArgumentException>(() => new SymbolicEngine().Execute(new SymbolicRequest { Expression = expression }));
    }

    [Test]
    public void NestedUnaryMinusNeverBecomesATslDecrementOperator()
    {
        var expression = new Entity.Mulf(-1, new Entity.Mulf(-1, MathS.Var("x")));
        var native = SymbolicOutput.Format(expression);
        Check.That(native, Is.EqualTo("-(-x)"));
        Check.That(new PortableEngine().EvaluateReal(native, x: 3), Is.EqualTo(3));
    }

    [Test]
    public void PrefixThenPostfixPowerMatchesIndependentNumericalTsl()
    {
        const string expression = "cosʸ(x)²";
        var result = new SymbolicEngine().Execute(new SymbolicRequest
        {
            Operation = "evaluate", Expression = expression,
            Substitutions = new Dictionary<string, string> { ["x"] = ".25", ["y"] = "3" }
        });
        var actual = double.Parse(result.Result, System.Globalization.CultureInfo.InvariantCulture);
        var expected = Math.Pow(Math.Pow(Math.Cos(.25), 3), 2);
        Check.That(actual, Is.EqualTo(expected).Within(1e-14));
        Check.That(new PortableEngine().EvaluateReal(expression, .25, 3), Is.EqualTo(expected).Within(1e-14));
    }
}
