using System.Runtime.Loader;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class FunctionRootTests(bool generatedSource)
{
    private delegate double RootFinder(Func<double, double> function, double a, double b, double tolerance, uint iterations);
    private delegate double RootDispatcher(string method, Func<double, double> function, double a, double b, double tolerance, uint iterations);

    private RootFinder _findRoot = null!;
    private RootDispatcher _dispatch = null!;
    private AssemblyLoadContext? _generatedContext;

    [OneTimeSetUp]
    public void LoadTheRuntimeOrGeneratedTslImplementation()
    {
        if (!generatedSource)
        {
            _findRoot = FunctionRoot.secantNewtonRaphsonMethod;
            _dispatch = FunctionRoot.findRoot;
            return;
        }

        // Execute the actual embedded class used by generated desktop TSL programs.
        // Checking the string alone would miss compilation or behavioral drift.
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Append(typeof(MathNet.Numerics.RootFinding.Secant).Assembly.Location).Distinct();
        var compilation = CSharpCompilation.Create("FunctionRoot.Generated.Tests",
            [CSharpSyntaxTree.ParseText(FunctionRoot.ToCode)],
            paths.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
        image.Position = 0;
        _generatedContext = new AssemblyLoadContext("FunctionRoot.Generated.Tests", isCollectible: true);
        var type = _generatedContext.LoadFromStream(image).GetType("FunctionRoot", throwOnError: true)!;
        _findRoot = type.GetMethod(nameof(FunctionRoot.secantNewtonRaphsonMethod))!.CreateDelegate<RootFinder>();
        _dispatch = type.GetMethod(nameof(FunctionRoot.findRoot))!.CreateDelegate<RootDispatcher>();
    }

    [OneTimeTearDown]
    public void UnloadGeneratedCode()
    {
        _findRoot = null!;
        _dispatch = null!;
        _generatedContext?.Unload();
    }

    [TestCase(-2.0, 2.0, 1.0)]
    [TestCase(-2.0, 2.0, -1.0)]
    [TestCase(10.0, 12.0, 10.75)]
    [TestCase(-12.0, -10.0, -10.75)]
    [TestCase(1.0, 2.0, 1.0)]
    [TestCase(1.0, 2.0, 2.0)]
    public void FindsLinearRootsInSymmetricAndTranslatedIntervals(double a, double b, double expected)
    {
        var root = _findRoot(x => x - expected, a, b, 1e-10, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(expected).Within(1e-10));
    }

    [Test]
    public void DistinctInteriorGuessesFindANonlinearRootAcrossZero()
    {
        var root = _findRoot(x => x * x - 0.25, -1, 1, 1e-10, 100);
        NUnit.Framework.Assert.That(Math.Abs(root), Is.EqualTo(0.5).Within(1e-10));
    }

    [TestCase(1e308, 1.6e308, 1.4e308)]
    [TestCase(-1.6e308, -1e308, -1.4e308)]
    [TestCase(-1.6e308, 1.6e308, 4e307)]
    public void FiniteLargeIntervalsDoNotOverflowTheInitialGuesses(double a, double b, double expected)
    {
        var root = _findRoot(x => x / 1e308 - expected / 1e308, a, b, 1e-10, 100);
        NUnit.Framework.Assert.That(root / 1e308, Is.EqualTo(expected / 1e308).Within(1e-12));
    }

    [TestCase(1.0)]
    [TestCase(-1.0)]
    [TestCase(1e308)]
    public void UsesDistinctGuessesWhenRoundingLeavesOnlyTwoInteriorNumbers(double start)
    {
        var a = Math.BitIncrement(start);
        var b = Math.BitIncrement(Math.BitIncrement(Math.BitIncrement(a)));
        var root = _findRoot(x => (x - a) / (b - a), a, b, 1e-10, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(a));
    }

    [Test]
    public void CanUseTheLowerInteriorValueAcrossAnExponentBoundary()
    {
        var boundary = Math.ScaleB(1.0, -1020);
        var a = Math.BitDecrement(Math.BitDecrement(boundary));
        var b = Math.BitIncrement(boundary);
        // The two interior values have different spacing. Scaling the residual
        // avoids underflow in the solver's secant step at this magnitude.
        var root = _findRoot(x => (x - a) / double.Epsilon, a, b, 1e-10, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(a));
    }

    [Test]
    public void AnIntervalWithoutTwoInteriorNumbersDoesNotEvaluateAnUndefinedSecant()
    {
        var a = 1.0;
        var b = Math.BitIncrement(Math.BitIncrement(a));
        var evaluations = 0;
        var root = _findRoot(x => { evaluations++; return x - a; }, a, b, 1e-10, 100);
        NUnit.Framework.Assert.That(root, Is.NaN);
        NUnit.Framework.Assert.That(evaluations, Is.Zero);
    }

    [TestCase(1.0, 1.0)]
    [TestCase(2.0, 1.0)]
    public void EmptyAndReversedIntervalsStillReturnNaN(double a, double b)
    {
        NUnit.Framework.Assert.That(_findRoot(x => x - 1, a, b, 1e-10, 100), Is.NaN);
    }

    [TestCase(0.0)]
    [TestCase(-1.0)]
    public void NonpositiveToleranceStillUsesTheMathNetArgumentValidation(double tolerance)
    {
        var b = Math.BitIncrement(Math.BitIncrement(1.0));
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => _findRoot(x => x - 1, 1, b, tolerance, 100));
    }

    [Test]
    public void AFunctionWithoutARealRootStillReturnsNaN()
    {
        NUnit.Framework.Assert.That(_findRoot(x => x * x + 1, -2, 2, 1e-10, 100), Is.NaN);
    }

    [Test]
    public void NamedMethodDispatchUsesTheCorrectedSolver()
    {
        var root = _dispatch("secant Newton Raphson method", x => x - 1, -2, 2, 1e-10, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(1).Within(1e-10));
    }
}
