using System.Runtime.Loader;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class BroydenMidpointTests(bool generatedSource)
{
    private delegate double RootFinder(Func<double, double> function, double a, double b, double tolerance, uint iterations);
    private delegate double RootDispatcher(string method, Func<double, double> function, double a, double b, double tolerance, uint iterations);

    private RootFinder _findRoot = null!;
    private RootDispatcher _dispatch = null!;
    private AssemblyLoadContext? _generatedContext;

    [OneTimeSetUp]
    public void LoadTheRuntimeOrActualGeneratedTslImplementation()
    {
        if (!generatedSource)
        {
            _findRoot = FunctionRoot.BroydenMethod;
            _dispatch = FunctionRoot.findRoot;
            return;
        }

        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Append(typeof(MathNet.Numerics.RootFinding.Broyden).Assembly.Location).Distinct();
        var compilation = CSharpCompilation.Create("BroydenMidpoint.Generated.Tests",
            [CSharpSyntaxTree.ParseText(FunctionRoot.ToCode)],
            paths.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
        image.Position = 0;
        _generatedContext = new AssemblyLoadContext("BroydenMidpoint.Generated.Tests", isCollectible: true);
        var type = _generatedContext.LoadFromStream(image).GetType("FunctionRoot", throwOnError: true)!;
        _findRoot = type.GetMethod(nameof(FunctionRoot.BroydenMethod))!.CreateDelegate<RootFinder>();
        _dispatch = type.GetMethod(nameof(FunctionRoot.findRoot))!.CreateDelegate<RootDispatcher>();
    }

    [OneTimeTearDown]
    public void UnloadGeneratedCode()
    {
        _findRoot = null!;
        _dispatch = null!;
        _generatedContext?.Unload();
    }

    public static IEnumerable<TestCaseData> Midpoints()
    {
        // Binary-scaled operands make these expected midpoints exact, without
        // repeating the production formula or depending on decimal rounding.
        var large = Math.ScaleB(1, 1023);
        var nearMaximum = Math.BitDecrement(double.MaxValue);
        yield return Midpoint(large, 1.5 * large, 1.25 * large, "LargePositive");
        yield return Midpoint(1.5 * large, large, 1.25 * large, "LargePositiveReversed");
        yield return Midpoint(-large, -1.5 * large, -1.25 * large, "LargeNegative");
        yield return Midpoint(-1.5 * large, -large, -1.25 * large, "LargeNegativeReversed");
        yield return Midpoint(double.MaxValue, double.MaxValue, double.MaxValue, "MaximumRepeated");
        yield return Midpoint(-double.MaxValue, -double.MaxValue, -double.MaxValue, "MinimumRepeated");
        // The tie rounds to the adjacent value with the even significand.
        yield return Midpoint(nearMaximum, double.MaxValue, nearMaximum, "AdjacentMaximum");
        yield return Midpoint(-nearMaximum, -double.MaxValue, -nearMaximum, "AdjacentMinimum");
        yield return Midpoint(-double.MaxValue, double.MaxValue, 0, "OppositeExtremes");
        yield return Midpoint(double.MaxValue, -double.MaxValue, 0, "OppositeExtremesReversed");
        yield return Midpoint(double.MaxValue, -nearMaximum, Math.ScaleB(1, 970), "Cancellation");
        yield return Midpoint(-double.MaxValue, nearMaximum, -Math.ScaleB(1, 970), "NegativeCancellation");
        yield return Midpoint(2, 6, 4, "Ordinary");
        yield return Midpoint(6, 2, 4, "OrdinaryReversed");
        yield return Midpoint(-6, -2, -4, "OrdinaryNegative");
        yield return Midpoint(5, 5, 5, "EqualGuesses");
        yield return Midpoint(-6, 2, -2, "AcrossZero");
        yield return Midpoint(double.Epsilon, double.Epsilon, double.Epsilon, "SmallestRepeated");
        yield return Midpoint(double.Epsilon, 3 * double.Epsilon, 2 * double.Epsilon, "Subnormal");
        yield return Midpoint(-double.Epsilon, -3 * double.Epsilon, -2 * double.Epsilon, "NegativeSubnormal");
        yield return Midpoint(double.Epsilon, -3 * double.Epsilon, -double.Epsilon, "SubnormalCancellation");
        yield return Midpoint(-0.0, -0.0, -0.0, "NegativeZero");
    }

    private static TestCaseData Midpoint(double a, double b, double expected, string name) =>
        new TestCaseData(a, b, expected).SetName("BroydenInitialGuess_" + name);

    [TestCaseSource(nameof(Midpoints))]
    public void FirstEvaluationUsesTheRepresentableMidpoint(double a, double b, double expected)
    {
        double? first = null;
        // Stop at the initial sample: this tests the wrapper's input to Broyden
        // without attributing the solver's later Jacobian arithmetic to it.
        NUnit.Framework.Assert.Throws<InitialGuessObservedException>(() => _findRoot(x =>
        {
            first = x;
            throw new InitialGuessObservedException();
        }, a, b, 1e-10, 100));

        NUnit.Framework.Assert.That(first, Is.Not.Null);
        NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits(first!.Value),
            Is.EqualTo(BitConverter.DoubleToInt64Bits(expected)));
    }

    [TestCase(1)]
    [TestCase(-1)]
    public void FindsALinearRootWhenTheFiniteEndpointSumOverflows(int sign)
    {
        var large = sign * Math.ScaleB(1, 1023);
        var expected = 1.25 * large;
        var root = _findRoot(x => (x - expected) / Math.Abs(large), large, 1.5 * large, 1e-10, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(expected));
    }

    [TestCase(2, 4)]
    [TestCase(4, 2)]
    [TestCase(3, 3)]
    public void TheGuessesRemainAnInitialEstimateRatherThanEnforcedBounds(double a, double b)
    {
        var root = _findRoot(x => x - 10, a, b, 1e-10, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(10).Within(1e-10));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void NonpositiveToleranceStillUsesTheMathNetArgumentValidation(double tolerance)
    {
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
            _findRoot(x => x - 1, 0, 2, tolerance, 100));
    }

    [Test]
    public void NamedMethodDispatchUsesTheFiniteInitialEstimate()
    {
        var large = Math.ScaleB(1, 1023);
        var expected = 1.25 * large;
        var root = _dispatch("Broyden's method", x => (x - expected) / large,
            large, 1.5 * large, 1e-10, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(expected));
    }

    [TestCase(1)]
    [TestCase(-1)]
    public void PortableTslCanCallTheCorrectedHostAndGeneratedMethod(int sign)
    {
        // With the root at the exactly representable initial guess, this checks
        // the real language pipeline as well as the direct delegate above.
        var script = "real scale = " + sign + "*System.Math.ScaleB(1.0,1023); " +
            "write(FunctionRoot.BroydenMethod((real t) => (t-1.25*scale)/System.Math.Abs(scale), " +
            "scale, 1.5*scale, 1e-10, 100)/scale);";
        var result = new PortableEngine().ExecuteScript(script,
            customFunctions: generatedSource ? FunctionRoot.ToCode : string.Empty);
        NUnit.Framework.Assert.That(result, Is.EqualTo("1.25"));
    }

    private sealed class InitialGuessObservedException : Exception;
}
