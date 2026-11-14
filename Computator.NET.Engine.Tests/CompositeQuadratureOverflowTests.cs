using System.Runtime.Loader;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class CompositeQuadratureOverflowTests(bool generatedSource)
{
    private delegate double Quadrature(Func<double, double> function, double a, double b, double panels);
    private readonly Dictionary<string, Quadrature> _methods = new();
    private AssemblyLoadContext? _context;

    [OneTimeSetUp]
    public void LoadTheHostOrCompleteEmittedImplementation()
    {
        var type = typeof(Integral);
        if (generatedSource)
        {
            var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Append(typeof(MathNet.Numerics.Integration.DoubleExponentialTransformation).Assembly.Location)
                .Append(typeof(Accord.Math.Integration.RombergMethod).Assembly.Location).Distinct();
            var compilation = CSharpCompilation.Create("CompositeQuadratureOverflow.Generated.Tests",
                [CSharpSyntaxTree.ParseText(Integral.ToCode)],
                paths.Select(path => MetadataReference.CreateFromFile(path)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var image = new MemoryStream();
            var result = compilation.Emit(image);
            NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
            image.Position = 0;
            _context = new AssemblyLoadContext("CompositeQuadratureOverflow.Generated.Tests", isCollectible: true);
            type = _context.LoadFromStream(image).GetType("Integral", throwOnError: true)!;
        }
        foreach (var name in new[] { nameof(Integral.rectangleMethod), nameof(Integral.simpsonMethod) })
            _methods.Add(name, type.GetMethod(name)!.CreateDelegate<Quadrature>());
    }

    [OneTimeTearDown]
    public void UnloadTheEmittedImplementation()
    {
        _methods.Clear();
        _context?.Unload();
    }

    [TestCase(nameof(Integral.rectangleMethod), 2)]
    [TestCase(nameof(Integral.rectangleMethod), 4)]
    [TestCase(nameof(Integral.rectangleMethod), 16)]
    [TestCase(nameof(Integral.simpsonMethod), 2)]
    [TestCase(nameof(Integral.simpsonMethod), 4)]
    [TestCase(nameof(Integral.simpsonMethod), 16)]
    public void FiniteConstantAreasSurviveOverflowingSampleSums(string method, int panels)
    {
        var constant = Math.ScaleB(1, 1023);
        foreach (var sign in new[] { -1.0, 1.0 })
        {
            NUnit.Framework.Assert.That(_methods[method](_ => sign * constant, 0, 1, panels),
                Is.EqualTo(sign * constant));
            NUnit.Framework.Assert.That(_methods[method](_ => sign * constant, 1, 0, panels),
                Is.EqualTo(-sign * constant));
        }
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void LargestFiniteConstantRetainsItsFiniteUnitIntervalArea(string method)
    {
        foreach (var sign in new[] { -1.0, 1.0 })
            NUnit.Framework.Assert.That(_methods[method](_ => sign * double.MaxValue, 0, 1, 2),
                Is.EqualTo(sign * double.MaxValue).Within(1).Ulps);
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void UnequalLargeSamplesRetainTheExactLinearArea(string method)
    {
        var scale = Math.ScaleB(1, 1023);
        NUnit.Framework.Assert.That(_methods[method](x => scale * (1 + x / 2), 0, 1, 4),
            Is.EqualTo(1.25 * scale));
        NUnit.Framework.Assert.That(_methods[method](x => -scale * (1 + x / 2), 1, 0, 4),
            Is.EqualTo(1.25 * scale));
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void LargePositiveAndNegativeContributionsCanCancel(string method)
    {
        var scale = Math.ScaleB(1, 1023);
        // The function is odd about the interval midpoint. Both rules have
        // symmetric nodes/weights, so their exact finite sums are zero.
        NUnit.Framework.Assert.That(_methods[method](x => x < 0.5 ? scale : x > 0.5 ? -scale : 0, 0, 1, 4),
            Is.Zero);
    }

    [Test]
    public void MidpointSumRestoresTinyContributionsAfterLargeValuesCancel()
    {
        var scale = Math.ScaleB(1, 1023);
        NUnit.Framework.Assert.That(_methods[nameof(Integral.rectangleMethod)](
            x => x < 2 ? scale : x < 4 ? -scale : double.Epsilon, 0, 5, 5),
            Is.EqualTo(double.Epsilon));
    }

    [Test]
    public void SimpsonSumRestoresATinyEndpointAfterLargeInteriorValuesCancel()
    {
        var scale = Math.ScaleB(1, 1023);
        NUnit.Framework.Assert.That(_methods[nameof(Integral.simpsonMethod)](
            x => x == 3 ? scale : x == 9 ? -scale : x == 12 ? double.Epsilon : 0, 0, 12, 4),
            Is.EqualTo(double.Epsilon));
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void TinySamplesSurviveMultiplicationByALargeInterval(string method)
    {
        NUnit.Framework.Assert.That(_methods[method](_ => double.Epsilon, 0, Math.ScaleB(1, 1023), 2),
            Is.EqualTo(Math.ScaleB(1, -51)));
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void ASubnormalPanelWidthStillContributesToALargeFiniteArea(string method)
    {
        var scale = Math.ScaleB(1, 1023);
        NUnit.Framework.Assert.That(_methods[method](_ => scale, 0, 2 * double.Epsilon, 2),
            Is.EqualTo(Math.ScaleB(1, -50)));
    }

    [Test]
    public void SimpsonAppliesItsDivisorBeforeAnOtherwiseOverflowingFinalProduct()
    {
        NUnit.Framework.Assert.That(_methods[nameof(Integral.simpsonMethod)](_ => 1, 0, 1e308, 2),
            Is.EqualTo(1e308).Within(1).Ulps);
        NUnit.Framework.Assert.That(_methods[nameof(Integral.simpsonMethod)](_ => 1, 1e308, 0, 2),
            Is.EqualTo(-1e308).Within(1).Ulps);
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void NonfiniteSamplesAndTrulyUnrepresentableAreasRemainNonfinite(string method)
    {
        var integrate = _methods[method];
        NUnit.Framework.Assert.That(integrate(_ => double.PositiveInfinity, 0, 1, 2), Is.EqualTo(double.PositiveInfinity));
        NUnit.Framework.Assert.That(integrate(_ => double.NegativeInfinity, 0, 1, 2), Is.EqualTo(double.NegativeInfinity));
        NUnit.Framework.Assert.That(integrate(_ => double.NaN, 0, 1, 2), Is.NaN);
        NUnit.Framework.Assert.That(integrate(x => x < 0.5 ? double.PositiveInfinity : double.NegativeInfinity,
            0, 1, 2), Is.NaN);
        NUnit.Framework.Assert.That(integrate(_ => double.MaxValue, 0, 2, 2), Is.EqualTo(double.PositiveInfinity));
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void TheFunctionIsSampledOnceAtEachOriginalNodeInTheOriginalOrder(string method)
    {
        var samples = new List<double>();
        _methods[method](x => { samples.Add(x); return Math.ScaleB(1, 1023); }, 0, 1, 4);
        NUnit.Framework.Assert.That(samples, Is.EqualTo(method == nameof(Integral.rectangleMethod)
            ? new[] { 0.125, 0.375, 0.625, 0.875 }
            : new[] { 0.25, 0.5, 0.75, 0.0, 1.0 }));
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void TslExpressionsUseTheFiniteResult(string method)
    {
        var result = new PortableEngine().EvaluateReal(
            $"Integral.{method}(t => 1e308, 0, 1, 2)/1e308",
            customFunctions: generatedSource ? Integral.ToCode : string.Empty);
        NUnit.Framework.Assert.That(result, Is.EqualTo(1).Within(2).Ulps);
    }

    [Test]
    public void PortableIntegrationUsesTheCorrectedSimpsonRule()
    {
        NUnit.Framework.Assert.That(new PortableEngine().Integrate("1e308", 0, 1, steps: 2),
            Is.EqualTo(1e308).Within(1).Ulps);
    }
}
