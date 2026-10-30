using System.Globalization;
using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

// Evidence-only fixture for the seven-PR integration candidate. This file is
// deliberately outside every source PR and the candidate's committed tree.
[TestFixture(false)]
[TestFixture(true)]
[NonParallelizable]
public class SevenPrCrossFeatureTests(bool emittedNumerics)
{
    private string Definitions => emittedNumerics ? FunctionRoot.ToCode + "\n" + Integral.ToCode : "";

    public static IEnumerable<TestCaseData> CombinedExpressions()
    {
        yield return Case("LeadingDotPowerAndEuclidInsideSecant",
            "FunctionRoot.secantMethod(t => Euclid.GreatestCommonDivisor(12,18)*(t-.125²),0,1,0,3)",
            0.015625, 0);
        yield return Case("ScientificPowerInsideSecant",
            "FunctionRoot.secantMethod(t => t-2e-3²,0,1,1e-19,8)", 0.000004, 4e-20);
        yield return Case("SeparatedNumericPowerInsideSecant",
            "FunctionRoot.secantMethod(t => t-1_024²,512000,2000000,0,3)", 1048576, 0);
        yield return Case("LargeScientificPowerRemainsAFiniteRoot",
            "FunctionRoot.secantMethod(t => t-1e100²,.5e200,2e200,0,8)", 1e200, 4e185);
        yield return Case("TinyScientificPowerSurvivesExtremeGuesses",
            "FunctionRoot.secantMethod(t => Euclid.GreatestCommonDivisor(12,18)*(t-1e-100²),0,1e200,0,8)",
            1e-200, 4e-215);
        yield return Case("DigitSuffixedLambdaNameKeepsNonlinearRoot",
            "FunctionRoot.secantMethod(value2 => value2²-.5²,.125,1,1e-13,100)", 0.5, 2e-13);
        yield return Case("EuclidInt64PowerAndPostfixPowerPermitExtrapolation",
            "FunctionRoot.secantMethod(t => t/Euclid.PowerOfTwo((integer)40)-.5²,-1099511627776,-549755813888,0,4)",
            274877906944, 0);
        yield return Case("EuclidSetsLinearIntegralCoefficientBoundAndPanels",
            "Integral.trapezoidalMethod(t => Euclid.GreatestCommonDivisor(12,18)*t+.5²,0,Euclid.GreatestCommonDivisor(4,6),Euclid.CeilingToPowerOfTwo(3))",
            12.5, 0);
        yield return Case("ScientificLiteralPowerIntegratesAsAConstant",
            "Integral.trapezoidalMethod(t => 2e-3²,0,Euclid.GreatestCommonDivisor(8,12),4)", 0.000016, 8e-20);
        yield return Case("ScientificPowerUsesSafeOverflowingEndpointAverage",
            "Integral.trapezoidalMethod(t => 1e154²,0,.5²,Euclid.Log2(16))", 2.5e307, 1e293);
        yield return Case("EuclidSuppliesOddSubnormalWidthToFiniteEndpointSum",
            "Integral.trapezoidalMethod(t => System.Math.ScaleB(1.0,1022)*1e0²,0,System.Double.Epsilon*Euclid.Modulus(-7,5),1)",
            3 * Math.ScaleB(1, -52), 0);
        yield return Case("SecantFindsKnownRootOfIntegratedLinearFunction",
            "FunctionRoot.secantMethod(t => Integral.trapezoidalMethod(u => .5²*u+Euclid.GreatestCommonDivisor(12,18),0,t,Euclid.PowerOfTwo(3))-12.5,1,3,1e-12,100)",
            2, 3e-13);
    }

    private static TestCaseData Case(string name, string expression, double expected, double tolerance) =>
        new TestCaseData(expression, expected, tolerance).SetName(name);

    [TestCaseSource(nameof(CombinedExpressions))]
    public void CompiledCombinedExpressionHasItsIndependentKnownValue(string expression, double expected, double tolerance)
    {
        // Constants/linear antiderivatives, exact binary powers, or the
        // independently solved polynomial 0.125*t²+6*t=12.5 supply the oracle.
        var actual = new PortableEngine().EvaluateReal(expression, customFunctions: Definitions);
        NUnit.Framework.Assert.That(double.IsFinite(actual), Is.True, expression);
        NUnit.Framework.Assert.That(actual, Is.EqualTo(expected).Within(tolerance), expression);
    }

    [Test]
    public void ScriptCombinesOutArgumentsDigitSuffixedPowersRootAndIntegral()
    {
        const string script = """
            integer u,v;
            integer d=Euclid.ExtendedGreatestCommonDivisor(240,46,out u,out v);
            real value2=.5;
            real root=FunctionRoot.secantMethod(t => d*t-value2²,0,1,0,4);
            real area=Integral.trapezoidalMethod(t => Euclid.PowerOfTwo((integer)40)*root,0,.5²,4);
            write(root); write(":"); write(area); write(":"); write(240*u+46*v);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, customFunctions: Definitions),
            Is.EqualTo("0.125:34359738368:2"));
    }

    [TestCase("de-DE")]
    [TestCase("cs-CZ")]
    public void CombinedMathAndScriptFormattingStayCorrectUnderLocalizedCultures(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            const string script = """
                real root=FunctionRoot.secantMethod(t => t-.5²,0,1,0,4);
                write(root); write(":");
                write(Integral.trapezoidalMethod(t => Euclid.GreatestCommonDivisor(12,18)*t+.5²,0,2,4));
                """;
            NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, customFunctions: Definitions),
                Is.EqualTo("0.25:12.5"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.CurrentUICulture = previousUi;
        }
    }
}
