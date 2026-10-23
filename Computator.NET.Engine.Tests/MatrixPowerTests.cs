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
public class MatrixPowerTests(bool generatedSource)
{
    private Func<Matrix<double>, int, Matrix<double>> _realPower = null!;
    private Func<Matrix<Complex>, int, Matrix<Complex>> _complexPower = null!;
    private AssemblyLoadContext? _generatedContext;

    [OneTimeSetUp]
    public void LoadTheRuntimeOrCompleteEmittedImplementation()
    {
        if (!generatedSource)
        {
            _realPower = MatrixFunctions.pow;
            _complexPower = MatrixFunctions.pow;
            return;
        }

        // Desktop TSL embeds this whole helper independently of the host methods.
        var source = "using Computator.NET.Core.Evaluation; public static class EmittedMatrixFunctions {"
            + MatrixFunctions.ToCode + "}";
        var paths = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Concat(new[] { typeof(MatrixFunctions).Assembly.Location, typeof(Matrix<double>).Assembly.Location,
                typeof(Accord.Math.Integration.RombergMethod).Assembly.Location }).Distinct();
        var compilation = CSharpCompilation.Create("MatrixPower.Generated.Tests",
            [CSharpSyntaxTree.ParseText(source)], paths.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine, result.Diagnostics));
        image.Position = 0;
        _generatedContext = new AssemblyLoadContext("MatrixPower.Generated.Tests", isCollectible: true);
        var type = _generatedContext.LoadFromStream(image).GetType("EmittedMatrixFunctions", throwOnError: true)!;
        var method = type.GetMethod("pow")!;
        _realPower = method.MakeGenericMethod(typeof(double)).CreateDelegate<Func<Matrix<double>, int, Matrix<double>>>();
        _complexPower = method.MakeGenericMethod(typeof(Complex)).CreateDelegate<Func<Matrix<Complex>, int, Matrix<Complex>>>();
    }

    [OneTimeTearDown]
    public void UnloadGeneratedCode()
    {
        _realPower = null!;
        _complexPower = null!;
        _generatedContext?.Unload();
    }

    private static IEnumerable<TestCaseData> IntegerPowers
    {
        get
        {
            foreach (var exponent in new[] { int.MinValue, int.MinValue + 1, -3, -1, 0, 1, 4, int.MaxValue })
                foreach (var sparse in new[] { false, true })
                    yield return new TestCaseData(exponent, sparse);
        }
    }

    [TestCaseSource(nameof(IntegerPowers))]
    public void RealJordanPowersFollowTheBinomialIdentityAndPreserveInput(int exponent, bool sparse)
    {
        // For N^2 = 0, (I + N)^n = I + n*N for every integer n, including negative n.
        // This gives an independent exact oracle without calling another power routine.
        var original = new double[,] { { 1, 0.5 }, { 0, 1 } };
        var input = Create(original, sparse);
        var result = _realPower(input, exponent);
        NUnit.Framework.Assert.That(result.ToArray(), Is.EqualTo(new double[,] { { 1, exponent * 0.5 }, { 0, 1 } }));
        result[0, 0] = 123;
        NUnit.Framework.Assert.That(input.ToArray(), Is.EqualTo(original));
    }

    [TestCaseSource(nameof(IntegerPowers))]
    public void ComplexJordanPowersFollowTheBinomialIdentityAndPreserveInput(int exponent, bool sparse)
    {
        var original = new Complex[,] { { Complex.One, Complex.ImaginaryOne }, { Complex.Zero, Complex.One } };
        var input = Create(original, sparse);
        var result = _complexPower(input, exponent);
        var expected = new Complex[,] { { Complex.One, new Complex(0, exponent) }, { Complex.Zero, Complex.One } };
        NUnit.Framework.Assert.That(result.ToArray(), Is.EqualTo(expected));
        result[0, 0] = 123;
        NUnit.Framework.Assert.That(input.ToArray(), Is.EqualTo(original));
    }

    [Test]
    public void MinimumExponentOfARealInvolutionIsIdentity()
    {
        var input = Matrix<double>.Build.DenseOfArray(new double[,] { { 0, 1 }, { 1, 0 } });
        NUnit.Framework.Assert.That(_realPower(input, int.MinValue).ToArray(),
            Is.EqualTo(new double[,] { { 1, 0 }, { 0, 1 } }));
    }

    [Test]
    public void MinimumExponentOfAComplexInvolutionIsIdentity()
    {
        var input = Matrix<Complex>.Build.DenseOfArray(new Complex[,] {
            { Complex.Zero, Complex.ImaginaryOne }, { -Complex.ImaginaryOne, Complex.Zero }
        });
        NUnit.Framework.Assert.That(_complexPower(input, int.MinValue).ToArray(),
            Is.EqualTo(new Complex[,] { { Complex.One, Complex.Zero }, { Complex.Zero, Complex.One } }));
    }

    [TestCase(int.MinValue)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(1)]
    public void RectangularMatricesStillRequireASquareShape(int exponent)
    {
        NUnit.Framework.Assert.Throws<ArgumentException>(() => _realPower(Matrix<double>.Build.Dense(2, 3), exponent));
        NUnit.Framework.Assert.Throws<ArgumentException>(() => _complexPower(Matrix<Complex>.Build.Dense(2, 3), exponent));
    }

    private static Matrix<T> Create<T>(T[,] values, bool sparse)
        where T : struct, IEquatable<T>, IFormattable =>
        sparse ? Matrix<T>.Build.SparseOfArray(values) : Matrix<T>.Build.DenseOfArray(values);
}

[TestFixture]
public class MatrixPowerExpressionTests
{
    [TestCase("pow(matrix({{1,0.5},{0,1}}),int.MinValue)[0,1]", -1073741824.0)]
    [TestCase("pow(matrix({{1,0.5},{0,1}}),-2147483648)[0,1]", -1073741824.0)]
    [TestCase("pow(matrix({{0,1},{1,0}}),int.MinValue)[0,0]", 1.0)]
    public void PortableExpressionsAcceptTheWholeSignedExponentRange(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.EqualTo(expected));

    [Test]
    public void PortableScriptUsesTheMinimumExponentWithAComplexMatrix()
    {
        const string script = """
            var m = MathNet.Numerics.LinearAlgebra.Matrix<System.Numerics.Complex>.Build.DenseOfArray(
                new System.Numerics.Complex[,] { { 1, System.Numerics.Complex.ImaginaryOne }, { 0, 1 } });
            var result = pow(m, int.MinValue);
            write(result[0,1].Real);
            write(";");
            write(result[0,1].Imaginary);
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("0;-2147483648"));
    }
}
