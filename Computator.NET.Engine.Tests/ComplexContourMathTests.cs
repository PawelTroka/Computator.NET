using Computator.NET.Charting.ComplexCharting;
using Computator.NET.DataTypes.Charts;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ComplexContourMathTests
{
    [TestCase(CountourLinesMode.Logarithmic, 0.25, 2.0, 0.25)]
    [TestCase(CountourLinesMode.Logarithmic, 1.5, 2.0, 0.5)]
    [TestCase(CountourLinesMode.Logarithmic, 2.0, 2.0, 1.0)]
    [TestCase(CountourLinesMode.Linear, 0.25, 2.0, 0.125)]
    [TestCase(CountourLinesMode.Linear, 5.0, 2.0, 0.5)]
    [TestCase(CountourLinesMode.Linear, 2.0, 2.0, 1.0)]
    [TestCase(CountourLinesMode.Exponential, 0.5, 2.718281828459045, 0.36067376022224085)]
    public void ValidBandsHaveTheExpectedFraction(CountourLinesMode mode, double magnitude,
        double step, double expected)
    {
        NUnit.Framework.Assert.That(ComplexContourMath.TryGetPosition(magnitude, step, mode, out var fraction),
            Is.True);
        NUnit.Framework.Assert.That(fraction, Is.EqualTo(expected).Within(1e-10));
    }

    [TestCase(CountourLinesMode.Exponential, 1.0, 2.718281828459045)]
    [TestCase(CountourLinesMode.Exponential, 0.0, 2.718281828459045)]
    [TestCase(CountourLinesMode.Logarithmic, 10.0, 1.0)]
    [TestCase(CountourLinesMode.Logarithmic, 10.0, 0.5)]
    [TestCase(CountourLinesMode.Linear, 10.0, 0.0)]
    [TestCase(CountourLinesMode.Linear, 10.0, -0.1)]
    [TestCase(CountourLinesMode.Exponential, 2.0, 1.0)]
    [TestCase(CountourLinesMode.NoCountourLines, 2.0, 2.0)]
    [TestCase(CountourLinesMode.Linear, double.NaN, 2.0)]
    [TestCase(CountourLinesMode.Linear, double.PositiveInfinity, 2.0)]
    [TestCase(CountourLinesMode.Linear, 2.0, double.NaN)]
    [TestCase(CountourLinesMode.Linear, 2.0, double.PositiveInfinity)]
    [TestCase(CountourLinesMode.Linear, 1e100, 1e-100)]
    public void DegenerateInputsUseSafeSmoothColoring(CountourLinesMode mode,
        double magnitude, double step)
    {
        NUnit.Framework.Assert.That(ComplexContourMath.TryGetPosition(magnitude, step, mode, out _),
            Is.False);
    }

    [TestCase(CountourLinesMode.Exponential, 0.25, 2.718281828459045)]
    [TestCase(CountourLinesMode.Exponential, 0.5, 2.718281828459045)]
    [TestCase(CountourLinesMode.Exponential, 2.0, 2.718281828459045)]
    [TestCase(CountourLinesMode.Linear, 1e12, 0.000001)]
    [TestCase(CountourLinesMode.Logarithmic, 1e200, 1.000000001)]
    public void NonDegenerateExtremeInputsCompleteWithoutIteration(CountourLinesMode mode,
        double magnitude, double step)
    {
        if (ComplexContourMath.TryGetPosition(magnitude, step, mode, out var fraction))
            NUnit.Framework.Assert.That(fraction, Is.InRange(0.0, 1.0));
    }
}
