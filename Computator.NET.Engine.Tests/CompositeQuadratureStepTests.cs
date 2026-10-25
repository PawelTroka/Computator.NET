using System.Runtime.Loader;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class CompositeQuadratureStepTests(bool generatedSource)
{
    private delegate double Quadrature(Func<double, double> function, double a, double b, double steps);

    private readonly Dictionary<string, Quadrature> _methods = new();
    private AssemblyLoadContext? _context;

    private static readonly string[] MethodNames =
    [
        nameof(Integral.rectangleMethod),
        nameof(Integral.trapezoidalMethod),
        nameof(Integral.simpsonMethod)
    ];

    [OneTimeSetUp]
    public void LoadTheHostOrExactGeneratedImplementation()
    {
        var type = typeof(Integral);
        if (generatedSource)
        {
            var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Append(typeof(MathNet.Numerics.Integration.DoubleExponentialTransformation).Assembly.Location)
                .Append(typeof(Accord.Math.Integration.RombergMethod).Assembly.Location).Distinct();
            var compilation = CSharpCompilation.Create("Integral.Steps.Generated.Tests",
                [CSharpSyntaxTree.ParseText(Integral.ToCode)],
                paths.Select(path => MetadataReference.CreateFromFile(path)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var image = new MemoryStream();
            var result = compilation.Emit(image);
            NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
            image.Position = 0;
            _context = new AssemblyLoadContext("Integral.Steps.Generated.Tests", isCollectible: true);
            type = _context.LoadFromStream(image).GetType("Integral", throwOnError: true)!;
        }

        foreach (var name in MethodNames)
            _methods.Add(name, type.GetMethod(name)!.CreateDelegate<Quadrature>());
    }

    [OneTimeTearDown]
    public void UnloadGeneratedCode()
    {
        _methods.Clear();
        _context?.Unload();
    }

    public static IEnumerable<TestCaseData> InvalidCounts()
    {
        foreach (var method in MethodNames)
        foreach (var count in new[]
                 {
                     0.0, -1.0, 0.5, 2.5, double.Epsilon, double.NaN,
                     double.PositiveInfinity, double.NegativeInfinity,
                     (double)int.MaxValue + 1, (double)uint.MaxValue, double.MaxValue
                 })
            yield return new TestCaseData(method, count);

        // Rounding this otherwise integral count up to an even count would
        // exceed the loop counter's range.
        yield return new TestCaseData(nameof(Integral.simpsonMethod), (double)int.MaxValue);
    }

    [TestCaseSource(nameof(InvalidCounts))]
    public void InvalidCountsAreRejectedBeforeSampling(string method, double count)
    {
        var evaluations = 0;
        double MustNotBeSampled(double x)
        {
            evaluations++;
            // Keep the regression bounded even against an unfixed implementation:
            // infinity or an out-of-range count must never start a huge loop.
            throw new SamplingStartedException();
        }

        var exception = NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            _methods[method](MustNotBeSampled, 0, 1, count));

        NUnit.Framework.Assert.That(exception!.ParamName, Is.EqualTo("N"));
        NUnit.Framework.Assert.That(evaluations, Is.Zero);
    }

    [TestCase(nameof(Integral.rectangleMethod), 1, 0.5)]
    [TestCase(nameof(Integral.rectangleMethod), 2, 0.5)]
    [TestCase(nameof(Integral.rectangleMethod), 3, 0.5)]
    [TestCase(nameof(Integral.trapezoidalMethod), 1, 0.5)]
    [TestCase(nameof(Integral.trapezoidalMethod), 2, 0.5)]
    [TestCase(nameof(Integral.trapezoidalMethod), 3, 0.5)]
    [TestCase(nameof(Integral.simpsonMethod), 1, 0.25)]
    [TestCase(nameof(Integral.simpsonMethod), 2, 0.25)]
    [TestCase(nameof(Integral.simpsonMethod), 3, 0.25)]
    public void ValidSmallCountsRetainTheRuleAndStayInsideTheInterval(string method, int count, double expected)
    {
        foreach (var reversed in new[] { false, true })
        {
            var samples = new List<double>();
            var result = _methods[method](x =>
            {
                samples.Add(x);
                return method == nameof(Integral.simpsonMethod) ? x * x * x : x;
            }, reversed ? 1 : 0, reversed ? 0 : 1, count);

            NUnit.Framework.Assert.That(result, Is.EqualTo(reversed ? -expected : expected).Within(1e-14));
            NUnit.Framework.Assert.That(samples, Is.All.InRange(0.0, 1.0));
            var panels = method == nameof(Integral.simpsonMethod) && count % 2 == 1 ? count + 1 : count;
            var nodes = method == nameof(Integral.rectangleMethod) ? panels : panels + 1;
            NUnit.Framework.Assert.That(samples.Distinct().Count(), Is.EqualTo(nodes));
        }
    }

    [TestCase(nameof(Integral.rectangleMethod), (double)int.MaxValue)]
    [TestCase(nameof(Integral.trapezoidalMethod), (double)int.MaxValue)]
    [TestCase(nameof(Integral.simpsonMethod), (double)int.MaxValue - 1)]
    [TestCase(nameof(Integral.simpsonMethod), (double)int.MaxValue - 2)]
    public void LargestSupportedPanelCountsAreStillAccepted(string method, double count)
    {
        // This tests acceptance of the public upper boundary without performing
        // billions of evaluations. Stop at the first requested sample.
        NUnit.Framework.Assert.Throws<SamplingStartedException>(() =>
            _methods[method](_ => throw new SamplingStartedException(), 0, 1, count));
    }

    [TestCase(nameof(Integral.rectangleMethod))]
    [TestCase(nameof(Integral.trapezoidalMethod))]
    [TestCase(nameof(Integral.simpsonMethod))]
    public void TslExpressionsRejectFractionalCounts(string method)
    {
        var engine = new PortableEngine();
        var exception = NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            engine.EvaluateReal($"Integral.{method}(t => 2.0, 0, 1, 2.5)",
                customFunctions: generatedSource ? Integral.ToCode : string.Empty));

        NUnit.Framework.Assert.That(exception!.ParamName, Is.EqualTo("N"));
    }

    private sealed class SamplingStartedException : Exception;
}
