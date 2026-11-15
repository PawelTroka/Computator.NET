using System.Runtime.Loader;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class FivePointDerivativeArithmeticTests(bool generatedSource)
{
    private delegate double Difference(Func<double, double> function, double x, uint order, double eps);
    private Difference _differentiate = null!;
    private AssemblyLoadContext? _generatedContext;

    [OneTimeSetUp]
    public void LoadTheHostOrCompleteEmittedImplementation()
    {
        if (!generatedSource)
        {
            _differentiate = Derivative.centeredFivePointMethod;
            return;
        }

        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Append(typeof(MathNet.Numerics.Differentiate).Assembly.Location).Distinct();
        var compilation = CSharpCompilation.Create("FivePointDerivative.Generated.Tests",
            [CSharpSyntaxTree.ParseText(Derivative.ToCode)],
            paths.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
        image.Position = 0;
        _generatedContext = new AssemblyLoadContext("FivePointDerivative.Generated.Tests", isCollectible: true);
        var type = _generatedContext.LoadFromStream(image).GetType("Derivative", throwOnError: true)!;
        _differentiate = type.GetMethod(nameof(Derivative.centeredFivePointMethod))!.CreateDelegate<Difference>();
    }

    [OneTimeTearDown]
    public void UnloadGeneratedCode()
    {
        _differentiate = null!;
        _generatedContext?.Unload();
    }

    [TestCase(1e307)]
    [TestCase(1e308)]
    [TestCase(-1e308)]
    [TestCase(double.MaxValue)]
    public void LargeFiniteConstantsHaveZeroDerivative(double value)
    {
        NUnit.Framework.Assert.That(_differentiate(_ => value, 0, 1, 0.125), Is.Zero);
    }

    [TestCase(1e154)]
    [TestCase(-1e154)]
    public void LargeQuadraticValuesRetainTheirFiniteSlope(double x)
    {
        var slope = _differentiate(t => t * t, x, 1, 0.125);
        NUnit.Framework.Assert.That(slope / x, Is.EqualTo(2.0).Within(1e-13));
    }

    [TestCase(2e307)]
    [TestCase(5e307)]
    public void LinearSlopeSurvivesOverflowingWeightedDifferencesAndDenominator(double step)
    {
        // All four coordinates and samples are finite, while 12*step is not.
        NUnit.Framework.Assert.That(_differentiate(t => t, 0, 1, step), Is.EqualTo(1.0).Within(1e-15));
    }

    [Test]
    public void AFiniteSmallSlopeSurvivesAnOverflowingDenominator()
    {
        var slope = _differentiate(t => t / 1e308, 0, 1, 2e307);
        NUnit.Framework.Assert.That(slope / 1e-308, Is.EqualTo(1.0).Within(1e-14));
    }

    [Test]
    public void CancellingLargeInnerSamplesDoNotEraseTheTinyOuterDifference()
    {
        const double step = 1e-9;
        // The inner samples cancel exactly. The remaining numerator is -2 ulps
        // at zero; scaling all samples down before cancellation would erase it.
        var slope = _differentiate(t => Math.Abs(t) == step ? 1e308 : Math.CopySign(double.Epsilon, t), 0, 1, step);
        var expected = -(double.Epsilon / step) / 6;
        NUnit.Framework.Assert.That(slope, Is.EqualTo(expected).Within(double.Epsilon));
        NUnit.Framework.Assert.That(slope, Is.LessThan(0.0));
    }

    [Test]
    public void ExactCubicCancellationStaysZeroWithASubnormalStep()
    {
        const double step = double.Epsilon;
        var scale = Math.ScaleB(1.0, 1020);
        // The cubic has zero slope at the origin. The finite samples are
        // [-7, 0, 2, 9]*scale; weighting and subtraction overflow before the
        // zero numerator can be recovered by rescaling.
        var slope = _differentiate(t => scale * (1 + Math.Pow(t / step, 3)), 0, 1, step);
        NUnit.Framework.Assert.That(slope, Is.Zero);
    }

    [Test]
    public void OrdinaryCubicCancellationRetainsItsSmallNonzeroSlope()
    {
        // The first cubic has zero slope at the origin. The second adds a
        // small derivative. Pairing the [24, 3, 0, 1e-20] samples too early
        // would erase the last sample and incorrectly report zero.
        var slope = _differentiate(t => (-1.5 * t - 1) * (t - 1) * (t - 2)
            + (1e-20 / 12) * (t + 2) * (t + 1) * (t - 1), 0, 1, 1);
        NUnit.Framework.Assert.That(slope / (-1e-20 / 12), Is.EqualTo(1.0).Within(1e-14));
    }

    [Test]
    public void RecursiveConstantDerivativesUseTheSameFiniteArithmetic()
    {
        NUnit.Framework.Assert.That(_differentiate(_ => 1e308, 2, 3, 0.125), Is.Zero);
    }

    [Test]
    public void TheFallbackDoesNotReevaluateOrReorderTheCallable()
    {
        var visited = new List<double>();
        NUnit.Framework.Assert.That(_differentiate(t => { visited.Add(t); return 1e308; }, -2, 1, 0.125), Is.Zero);
        NUnit.Framework.Assert.That(visited, Is.EqualTo(new[] { -1.5, -1.75, -2.25, -2.5 }));
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void NonfiniteSamplesRemainNonfinite(double value)
    {
        NUnit.Framework.Assert.That(_differentiate(_ => value, 0, 1, 0.125), Is.NaN);
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void NonfiniteStepsKeepTheExistingArithmetic(double step)
    {
        NUnit.Framework.Assert.That(_differentiate(_ => 1e308, 0, 1, step), Is.NaN);
    }

    [Test]
    public void ATrulyUnrepresentableSlopeStillOverflows()
    {
        NUnit.Framework.Assert.That(_differentiate(t => 1e308 * t / 0.01, 0, 1, 0.001), Is.EqualTo(double.PositiveInfinity));
    }

    [Test]
    public void TheNamedMethodRetainsTheFiniteResultThroughActualTslEvaluation()
    {
        var actual = new PortableEngine().EvaluateReal(
            "Derivative.derivative(\"centered five-point method\", t => 1e308, 0.0)",
            customFunctions: generatedSource ? Derivative.ToCode : string.Empty);
        NUnit.Framework.Assert.That(actual, Is.Zero);
    }
}
