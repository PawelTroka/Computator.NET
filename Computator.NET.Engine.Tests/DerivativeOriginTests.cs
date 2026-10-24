using System.Runtime.Loader;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class DerivativeOriginTests(bool generatedSource)
{
    private delegate double Difference(Func<double, double> function, double x, uint order, double eps);
    private readonly Dictionary<string, Difference> _methods = new();
    private AssemblyLoadContext? _generatedContext;

    private static readonly string[] Methods =
    [
        nameof(Derivative.finiteDifferenceFormula),
        nameof(Derivative.stableFiniteDifferenceFormula),
        nameof(Derivative.twoPointfiniteDifferenceFormula),
        nameof(Derivative.centeredFivePointMethod)
    ];

    [OneTimeSetUp]
    public void LoadActualHostOrCompleteEmittedImplementation()
    {
        var type = typeof(Derivative);
        if (generatedSource)
        {
            var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Append(typeof(MathNet.Numerics.Differentiate).Assembly.Location).Distinct();
            var compilation = CSharpCompilation.Create("DerivativeOrigin.Generated.Tests",
                [CSharpSyntaxTree.ParseText(Derivative.ToCode)],
                paths.Select(path => MetadataReference.CreateFromFile(path)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var image = new MemoryStream();
            var result = compilation.Emit(image);
            NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
            image.Position = 0;
            _generatedContext = new AssemblyLoadContext("DerivativeOrigin.Generated.Tests", isCollectible: true);
            type = _generatedContext.LoadFromStream(image).GetType("Derivative", throwOnError: true)!;
        }

        foreach (var method in Methods)
            _methods.Add(method, type.GetMethod(method)!.CreateDelegate<Difference>());
    }

    [OneTimeTearDown]
    public void ReleaseGeneratedImplementation()
    {
        _methods.Clear();
        _generatedContext?.Unload();
    }

    [Test]
    public void DefaultStepRetainsTheExponentialSlopeNearZero(
        [ValueSource(nameof(Methods))] string method,
        [Values(0.0, 1e-10, -1e-10, double.Epsilon, -double.Epsilon)] double x)
    {
        var actual = _methods[method](Math.Exp, x, 1, 1e-9);

        // The forward rules have truncation error; the stable rule uses sqrt(eps).
        // This is a local error budget for exp, not an accuracy claim for any fx.
        var tolerance = method == nameof(Derivative.stableFiniteDifferenceFormula) ? 2e-5 : 5e-7;
        NUnit.Framework.Assert.That(actual, Is.EqualTo(Math.Exp(x)).Within(tolerance));
    }

    [Test]
    public void RecursiveDifferencesOfPolynomialsWorkAtAndAcrossTheOrigin(
        [ValueSource(nameof(Methods))] string method,
        [Values(0.0, 1e-10, -1e-10)] double x,
        [Values(2u, 3u)] uint order)
    {
        // A nonzero offset makes collapsed samples observable. Choose a useful
        // step for repeated subtraction instead of requesting the tiny default.
        Func<double, double> function = order == 2 ? t => 1 + t * t : t => 1 + t * t * t;
        var eps = method == nameof(Derivative.stableFiniteDifferenceFormula) ? 1e-4 : 1e-2;

        var actual = _methods[method](function, x, order, eps);

        NUnit.Framework.Assert.That(actual, Is.EqualTo(order == 2 ? 2.0 : 6.0).Within(1e-6));
    }

    [Test]
    public void RequestedStepUsesAnAbsoluteScaleNearZeroAndRelativeScaleFartherAway(
        [ValueSource(nameof(Methods))] string method,
        [Values(0.0, 0.25, -0.25, 2.0, -2.0)] double x)
    {
        var samples = new List<double>();
        var eps = method == nameof(Derivative.stableFiniteDifferenceFormula) ? 0.015625 : 0.125;
        _methods[method](sample => { samples.Add(sample); return sample * sample; }, x, 1, eps);

        var signedStep = 0.125 * Math.Max(1, Math.Abs(x)) * (x < 0 ? -1 : 1);
        var expected = method switch
        {
            nameof(Derivative.finiteDifferenceFormula) or nameof(Derivative.stableFiniteDifferenceFormula)
                => new[] { x, x + signedStep },
            nameof(Derivative.twoPointfiniteDifferenceFormula) => new[] { x - signedStep, x + signedStep },
            _ => new[] { x - 2 * signedStep, x - signedStep, x + signedStep, x + 2 * signedStep }
        };
        NUnit.Framework.Assert.That(samples, Is.EquivalentTo(expected));
    }

    [Test]
    public void OrderZeroEvaluatesOnlyTheRequestedPoint(
        [ValueSource(nameof(Methods))] string method, [Values(0.0, -1e-10)] double x)
    {
        var samples = new List<double>();
        var actual = _methods[method](sample => { samples.Add(sample); return 7.0; }, x, 0, 0.125);

        NUnit.Framework.Assert.That(actual, Is.EqualTo(7.0));
        NUnit.Framework.Assert.That(samples, Is.EqualTo(new[] { x }));
    }

    [Test]
    public void AnExplicitStepKeepsTheCenteredLogarithmStencilInsideItsSmallDomain()
    {
        const double x = 1e-10;
        // The absolute default step, 1e-9, would cross the boundary at zero.
        // Callers can choose a smaller eps for the function's domain and scale.
        var actual = _methods[nameof(Derivative.twoPointfiniteDifferenceFormula)](sample =>
        {
            NUnit.Framework.Assert.That(sample, Is.GreaterThan(0.0));
            return Math.Log(sample);
        }, x, 1, 1e-15);

        // d(log x)/dx = 1/x; this absolute bound is 2e-9 of the expected slope.
        NUnit.Framework.Assert.That(actual, Is.EqualTo(1.0 / x).Within(20.0));
    }

    [TestCase("finite difference formula")]
    [TestCase("stable finite difference formula")]
    [TestCase("two-point finite difference formula")]
    [TestCase("centered five-point method")]
    public void NamedMethodWorksThroughActualTslEvaluation(string method)
    {
        var actual = new PortableEngine().EvaluateReal(
            $"Derivative.derivative(\"{method}\", t => exp(t), 0.0)",
            customFunctions: generatedSource ? Derivative.ToCode : string.Empty);

        NUnit.Framework.Assert.That(actual, Is.EqualTo(1.0).Within(2e-5));
    }
}
