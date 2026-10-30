using System.Globalization;
using System.Numerics;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PolynomialExecutionTests
{
    [TestCase("Polynomial.Evaluate(x,1,-3,2)",0,1)]
    [TestCase("Polynomial.Evaluate(x,1,-3,2)",1,0)]
    [TestCase("Polynomial.Evaluate(x,1,-3,2)",2,3)]
    [TestCase("Polynomial.Evaluate(x,new real[0])",5,0)]
    [TestCase("Polynomial.Zero.Evaluate(x)",5,0)]
    [TestCase("new Polynomial().Evaluate(x)",5,0)]
    [TestCase("new Polynomial(1,-3,2).Differentiate().Evaluate(x)",2,5)]
    [TestCase("new Polynomial(1,-3,2).Evaluate(x)",-2,15)]
    [TestCase("new Polynomial(1,-3,2).Evaluate(x)",0.5d,0)]
    [TestCase("new Polynomial(1,-3,2).Integrate().Differentiate().Evaluate(x)",2,3)]
    [TestCase("new Polynomial(1,-3,2).Integrate().Evaluate(x)",3,7.5d)]
    [TestCase("new Polynomial(3.0).Evaluate(x)",5,3)]
    public void RealExpressionsFollowAscendingCoefficientConvention(string expression, double x, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression, x), Is.EqualTo(expected).Within(1e-12));

    [TestCase("-a",-5)]
    [TestCase("3.0*a",15)]
    [TestCase("Polynomial.Add(a,3.0)",8)]
    [TestCase("Polynomial.Add(a,b)",6)]
    [TestCase("Polynomial.Divide(a,2.0)",2.5d)]
    [TestCase("Polynomial.Multiply(a,3.0)",15)]
    [TestCase("Polynomial.Multiply(a,b)",5)]
    [TestCase("Polynomial.Negate(a)",-5)]
    [TestCase("Polynomial.PointwiseDivide(a,b)",-3.6666666666666665d)]
    [TestCase("Polynomial.PointwiseMultiply(a,b)",-1)]
    [TestCase("Polynomial.Subtract(3.0,a)",-2)]
    [TestCase("Polynomial.Subtract(a,3.0)",2)]
    [TestCase("Polynomial.Subtract(a,b)",4)]
    [TestCase("a*b",5)]
    [TestCase("a+b",6)]
    [TestCase("a-b",4)]
    [TestCase("a/2.0",2.5d)]
    public void ArithmeticIsAvailableInScriptsAndPreservesInputs(string expression, double expected)
    {
        var result = Numbers("var a=new Polynomial(1,2); var b=new Polynomial(3,-1); var result="+expression+
            "; write(result.Evaluate(2)); write(\";\"); write(a.Evaluate(2)); write(\";\"); write(b.Evaluate(2));");
        NUnit.Framework.Assert.That(result, Is.EqualTo(new[] { expected, 5.0, 1.0 }).Within(1e-12));
    }

    [TestCase("new Polynomial(1,-3,2).Evaluate(z)")]
    [TestCase("Polynomial.Evaluate(z,1,-3,2)")]
    public void ComplexEvaluationMatchesTheAnalyticPolynomial(string expression)
    {
        var z = new Complex(2, 3);
        // 1-3(2+3i)+2(2+3i)^2 = -15+15i.
        var result = new PortableEngine().EvaluateComplex(expression, z);
        NUnit.Framework.Assert.That(result, Is.EqualTo(new Complex(-15, 15)));
    }

    [Test]
    public void StaticEvaluationAlsoAcceptsComplexCoefficients()
    {
        var result = new PortableEngine().EvaluateComplex(
            "Polynomial.Evaluate(z, new complex[] { new complex(1,2), new complex(3,-1) })", new Complex(2,3));
        NUnit.Framework.Assert.That(result, Is.EqualTo(new Complex(10,9)));
    }

    [Test]
    public void DivisionReturnsQuotientAndRemainder()
    {
        var result = Numbers("var a=new Polynomial(1,0,1); var b=new Polynomial(1,1); var result=Polynomial.DivideRemainder(a,b); "+
            "write(result.Item1.Evaluate(3)); write(\";\"); write(result.Item2.Evaluate(3)); write(\";\"); "+
            "write((result.Item1*b+result.Item2).Evaluate(3)); write(\";\"); write(a.Evaluate(3));");
        NUnit.Framework.Assert.That(result, Is.EqualTo(new[] {2.0,2.0,10.0,10.0}));
    }

    [Test]
    public void GeneralCubicRootsAreNotAssumedToBeSorted()
    {
        var roots = Roots("new Polynomial(-6,11,-6,1)").OrderBy(root => root.Real).ToArray();
        NUnit.Framework.Assert.That(roots, Has.Length.EqualTo(3));
        for (var i=0; i<3; i++) NUnit.Framework.Assert.That(Complex.Abs(roots[i]-(i+1)), Is.LessThan(1e-12));
    }

    [Test]
    public void GeneralRootsIncludeNonRealSolutions()
    {
        var roots = Roots("new Polynomial(1,0,0,0,1)");
        var h = Math.Sqrt(0.5);
        var expected = new[] { new Complex(h,h), new Complex(h,-h), new Complex(-h,h), new Complex(-h,-h) };
        NUnit.Framework.Assert.That(roots, Has.Length.EqualTo(4));
        foreach(var root in expected) NUnit.Framework.Assert.That(roots.Min(actual => Complex.Abs(actual-root)), Is.LessThan(1e-12));
    }

    [Test]
    public void RepeatedRootsAreNumericalApproximations()
    {
        var roots=Roots("new Polynomial(1,-2,1)");
        NUnit.Framework.Assert.That(roots, Has.Length.EqualTo(2));
        foreach(var root in roots) NUnit.Framework.Assert.That(Complex.Abs(root-1), Is.LessThan(1e-7));
    }

    [TestCase("Polynomial.Zero")]
    [TestCase("new Polynomial(0,0,0)")]
    [TestCase("new Polynomial(7.0)")]
    public void ConstantAndZeroPolynomialRootsRetainTheDocumentedEmptyConvention(string expression) =>
        NUnit.Framework.Assert.That(Roots(expression), Is.Empty);

    [Test]
    public void TrailingZeroCoefficientsDoNotInventRoots() =>
        NUnit.Framework.Assert.That(Roots("new Polynomial(-2,1,0,0)"), Is.EqualTo(new[] { new Complex(2,0) }));

    [Test]
    public void FittingRecoversAnExactQuadratic()
    {
        var result=Numbers("var p=Polynomial.Fit(new real[]{-2,-1,0,1,2},new real[]{15,6,1,0,3},2); "+
            "write(p.Coefficients[0]); write(\";\"); write(p.Coefficients[1]); write(\";\"); write(p.Coefficients[2]);");
        NUnit.Framework.Assert.That(result, Is.EqualTo(new[] {1.0,-3.0,2.0}).Within(1e-12));
    }

    [Test]
    public void ArrayConstructorRetainsItsDocumentedSharedStorage()
    {
        var result=Numbers("var c=new real[]{1,2}; var p=new Polynomial(c); c[0]=9; write(p.Evaluate(0)); "+
            "write(\";\"); p.Coefficients[1]=7; write(c[1]);");
        NUnit.Framework.Assert.That(result, Is.EqualTo(new[] {9.0,7.0}));
    }

    [Test]
    public void IntegerConstructorMeansCapacityWhileRealConstructorMeansConstant() =>
        NUnit.Framework.Assert.That(Numbers("write(new Polynomial(3).Evaluate(0)); write(\";\"); write(new Polynomial(3.0).Evaluate(0));"),
            Is.EqualTo(new[] {0.0,3.0}));

    [Test]
    public void AliasIsAvailableInCustomFunctionDefinitions() =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("p(x)", 2, customFunctions:
            "static real p(real t) => new Polynomial(1,-3,2).Evaluate(t);"), Is.EqualTo(3.0));

    [Test]
    public void DividingByZeroPolynomialReportsTheLibraryError() =>
        NUnit.Framework.Assert.Throws<DivideByZeroException>(() => new PortableEngine().ExecuteScript(
            "var result=Polynomial.DivideRemainder(new Polynomial(1,2),Polynomial.Zero); write(result.Item1);"));

    [Test]
    public void NullEvaluationCoefficientsReportTheLibraryError() =>
        NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new PortableEngine().EvaluateReal("Polynomial.Evaluate(x,(real[])null)"));

    private static double[] Numbers(string script) => new PortableEngine().ExecuteScript(script)
        .Split(';', StringSplitOptions.RemoveEmptyEntries).Select(value => double.Parse(value, CultureInfo.InvariantCulture)).ToArray();

    private static Complex[] Roots(string polynomial)
    {
        var values=Numbers("var roots="+polynomial+".Roots(); foreach(var root in roots) { write(root.Real); write(\";\"); write(root.Imaginary); write(\";\"); }");
        var result=new Complex[values.Length/2];
        for(var i=0; i<result.Length; i++) result[i]=new Complex(values[i*2],values[i*2+1]);
        return result;
    }
}
