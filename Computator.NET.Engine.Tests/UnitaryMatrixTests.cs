using System.Numerics;
using System.Runtime.Loader;
using Computator.NET.Core.Functions;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(false)]
[TestFixture(true)]
public class UnitaryMatrixTests(bool generatedSource)
{
    private Func<Matrix<double>, bool> _real = null!;
    private Func<Matrix<Complex>, bool> _complex = null!;
    private AssemblyLoadContext? _context;

    [OneTimeSetUp]
    public void SelectHostOrCompleteEmittedImplementation()
    {
        if (!generatedSource)
        {
            _real = MatrixFunctions.isUnitary;
            _complex = MatrixFunctions.isUnitary;
            return;
        }

        var source = "using Computator.NET.Core.Evaluation; public static class EmittedMatrixFunctions {"
            + MatrixFunctions.ToCode + "}";
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Concat(new[] { typeof(MatrixFunctions).Assembly.Location, typeof(Matrix<double>).Assembly.Location,
                typeof(Accord.Math.Integration.RombergMethod).Assembly.Location }).Distinct();
        var compilation = CSharpCompilation.Create("UnitaryMatrix.Generated.Tests",
            [CSharpSyntaxTree.ParseText(source)], paths.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
        image.Position = 0;
        _context = new AssemblyLoadContext("UnitaryMatrix.Generated.Tests", isCollectible: true);
        var method = _context.LoadFromStream(image).GetType("EmittedMatrixFunctions", throwOnError: true)!
            .GetMethod("isUnitary")!;
        _real = method.MakeGenericMethod(typeof(double)).CreateDelegate<Func<Matrix<double>, bool>>();
        _complex = method.MakeGenericMethod(typeof(Complex)).CreateDelegate<Func<Matrix<Complex>, bool>>();
    }

    [OneTimeTearDown]
    public void UnloadGeneratedCode()
    {
        _real = null!;
        _complex = null!;
        _context?.Unload();
    }

    [TestCase(1, 2, false)]
    [TestCase(1, 2, true)]
    [TestCase(2, 3, false)]
    [TestCase(2, 3, true)]
    [TestCase(3, 2, false)]
    [TestCase(3, 2, true)]
    public void RectangularCoordinateMapsAreNotUnitary(int rows, int columns, bool sparse)
    {
        // For rows < columns, A*A^H=I but A^H*A has a nontrivial null space.
        // A unitary transformation must preserve vectors in both directions.
        var values = new double[rows, columns];
        for (var i = 0; i < Math.Min(rows, columns); i++) values[i, i] = 1;
        var real = Create(values, sparse);
        var complexValues = new Complex[rows, columns];
        for (var i = 0; i < Math.Min(rows, columns); i++) complexValues[i, i] = Complex.ImaginaryOne;
        var complex = Create(complexValues, sparse);

        NUnit.Framework.Assert.That(_real(real), Is.False);
        NUnit.Framework.Assert.That(_complex(complex), Is.False);
        NUnit.Framework.Assert.That(real.ToArray(), Is.EqualTo(values));
        NUnit.Framework.Assert.That(complex.ToArray(), Is.EqualTo(complexValues));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void RectangularDenseOrthonormalRowsDoNotMakeAUnitaryMatrix(bool sparse)
    {
        var real = Create(new double[,] { { 0.6, 0.8, 0 }, { 0, 0, 1 } }, sparse);
        var complex = Create(new Complex[,] { { 0.6, new Complex(0, 0.8), 0 }, { 0, 0, Complex.ImaginaryOne } }, sparse);
        NUnit.Framework.Assert.That(_real(real), Is.False);
        NUnit.Framework.Assert.That(_complex(complex), Is.False);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SquareOrthogonalAndComplexUnitaryMatricesRemainAccepted(bool sparse)
    {
        var realValues = new double[,] { { 0.6, -0.8 }, { 0.8, 0.6 } };
        var complexValues = new Complex[,] { { 0.6, new Complex(0, 0.8) }, { new Complex(0, 0.8), 0.6 } };
        var real = Create(realValues, sparse);
        var complex = Create(complexValues, sparse);
        NUnit.Framework.Assert.That(_real(real), Is.True);
        NUnit.Framework.Assert.That(_complex(complex), Is.True, "The test must conjugate, not merely transpose.");
        NUnit.Framework.Assert.That(real.ToArray(), Is.EqualTo(realValues));
        NUnit.Framework.Assert.That(complex.ToArray(), Is.EqualTo(complexValues));
    }

    [TestCase(0.0, false)]
    [TestCase(0.5, false)]
    [TestCase(1.0, true)]
    [TestCase(-1.0, true)]
    [TestCase(2.0, false)]
    [TestCase(double.NaN, false)]
    [TestCase(double.PositiveInfinity, false)]
    public void SquareScalarMatricesStillRequireUnitMagnitude(double value, bool expected)
    {
        NUnit.Framework.Assert.That(_real(Create(new[,] { { value } }, false)), Is.EqualTo(expected));
        NUnit.Framework.Assert.That(_complex(Create(new[,] { { new Complex(0, value) } }, false)), Is.EqualTo(expected));
    }

    [TestCase(1e-12, true)]
    [TestCase(1e-6, false)]
    public void ExistingApproximateComparisonToleranceIsPreserved(double perturbation, bool expected)
    {
        var scale = 1.0 + perturbation;
        NUnit.Framework.Assert.That(_real(Create(new[,] { { scale, 0 }, { 0, 1.0 } }, false)), Is.EqualTo(expected));
        NUnit.Framework.Assert.That(_complex(Create(new Complex[,] { { new Complex(0, scale), 0 }, { 0, 1 } }, false)), Is.EqualTo(expected));
    }

    private static Matrix<T> Create<T>(T[,] values, bool sparse)
        where T : struct, IEquatable<T>, IFormattable =>
        sparse ? Matrix<T>.Build.SparseOfArray(values) : Matrix<T>.Build.DenseOfArray(values);
}

[TestFixture]
public class UnitaryMatrixExpressionTests
{
    [TestCase("isUnitary(matrix({{1,0}})) ? 1 : 0", 0.0)]
    [TestCase("isUnitary(matrix({{1,0,0},{0,1,0}})) ? 1 : 0", 0.0)]
    [TestCase("isUnitary(matrix({{0,-1},{1,0}})) ? 1 : 0", 1.0)]
    [TestCase("isUnitary(matrix({{1,1},{0,1}})) ? 1 : 0", 0.0)]
    public void PortableExpressionsUseTheUnitaryPredicate(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.EqualTo(expected));

    [Test]
    public void PortableScriptsRejectRectangularComplexRows()
    {
        const string script = """
            var m = matrix(new System.Numerics.Complex[,] { { 0, System.Numerics.Complex.ImaginaryOne } });
            write(isUnitary(m));
            write(";");
            write(isUnitary(matrix(new System.Numerics.Complex[,] { { System.Numerics.Complex.ImaginaryOne } })));
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("False;True"));
    }

    [Test]
    public void LegacyIdentityPatternPredicateStillAcceptsRectangularPatterns()
    {
        // Keep the existing separately documented pattern API unchanged.
        NUnit.Framework.Assert.That(MatrixFunctions.isIndentity(Matrix<double>.Build.DenseIdentity(2, 3)), Is.True);
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(isIndentity(matrix({{1,0}})));"), Is.EqualTo("True"));
    }
}
