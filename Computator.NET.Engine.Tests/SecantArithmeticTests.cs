using System.Runtime.Loader;
using Computator.NET.Core.NumericalCalculations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class SecantArithmeticTests(bool generatedSource)
{
    private delegate double RootFinder(Func<double, double> function, double a, double b, double tolerance, uint iterations);
    private delegate double RootDispatcher(string method, Func<double, double> function, double a, double b, double tolerance, uint iterations);

    private RootFinder _findRoot = null!;
    private RootDispatcher _dispatch = null!;
    private AssemblyLoadContext? _context;

    [OneTimeSetUp]
    public void LoadTheHostOrCompleteGeneratedImplementation()
    {
        var type = typeof(FunctionRoot);
        if (generatedSource)
        {
            var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Append(typeof(MathNet.Numerics.RootFinding.Secant).Assembly.Location).Distinct();
            var compilation = CSharpCompilation.Create("Secant.Arithmetic.Generated.Tests",
                [CSharpSyntaxTree.ParseText(FunctionRoot.ToCode)],
                paths.Select(path => MetadataReference.CreateFromFile(path)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var image = new MemoryStream();
            var result = compilation.Emit(image);
            NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
            image.Position = 0;
            _context = new AssemblyLoadContext("Secant.Arithmetic.Generated.Tests", isCollectible: true);
            type = _context.LoadFromStream(image).GetType("FunctionRoot", throwOnError: true)!;
        }

        _findRoot = type.GetMethod(nameof(FunctionRoot.secantMethod))!.CreateDelegate<RootFinder>();
        _dispatch = type.GetMethod(nameof(FunctionRoot.findRoot))!.CreateDelegate<RootDispatcher>();
    }

    [OneTimeTearDown]
    public void UnloadGeneratedCode()
    {
        _findRoot = null!;
        _dispatch = null!;
        _context?.Unload();
    }

    [TestCase(700, 1)]
    [TestCase(700, -1)]
    [TestCase(-700, 1)]
    [TestCase(-700, -1)]
    [TestCase(0, 1)]
    [TestCase(0, -1)]
    public void FindsAnExactLinearRootWithoutOverflowingOrUnderflowingTheProduct(int exponent, int sign)
    {
        var scale = sign * Math.ScaleB(1, exponent);
        var expected = 1.5 * scale;
        NUnit.Framework.Assert.That(_findRoot(x => x - expected, scale, 2 * scale, 0, 2), Is.EqualTo(expected));
    }

    [TestCase(1e308)]
    [TestCase(-1e308)]
    [TestCase(1e-308)]
    [TestCase(-1e-308)]
    public void ScalingTheResidualDoesNotLoseAnExactRoot(double scale)
    {
        NUnit.Framework.Assert.That(_findRoot(x => scale * x, -1, 1, 0, 2), Is.Zero);
    }

    [TestCase(1)]
    [TestCase(-1)]
    public void OppositeFiniteEndpointsDoNotProduceAnInfiniteWidth(int direction)
    {
        var seen = new List<double>();
        var root = _findRoot(x => { seen.Add(x); return x / double.MaxValue; },
            -direction * double.MaxValue, direction * double.MaxValue, 0, 2);
        NUnit.Framework.Assert.That(root, Is.Zero);
        NUnit.Framework.Assert.That(seen.All(double.IsFinite), Is.True);
    }

    [TestCase(1)]
    [TestCase(-1)]
    public void UnbracketedGuessesStillAllowAFiniteRootOutsideTheInitialInterval(int direction)
    {
        var scale = direction * Math.ScaleB(1, 1022);
        var expected = 3 * scale;
        NUnit.Framework.Assert.That(_findRoot(x => x / scale - 3, scale, 2 * scale, 0, 2), Is.EqualTo(expected));
    }

    [TestCase(1)]
    [TestCase(-1)]
    public void AFiniteExtrapolationCanCrossMoreThanTheLargestRepresentableDistance(int direction)
    {
        var scale = direction * Math.ScaleB(1, 1022);
        var expected = 3 * scale;
        NUnit.Framework.Assert.That(_findRoot(x => x / scale - 3, -3 * scale, -2 * scale, 0, 2), Is.EqualTo(expected));
    }

    [TestCase(1e-200)]
    [TestCase(1e-300)]
    public void TinyRootIsRetainedWhenTheResidualRatioUnderflows(double expected)
    {
        var root = _findRoot(x => x - expected, 0, 1e200, 0, 2);
        NUnit.Framework.Assert.That(root, Is.EqualTo(expected).Within(Math.Abs(expected) * 2e-15));
    }

    [TestCase(1e16)]
    [TestCase(-1e16)]
    [TestCase(1e200)]
    [TestCase(-1e200)]
    public void NearbyResidualsRetainTheirDifferenceBeforeDivision(double expected)
    {
        // The two residuals differ by one representable step. Dividing them
        // before subtracting one would round away some of that information.
        var spacing = Math.BitIncrement(expected) - expected;
        NUnit.Framework.Assert.That(_findRoot(x => x - expected, 0, spacing, 0, 1), Is.EqualTo(expected));
    }

    [Test]
    public void OrdinaryNonlinearRootRetainsResidualAccuracy()
    {
        var root = _findRoot(x => x * x - 2, 1, 2, 1e-12, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(Math.Sqrt(2)).Within(1e-12));
        NUnit.Framework.Assert.That(Math.Abs(root * root - 2), Is.LessThanOrEqualTo(1e-12));
    }

    [Test]
    public void TrigonometricRootRetainsResidualAccuracy()
    {
        var root = _findRoot(x => Math.Cos(x) - x, 0, 1, 1e-12, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(0.7390851332151607).Within(1e-12));
    }

    [Test]
    public void EachNewPointIsSampledOnce()
    {
        var points = new List<double>();
        var root = _findRoot(x => { points.Add(x); return x - 1.5; }, 1, 2, 0, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(1.5));
        NUnit.Framework.Assert.That(points, Is.EqualTo(new[] { 2.0, 1.0, 1.5 }));
    }

    [Test]
    public void ExactSecondGuessDoesNotEvaluateTheUnusedFirstGuess()
    {
        var points = new List<double>();
        var root = _findRoot(x => { points.Add(x); return x - 2; }, 1, 2, 0, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(2));
        NUnit.Framework.Assert.That(points, Is.EqualTo(new[] { 2.0 }));
    }

    [Test]
    public void ExactFirstGuessIsReturnedWithoutResamplingIt()
    {
        var points = new List<double>();
        var root = _findRoot(x => { points.Add(x); return x - 1; }, 1, 2, 0, 100);
        NUnit.Framework.Assert.That(root, Is.EqualTo(1));
        NUnit.Framework.Assert.That(points, Is.EqualTo(new[] { 2.0, 1.0 }));
    }

    [TestCase(0)]
    [TestCase(1)]
    public void CoincidentGuessesAreSampledOnlyOnce(double residual)
    {
        var count = 0;
        var root = _findRoot(x => { count++; return residual; }, 2, 2, 0, 100);
        NUnit.Framework.Assert.That(count, Is.EqualTo(1));
        NUnit.Framework.Assert.That(root, residual == 0 ? Is.EqualTo(2) : Is.NaN);
    }

    [TestCase(0, 0, true)]
    [TestCase(1e-10, 1e-9, true)]
    [TestCase(1e-10, 1e-11, false)]
    public void ZeroIterationsOnlyChecksTheSecondGuess(double residual, double tolerance, bool accepted)
    {
        var points = new List<double>();
        var root = _findRoot(x => { points.Add(x); return residual; }, 1, 2, tolerance, 0);
        NUnit.Framework.Assert.That(points, Is.EqualTo(new[] { 2.0 }));
        NUnit.Framework.Assert.That(root, accepted ? Is.EqualTo(2) : Is.NaN);
    }

    [TestCase(-1)]
    [TestCase(double.NaN)]
    public void InvalidToleranceDoesNotAcceptAnExactRoot(double tolerance)
    {
        NUnit.Framework.Assert.That(_findRoot(x => x - 1.5, 1, 2, tolerance, 10), Is.NaN);
    }

    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void EqualOrNonfiniteResidualsDoNotInventARoot(double residual)
    {
        NUnit.Framework.Assert.That(_findRoot(x => residual, 1, 2, 1e-12, 2), Is.NaN);
    }

    [Test]
    public void ExceptionsFromTheFunctionArePropagated()
    {
        NUnit.Framework.Assert.Throws<InvalidOperationException>(() =>
            _findRoot(x => throw new InvalidOperationException("sample failure"), 1, 2, 1e-12, 2));
    }

    [Test]
    public void AnInsufficientIterationBudgetStillReturnsNaN()
    {
        NUnit.Framework.Assert.That(_findRoot(x => x * x - 2, 1, 2, 1e-14, 1), Is.NaN);
    }

    [Test]
    public void MaximumUnsignedBudgetCanReturnAnImmediateRoot()
    {
        NUnit.Framework.Assert.That(_findRoot(x => x - 1.5, 1, 2, 0, uint.MaxValue), Is.EqualTo(1.5));
    }

    [Test]
    public void NamedDispatcherUsesTheCorrectedPlainSecantMethod()
    {
        var scale = Math.ScaleB(1, 700);
        NUnit.Framework.Assert.That(_dispatch("secant method", x => x - 1.5 * scale,
            scale, 2 * scale, 0, 2), Is.EqualTo(1.5 * scale));
    }

    [TestCase(700)]
    [TestCase(-700)]
    public void PortableTslCanCallTheHostAndCompleteGeneratedMethod(int exponent)
    {
        var script = "real scale = System.Math.ScaleB(1.0," + exponent + "); " +
            "write(FunctionRoot.secantMethod((real t) => t-1.5*scale, scale, 2*scale, 0, 2)/scale);";
        var result = new PortableEngine().ExecuteScript(script,
            customFunctions: generatedSource ? FunctionRoot.ToCode : string.Empty);
        NUnit.Framework.Assert.That(result, Is.EqualTo("1.5"));
    }
}
