using Computator.NET.Engine.Charting;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class CurveSamplingTests
{
    [TestCase("x²", -2, 2)]
    [TestCase("sin(x)", -3, 3)]
    public void PortableExpressionsProduceFiniteOrderedSamples(string expression, double minimum, double maximum)
    {
        var curve = new PortableEngine().SampleCurve(expression, minimum, maximum, -5, 5, 65);
        NUnit.Framework.Assert.That(curve.Kind, Is.EqualTo("curve"));
        NUnit.Framework.Assert.That(curve.X.Length, Is.EqualTo(65));
        NUnit.Framework.Assert.That(curve.FiniteSamples, Is.EqualTo(65));
        NUnit.Framework.Assert.That(curve.MissingSamples, Is.Zero);
        NUnit.Framework.Assert.That(curve.X[0], Is.EqualTo(minimum));
        NUnit.Framework.Assert.That(curve.X[^1], Is.EqualTo(maximum));
        for (var index = 0; index < curve.X.Length; index++)
        {
            if (index > 0) NUnit.Framework.Assert.That(curve.X[index], Is.GreaterThan(curve.X[index - 1]));
            var expected = expression == "x²" ? curve.X[index] * curve.X[index] : Math.Sin(curve.X[index]);
            NUnit.Framework.Assert.That(curve.Y[index], Is.EqualTo(expected).Within(1e-12));
        }
    }

    [Test]
    public void OneCompiledDefinitionLivesForTheWholeCurveButNotTheNextRequest()
    {
        // A stateful custom function records its first argument. Recompiling at
        // each sample would reset it and incorrectly turn this line into zeros.
        const string definitions = "static real? origin; static real Probe(real x) { if (!origin.HasValue) origin=x; return x-origin.Value; }";
        var engine = new PortableEngine();
        foreach (var minimum in new[] { -2.0, 3.0 })
        {
            var curve = engine.SampleCurve("Probe(x)", minimum, minimum + 4, -5, 5, 33, definitions);
            for (var index = 0; index < curve.X.Length; index++)
                NUnit.Framework.Assert.That(curve.Y[index], Is.EqualTo(curve.X[index] - minimum).Within(1e-12));
        }
        NUnit.Framework.Assert.Throws<EngineCompilationException>(() => engine.SampleCurve("Probe(x)", -1, 1, -5, 5));
    }

    [TestCase("1/x")]
    [TestCase("1/(x-0.1)")]
    public void SingularitiesHaveGapsInsteadOfJoiningOppositeSides(string expression)
    {
        var curve = new PortableEngine().SampleCurve(expression, -1, 1, -5, 5, 33);
        var pole = expression == "1/x" ? 0 : 0.1;
        var gaps = curve.X.Where((x, index) => curve.Y[index] == null).ToArray();
        NUnit.Framework.Assert.That(gaps.Any(x => Math.Abs(x - pole) <= 1.0 / 32), Is.True);
        NUnit.Framework.Assert.That(curve.Y.Where(value => value.HasValue).All(value => double.IsFinite(value!.Value)), Is.True);
        NUnit.Framework.Assert.That(curve.MissingSamples, Is.EqualTo(gaps.Length));
    }

    [Test]
    public void SteepStraightLinesStayConnectedAndProbeWorkIsBounded()
    {
        var evaluations = 0;
        var curve = CurveSampler.Sample(x => { evaluations++; return 1e12 * x + 7; }, -1, 1, -5, 5, 65);
        NUnit.Framework.Assert.That(curve.X.Length, Is.EqualTo(65));
        NUnit.Framework.Assert.That(curve.MissingSamples, Is.Zero);
        NUnit.Framework.Assert.That(evaluations, Is.InRange(65, 65 + 8 * 64));
    }

    [Test]
    public void ArithmeticFailuresAndNonfiniteValuesAreHolesButOtherFailuresPropagate()
    {
        var arithmetic = CurveSampler.Sample(x => x == 0 ? throw new DivideByZeroException() : x,
            -1, 1, -5, 5, 33);
        NUnit.Framework.Assert.That(arithmetic.Y[Array.IndexOf(arithmetic.X, 0.0)], Is.Null);
        var undefined = new PortableEngine().SampleCurve("sqrt(-1)", -1, 1, -5, 5, 33);
        NUnit.Framework.Assert.That(undefined.FiniteSamples, Is.Zero);
        NUnit.Framework.Assert.That(undefined.MissingSamples, Is.EqualTo(33));
        NUnit.Framework.Assert.That(undefined.Y, Is.All.Null);
        var failure = NUnit.Framework.Assert.Throws<InvalidOperationException>(() =>
            CurveSampler.Sample(_ => throw new InvalidOperationException("Probe failure"), -1, 1, -5, 5, 33));
        NUnit.Framework.Assert.That(failure!.Message, Is.EqualTo("Probe failure"));
    }

    [TestCase(32)]
    [TestCase(4098)]
    [TestCase(int.MaxValue)]
    public void InvalidSampleBudgetsAreRejectedBeforeCompilation(int sampleCount) =>
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PortableEngine().SampleCurve("missing(", -1, 1, -5, 5, sampleCount));

    [TestCase(double.NaN, 1, -1, 1)]
    [TestCase(-1, double.PositiveInfinity, -1, 1)]
    [TestCase(1, 1, -1, 1)]
    [TestCase(1, -1, -1, 1)]
    [TestCase(-double.MaxValue, double.MaxValue, -1, 1)]
    [TestCase(-1, 1, double.NegativeInfinity, 1)]
    [TestCase(-1, 1, 1, 1)]
    [TestCase(-1, 1, 1, -1)]
    [TestCase(-1, 1, -double.MaxValue, double.MaxValue)]
    public void InvalidBoundsAreRejectedBeforeCompilation(double xMin, double xMax, double yMin, double yMax) =>
        NUnit.Framework.Assert.Throws<ArgumentException>(() =>
            new PortableEngine().SampleCurve("missing(", xMin, xMax, yMin, yMax));

    [Test]
    public void UnrepresentableCoordinatesFailBeforeCallingTheFunction()
    {
        var evaluations = 0;
        NUnit.Framework.Assert.Throws<ArgumentException>(() => CurveSampler.Sample(x => { evaluations++; return x; },
            1e16, 1e16 + 32, -5, 5, 65));
        NUnit.Framework.Assert.That(evaluations, Is.Zero);
    }

    [Test]
    public void CancellationIsObservedBeforeCompilationAndBetweenSamples()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        NUnit.Framework.Assert.Throws<OperationCanceledException>(() => new PortableEngine().SampleCurve(
            "missing(", -1, 1, -5, 5, cancellation: cancelled.Token));
        using var during = new CancellationTokenSource();
        var evaluations = 0;
        NUnit.Framework.Assert.Throws<OperationCanceledException>(() => CurveSampler.Sample(x =>
        {
            if (++evaluations == 7) during.Cancel();
            return x;
        }, -1, 1, -5, 5, 33, during.Token));
        NUnit.Framework.Assert.That(evaluations, Is.EqualTo(7));
    }

    [TestCase("z²")]
    [TestCase("x+y")]
    [TestCase("missing(x)")]
    public void UnsupportedOrInvalidExpressionsRetainCompilationDiagnostics(string expression)
    {
        var error = NUnit.Framework.Assert.Throws<EngineCompilationException>(() =>
            new PortableEngine().SampleCurve(expression, -1, 1, -5, 5, 33));
        NUnit.Framework.Assert.That(error!.Diagnostics.Any(item => item.File == "expression.tsl"), Is.True);
    }

    [TestCase("x=1")]
    [TestCase("((x=1))")]
    [TestCase("x²+y²=1")]
    [TestCase("x=x=1")]
    public void ImplicitEquationsAreRejectedInsteadOfBeingDrawnAsAssignments(string expression)
    {
        var error = NUnit.Framework.Assert.Throws<ArgumentException>(() =>
            new PortableEngine().SampleCurve(expression, -1, 1, -5, 5, 33));
        NUnit.Framework.Assert.That(error!.Message, Does.Contain("implicit equations are unavailable"));
    }

    [TestCase("x>=0 ? x : -x")]
    [TestCase("x==0 ? 0 : abs(x)")]
    [TestCase("Probe(value: x)")]
    public void ValidComparisonsAndNamedArgumentsAreNotMistakenForEquations(string expression)
    {
        var curve = new PortableEngine().SampleCurve(expression, -1, 1, -5, 5, 33,
            "static real Probe(real value) { return abs(value); }");
        for (var index = 0; index < curve.X.Length; index++)
            NUnit.Framework.Assert.That(curve.Y[index], Is.EqualTo(Math.Abs(curve.X[index])).Within(1e-12));
    }

    [Test]
    public void MaximumCurveRequestKeepsTheResponseWithinTheWorkerBudget()
    {
        var evaluations = 0;
        var curve = CurveSampler.Sample(x => { evaluations++; return Math.Sin(100000 * x); },
            -1, 1, -1, 1, CurveSampler.MaximumSamples);
        NUnit.Framework.Assert.That(evaluations, Is.LessThanOrEqualTo(4097 + 8 * 4096));
        NUnit.Framework.Assert.That(curve.X.Length, Is.InRange(4097, 8193));
        var json = System.Text.Json.JsonSerializer.Serialize(curve);
        NUnit.Framework.Assert.That(json.Length, Is.LessThan(1048576));
    }
}
