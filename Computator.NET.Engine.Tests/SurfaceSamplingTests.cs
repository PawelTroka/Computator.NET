using Computator.NET.Engine;
using Computator.NET.Engine.Charting;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class SurfaceSamplingTests
{
    [Test]
    public void RealTslParaboloidHasExactRowMajorSamples()
    {
        var grid = new PortableEngine().SampleSurface("x²+y²", -2, 2, -2, 2, 9);
        NUnit.Framework.Assert.That(grid.Kind, Is.EqualTo("surface"));
        NUnit.Framework.Assert.That(grid.X, Has.Length.EqualTo(9));
        NUnit.Framework.Assert.That(grid.Y, Has.Length.EqualTo(9));
        NUnit.Framework.Assert.That(grid.Z, Has.Length.EqualTo(9));
        NUnit.Framework.Assert.That(grid.Z.All(row => row.Length == 9), Is.True);
        NUnit.Framework.Assert.That(grid.Z[0][0], Is.EqualTo(8));
        NUnit.Framework.Assert.That(grid.Z[4][4], Is.EqualTo(0));
        NUnit.Framework.Assert.That(grid.Z[8][8], Is.EqualTo(8));
        NUnit.Framework.Assert.That(grid.FiniteSamples, Is.EqualTo(81));
        NUnit.Framework.Assert.That(grid.MissingSamples, Is.Zero);
    }

    [Test]
    public void TwoVariableCustomFunctionsAreCompiledOncePerSurface()
    {
        const string functions = "static real Probe(real x, real y) { return x-y+7; }";
        var grid = new PortableEngine().SampleSurface("Probe(x,y)", -2, 2, -3, 3, 9, functions);
        NUnit.Framework.Assert.That(grid.Z[0][0], Is.EqualTo(8));
        NUnit.Framework.Assert.That(grid.Z[0][8], Is.EqualTo(12));
        NUnit.Framework.Assert.That(grid.Z[8][0], Is.EqualTo(2));
    }

    [Test]
    public void UndefinedValuesAreNullAndArithmeticErrorsRemainHoles()
    {
        var grid = SurfaceSampler.Sample((x, y) =>
            x < 0 ? throw new DivideByZeroException() : x == 0 && y == 0 ? double.NaN : x + y,
            -2, 2, -2, 2, 9);
        NUnit.Framework.Assert.That(grid.Z[0][0], Is.Null);
        NUnit.Framework.Assert.That(grid.Z[4][4], Is.Null);
        NUnit.Framework.Assert.That(grid.Z[8][8], Is.EqualTo(4));
        NUnit.Framework.Assert.That(grid.FiniteSamples + grid.MissingSamples, Is.EqualTo(81));
    }

    [Test]
    public void NonArithmeticEvaluationErrorsAreNotDisguisedAsMissingPoints()
    {
        NUnit.Framework.Assert.That(() => SurfaceSampler.Sample((x, y) =>
            throw new InvalidOperationException("broken definition"), -1, 1, -1, 1, 9),
            Throws.TypeOf<InvalidOperationException>());
    }

    [TestCase(8)]
    [TestCase(130)]
    public void InvalidGridSizesFailBeforeExecutingAnyUserCode(int gridSize)
    {
        var called = false;
        NUnit.Framework.Assert.That(() => SurfaceSampler.Sample((x, y) =>
        {
            called = true;
            return 0;
        }, -1, 1, -1, 1, gridSize), Throws.TypeOf<ArgumentOutOfRangeException>());
        NUnit.Framework.Assert.That(called, Is.False);
    }

    [Test]
    public void CloselySpacedBoundsCannotCollapseToDuplicateAxisCoordinates()
    {
        NUnit.Framework.Assert.That(() =>
            SurfaceSampler.Sample((x, y) => x + y, 1e16, 1e16 + 32, -1, 1, 129),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void CancellationStopsSamplingBetweenPoints()
    {
        using var cancellation = new CancellationTokenSource();
        var samples = 0;
        NUnit.Framework.Assert.That(() => SurfaceSampler.Sample((x, y) =>
        {
            if (++samples == 10) cancellation.Cancel();
            return x + y;
        }, -1, 1, -1, 1, 9, cancellation.Token), Throws.TypeOf<OperationCanceledException>());
        NUnit.Framework.Assert.That(samples, Is.EqualTo(10));
    }

    [Test]
    public void ImplicitEquationsDoNotBecomeAssignmentSurfaces()
    {
        NUnit.Framework.Assert.That(() => new PortableEngine().SampleSurface("x²+y²=1", -1, 1, -1, 1, 9),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("implicit"));
    }

    [Test]
    public void LargestSupportedGridReturnsBoundedSamples()
    {
        var grid = SurfaceSampler.Sample((x, y) => x + y, -5, 5, -5, 5, 129);
        NUnit.Framework.Assert.That(grid.FiniteSamples, Is.EqualTo(129 * 129));
        NUnit.Framework.Assert.That(grid.Z[0][0], Is.EqualTo(-10));
        NUnit.Framework.Assert.That(grid.Z[128][128], Is.EqualTo(10));
    }
}
