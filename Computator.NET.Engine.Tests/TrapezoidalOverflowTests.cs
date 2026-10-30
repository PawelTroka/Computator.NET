using System.Runtime.Loader;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class TrapezoidalOverflowTests(bool generatedSource)
{
    private delegate double Quadrature(Func<double, double> function, double a, double b, double panels);
    private Quadrature _integrate = null!;
    private AssemblyLoadContext? _context;

    [OneTimeSetUp]
    public void LoadTheHostOrActualEmittedImplementation()
    {
        _integrate = Integral.trapezoidalMethod;
        if (!generatedSource) return;

        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Append(typeof(MathNet.Numerics.Integration.DoubleExponentialTransformation).Assembly.Location)
            .Append(typeof(Accord.Math.Integration.RombergMethod).Assembly.Location).Distinct();
        var compilation = CSharpCompilation.Create("TrapezoidalOverflow.Generated.Tests",
            [CSharpSyntaxTree.ParseText(Integral.ToCode)],
            paths.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
        image.Position = 0;
        _context = new AssemblyLoadContext("TrapezoidalOverflow.Generated.Tests", isCollectible: true);
        var type = _context.LoadFromStream(image).GetType("Integral", throwOnError: true)!;
        _integrate = type.GetMethod(nameof(Integral.trapezoidalMethod))!.CreateDelegate<Quadrature>();
    }

    [OneTimeTearDown]
    public void UnloadTheEmittedImplementation()
    {
        _integrate = null!;
        _context?.Unload();
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(16)]
    public void FiniteConstantIntegralsSurviveAnOverflowingEndpointSum(int panels)
    {
        // These binary powers make the exact constant-function integrals
        // independently known without an expected-value quadrature formula.
        var constant = Math.ScaleB(1, 1023);
        foreach (var sign in new[] { -1.0, 1.0 })
        {
            NUnit.Framework.Assert.That(_integrate(_ => sign * constant, 0, 1, panels),
                Is.EqualTo(sign * constant));
            NUnit.Framework.Assert.That(_integrate(_ => sign * constant, 1, 0, panels),
                Is.EqualTo(-sign * constant));
        }
    }

    [Test]
    public void UnequalFiniteEndpointsRetainTheExactLinearIntegral()
    {
        var scale = Math.ScaleB(1, 1023);
        // Integrating scale*(1+t/2) on [0,1] gives 5*scale/4.
        NUnit.Framework.Assert.That(_integrate(t => scale * (1 + t / 2), 0, 1, 4),
            Is.EqualTo(1.25 * scale));
        NUnit.Framework.Assert.That(_integrate(t => -scale * (1 + t / 2), 1, 0, 4),
            Is.EqualTo(1.25 * scale));
    }

    [Test]
    public void LargestFiniteConstantStillHasAFiniteUnitIntervalIntegral()
    {
        NUnit.Framework.Assert.That(_integrate(_ => double.MaxValue, 0, 1, 1), Is.EqualTo(double.MaxValue));
        NUnit.Framework.Assert.That(_integrate(_ => -double.MaxValue, 0, 1, 1), Is.EqualTo(-double.MaxValue));
    }

    [Test]
    public void ASubnormalPanelWidthIsAppliedAfterAveragingLargeValues()
    {
        // 2^1023 * 2^-1074 = 2^-51. Halving the panel width first
        // would round it to zero before the large values can contribute.
        var constant = Math.ScaleB(1, 1023);
        NUnit.Framework.Assert.That(_integrate(_ => constant, 0, double.Epsilon, 1),
            Is.EqualTo(Math.ScaleB(1, -51)));
    }

    [Test]
    public void TinyEndpointValuesAreNotHalvedBeforeAddingThem()
    {
        NUnit.Framework.Assert.That(_integrate(_ => double.Epsilon, 0, 1, 1), Is.EqualTo(double.Epsilon));
        // The ordinary path must retain a tiny sample until multiplication
        // by a large panel width, rather than rounding its mean to zero.
        var width = Math.ScaleB(1, 1023);
        NUnit.Framework.Assert.That(_integrate(t => t == 0 ? 0 : double.Epsilon, 0, width, 1),
            Is.EqualTo(Math.ScaleB(1, -52)));
    }

    [Test]
    public void OppositeFiniteExtremesCanStillCancel()
    {
        NUnit.Framework.Assert.That(_integrate(t => t == 0 ? -double.MaxValue : double.MaxValue, 0, 1, 1),
            Is.EqualTo(0));
    }

    [Test]
    public void ActualNonfiniteSamplesAndUnrepresentableAreasRemainNonfinite()
    {
        NUnit.Framework.Assert.That(_integrate(_ => double.PositiveInfinity, 0, 1, 1),
            Is.EqualTo(double.PositiveInfinity));
        NUnit.Framework.Assert.That(_integrate(_ => double.NegativeInfinity, 0, 1, 1),
            Is.EqualTo(double.NegativeInfinity));
        NUnit.Framework.Assert.That(_integrate(_ => double.NaN, 0, 1, 1), Is.NaN);
        NUnit.Framework.Assert.That(_integrate(t => t == 0 ? double.NegativeInfinity : double.PositiveInfinity, 0, 1, 1), Is.NaN);
        NUnit.Framework.Assert.That(_integrate(_ => double.MaxValue, 0, 2, 1),
            Is.EqualTo(double.PositiveInfinity));
    }

    [Test]
    public void SamplingOrderAndCountStayTheSame()
    {
        var samples = new List<double>();
        _integrate(t => { samples.Add(t); return Math.ScaleB(1, 1023); }, 0, 1, 2);
        NUnit.Framework.Assert.That(samples, Is.EqualTo(new[] { 0.5, 0.0, 1.0, 0.5 }));
    }

    [Test]
    public void TheTslPipelineUsesTheCorrectedImplementation()
    {
        var result = new PortableEngine().EvaluateReal(
            "Integral.trapezoidalMethod(t => System.Math.ScaleB(1.0,1023), 0, 1, 2)/System.Math.ScaleB(1.0,1023)",
            customFunctions: generatedSource ? Integral.ToCode : string.Empty);
        NUnit.Framework.Assert.That(result, Is.EqualTo(1));
    }
}
