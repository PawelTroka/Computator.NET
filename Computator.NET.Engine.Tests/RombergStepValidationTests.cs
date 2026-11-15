using Computator.NET.Core.NumericalCalculations;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class RombergStepValidationTests
{
    [TestCase(0.0)]
    [TestCase(-1.0)]
    [TestCase(0.5)]
    [TestCase(2.5)]
    [TestCase(30.5)]
    [TestCase(31.0)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void HostRejectsInvalidCountsWithoutSampling(double count)
    {
        var evaluations = 0;
        var exception = NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            Integral.rombergMethod(_ =>
            {
                evaluations++;
                throw new InvalidOperationException("Invalid counts must not sample the function.");
            }, 0, 1, count));

        NUnit.Framework.Assert.That(exception!.ParamName, Is.EqualTo("N"));
        NUnit.Framework.Assert.That(evaluations, Is.Zero);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(31)]
    public void HostAndGeneratedTslRejectInvalidIntegerCounts(int count)
    {
        var expression = $"Integral.rombergMethod(t => t, 0, 1, {count})";
        foreach (var generated in new[] { false, true })
        {
            var exception = NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PortableEngine().EvaluateReal(expression,
                    customFunctions: generated ? Integral.ToCode : string.Empty));
            NUnit.Framework.Assert.That(exception!.ParamName, Is.EqualTo("N"),
                generated ? "Generated TSL implementation" : "Host implementation");
        }
    }

    [TestCase(2)]
    [TestCase(4)]
    public void ValidCountsRetainTheExistingRombergResults(int count)
    {
        var result = Integral.rombergMethod(x => x, 0, 1, count);
        NUnit.Framework.Assert.That(result, Is.EqualTo(0.5).Within(1e-12));
    }
}
