using Complex = System.Numerics.Complex;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Loader;
using Computator.NET.Core.Functions;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture(typeof(double), false)]
[TestFixture(typeof(double), true)]
[TestFixture(typeof(float), false)]
[TestFixture(typeof(float), true)]
[TestFixture(typeof(Complex), false)]
[TestFixture(typeof(Complex), true)]
[TestFixture(typeof(Complex32), false)]
[TestFixture(typeof(Complex32), true)]
public class TypedMatrixFactoryTests<T>(bool generatedSource)
    where T : struct, IEquatable<T>, IFormattable
{
    private MatrixFactoryImplementation _implementation = null!;
    private Func<int, int, Matrix<T>> _matrix = null!;
    private Func<int, Vector<T>> _vector = null!;

    [OneTimeSetUp]
    public void SelectHostOrCompleteEmittedImplementation()
    {
        _implementation = new MatrixFactoryImplementation(generatedSource);
        _matrix = _implementation.Matrix.MakeGenericMethod(typeof(T))
            .CreateDelegate<Func<int, int, Matrix<T>>>();
        _vector = _implementation.Vector.MakeGenericMethod(typeof(T))
            .CreateDelegate<Func<int, Vector<T>>>();
    }

    [OneTimeTearDown]
    public void ReleaseGeneratedCode()
    {
        _matrix = null!;
        _vector = null!;
        _implementation.Dispose();
    }

    [Test]
    public void MatrixHasTheRequestedScalarTypeAndZeroFilledRectangularShape()
    {
        var result = _matrix(2, 3);
        NUnit.Framework.Assert.That(result, Is.Not.Null);
        NUnit.Framework.Assert.That(result.RowCount, Is.EqualTo(2));
        NUnit.Framework.Assert.That(result.ColumnCount, Is.EqualTo(3));
        NUnit.Framework.Assert.That(result.Storage.IsDense, Is.True);
        NUnit.Framework.Assert.That(result.ToArray(), Is.EqualTo(new T[2, 3]));
    }

    [Test]
    public void VectorHasTheRequestedScalarTypeAndZeroFilledLength()
    {
        var result = _vector(3);
        NUnit.Framework.Assert.That(result, Is.Not.Null);
        NUnit.Framework.Assert.That(result.Count, Is.EqualTo(3));
        NUnit.Framework.Assert.That(result.Storage.IsDense, Is.True);
        NUnit.Framework.Assert.That(result.ToArray(), Is.EqualTo(new T[3]));
    }

    [Test]
    public void AllocationsHaveIndependentMutableStorageAndSupportMatrixVectorMultiplication()
    {
        var firstMatrix = _matrix(2, 3);
        var secondMatrix = _matrix(2, 3);
        var firstVector = _vector(3);
        var secondVector = _vector(3);
        firstMatrix[0, 1] = Matrix<T>.One;
        firstVector[1] = Vector<T>.One;

        NUnit.Framework.Assert.That(secondMatrix.ToArray(), Is.EqualTo(new T[2, 3]));
        NUnit.Framework.Assert.That(secondVector.ToArray(), Is.EqualTo(new T[3]));
        NUnit.Framework.Assert.That((firstMatrix * firstVector).ToArray(),
            Is.EqualTo(new[] { Matrix<T>.One, default(T) }));
    }

    [TestCase(0, 0)]
    [TestCase(0, 3)]
    [TestCase(2, 0)]
    public void EmptyMatricesKeepTheirRequestedDimensions(int rows, int columns)
    {
        var result = _matrix(rows, columns);
        NUnit.Framework.Assert.That(result.RowCount, Is.EqualTo(rows));
        NUnit.Framework.Assert.That(result.ColumnCount, Is.EqualTo(columns));
        NUnit.Framework.Assert.That(result.ToArray().Length, Is.Zero);
    }

    [Test]
    public void EmptyVectorsRemainSupported() =>
        NUnit.Framework.Assert.That(_vector(0).Count, Is.Zero);

    [TestCase(-1, 2)]
    [TestCase(2, -1)]
    public void NegativeMatrixDimensionsAreRejected(int rows, int columns) =>
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => _matrix(rows, columns));

    [Test]
    public void NegativeVectorLengthIsRejected() =>
        NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => _vector(-1));
}

[TestFixture(false)]
[TestFixture(true)]
public class UnsupportedMatrixFactoryTests(bool generatedSource)
{
    private MatrixFactoryImplementation _implementation = null!;

    [OneTimeSetUp]
    public void SelectImplementation() => _implementation = new MatrixFactoryImplementation(generatedSource);

    [OneTimeTearDown]
    public void ReleaseGeneratedCode() => _implementation.Dispose();

    [TestCase(typeof(sbyte))]
    [TestCase(typeof(byte))]
    [TestCase(typeof(short))]
    [TestCase(typeof(ushort))]
    [TestCase(typeof(int))]
    [TestCase(typeof(uint))]
    [TestCase(typeof(long))]
    [TestCase(typeof(ulong))]
    [TestCase(typeof(decimal))]
    [TestCase(typeof(DateTime))]
    public void UnsupportedScalarsRetainArgumentErrorsAtBothFactories(Type scalar)
    {
        foreach (var (factory, arguments) in new[]
                 {
                     (_implementation.Matrix, new object[] { 2, 3 }),
                     (_implementation.Vector, new object[] { 3 })
                 })
        {
            NUnit.Framework.Assert.Throws<ArgumentException>(() =>
                Invoke(factory.MakeGenericMethod(scalar), arguments));
        }
    }

    private static void Invoke(MethodInfo method, object[] arguments)
    {
        try { method.Invoke(null, arguments); }
        catch (TargetInvocationException error) when (error.InnerException != null)
        {
            // Assert the factory's exception, not reflection's wrapper.
            ExceptionDispatchInfo.Capture(error.InnerException).Throw();
        }
    }
}

[TestFixture]
public class TypedMatrixFactoryExecutionTests
{
    [TestCase("real", "1.5", "1.5")]
    [TestCase("float", "(float)1.5", "1.5")]
    [TestCase("complex", "new complex(0,2)", "2")]
    [TestCase("MathNet.Numerics.Complex32", "new MathNet.Numerics.Complex32(0,2)", "2")]
    public void PortableScriptsCanCreatePopulateAndMultiplyTypedFactories(string scalar, string value, string expected)
    {
        var component = scalar is "complex" or "MathNet.Numerics.Complex32" ? ".Imaginary" : "";
        var script = $"""
            var m = matrix<{scalar}>(2,3);
            var v = vector<{scalar}>(3);
            m[0,1] = {value}; v[1] = 1;
            write((m*v)[0]{component});
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo(expected));
    }

    [TestCase("matrix<integer>(2,3)")]
    [TestCase("matrix<natural>(2,3)")]
    [TestCase("vector<decimal>(3)")]
    [TestCase("vector<int>(3)")]
    public void PortableExecutionReportsUnsupportedScalarsAtConstruction(string expression) =>
        NUnit.Framework.Assert.Throws<ArgumentException>(() => new PortableEngine().ExecuteScript($"var m={expression};"));

    [Test]
    public void ElementAndArrayOverloadsKeepTheirExistingMeaning()
    {
        NUnit.Framework.Assert.That(MatrixFunctions.vector(3.0).ToArray(), Is.EqualTo(new[] { 3.0 }));
        NUnit.Framework.Assert.That(MatrixFunctions.vector(Complex.ImaginaryOne).ToArray(),
            Is.EqualTo(new[] { Complex.ImaginaryOne }));
        var input = new[,] { { 1, 2 }, { 3, 4 } };
        var matrix = MatrixFunctions.matrix(input);
        matrix[0, 0] = 10;
        NUnit.Framework.Assert.That(input[0, 0], Is.EqualTo(1));
        NUnit.Framework.Assert.That(matrix[1, 1], Is.EqualTo(4));
    }
}

internal sealed class MatrixFactoryImplementation : IDisposable
{
    private AssemblyLoadContext? _context;
    public MethodInfo Matrix { get; private set; }
    public MethodInfo Vector { get; private set; }

    public MatrixFactoryImplementation(bool generatedSource)
    {
        var type = typeof(MatrixFunctions);
        if (generatedSource)
        {
            // Execute the whole independently emitted desktop helper, not an
            // extracted copy of the two factory methods.
            var source = "using Computator.NET.Core.Evaluation; public static class EmittedMatrixFunctions {"
                + MatrixFunctions.ToCode + "}";
            var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
                .Concat(new[] { typeof(MatrixFunctions).Assembly.Location, typeof(Matrix<double>).Assembly.Location,
                    typeof(Accord.Math.Integration.RombergMethod).Assembly.Location }).Distinct();
            var compilation = CSharpCompilation.Create("TypedMatrixFactories.Generated.Tests",
                [CSharpSyntaxTree.ParseText(source)], paths.Select(path => MetadataReference.CreateFromFile(path)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var image = new MemoryStream();
            var result = compilation.Emit(image);
            NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
            image.Position = 0;
            _context = new AssemblyLoadContext("TypedMatrixFactories.Generated.Tests", isCollectible: true);
            type = _context.LoadFromStream(image).GetType("EmittedMatrixFunctions", throwOnError: true)!;
        }
        Matrix = type.GetMethods().Single(method => method.Name == "matrix" && method.IsGenericMethodDefinition);
        Vector = type.GetMethods().Single(method => method.Name == "vector" && method.IsGenericMethodDefinition);
    }

    public void Dispose()
    {
        Matrix = null!;
        Vector = null!;
        _context?.Unload();
        _context = null;
    }
}
