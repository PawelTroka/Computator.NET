using System.Numerics;
using Computator.NET.Core.Functions;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class PointwiseMatrixTests
{
    [TestCase(false, 1)]
    [TestCase(false, 2)]
    [TestCase(false, 3)]
    [TestCase(true, 1)]
    [TestCase(true, 2)]
    [TestCase(true, 3)]
    public void RealFoldsUseEachOperandOnceAndPreserveInputs(bool sparse, int count)
    {
        var inputs = new[] {
            Create(new double[,] { { 0, -6, 8 }, { 2, -4, 6 } }, sparse),
            Create(new double[,] { { 2, -3, 4 }, { 0.5, 2, 3 } }, sparse),
            Create(new double[,] { { 3, 2, -2 }, { 4, -1, 0.5 } }, sparse)
        }.Take(count).ToArray();
        var originals = inputs.Select(matrix => matrix.ToArray()).ToArray();
        var expectedProduct = count switch {
            1 => originals[0],
            2 => new double[,] { { 0, 18, 32 }, { 1, -8, 18 } },
            _ => new double[,] { { 0, 36, -64 }, { 4, 8, 9 } }
        };
        var expectedQuotient = count switch {
            1 => originals[0],
            2 => new double[,] { { 0, 2, 2 }, { 4, -2, 2 } },
            _ => new double[,] { { 0, 1, -1 }, { 1, 2, 4 } }
        };

        var product = MatrixFunctions.PointwiseMultiply(inputs);
        var quotient = MatrixFunctions.PointwiseDivide(inputs);
        NUnit.Framework.Assert.That(product.ToArray(), Is.EqualTo(expectedProduct).Within(1e-12));
        NUnit.Framework.Assert.That(quotient.ToArray(), Is.EqualTo(expectedQuotient).Within(1e-12));

        // Even a single-operand fold returns an independent matrix, including zeros.
        product[0, 0] = 123;
        quotient[0, 0] = 456;
        for (var index = 0; index < count; index++)
            NUnit.Framework.Assert.That(inputs[index].ToArray(), Is.EqualTo(originals[index]));
    }

    [TestCase(false, 1)]
    [TestCase(false, 2)]
    [TestCase(false, 3)]
    [TestCase(true, 1)]
    [TestCase(true, 2)]
    [TestCase(true, 3)]
    public void ComplexFoldsUseEachOperandOnceAndPreserveInputs(bool sparse, int count)
    {
        var inputs = new[] {
            Create(new[,] { { new Complex(2, 1), Complex.Zero, new Complex(-3, 2) } }, sparse),
            Create(new[,] { { new Complex(1, -1), new Complex(2, 1), new Complex(0, 2) } }, sparse),
            Create(new[,] { { Complex.ImaginaryOne, -Complex.One, new Complex(-1, 1) } }, sparse)
        }.Take(count).ToArray();
        var originals = inputs.Select(matrix => matrix.ToArray()).ToArray();
        var expectedProduct = count switch {
            1 => originals[0],
            2 => new[,] { { new Complex(3, -1), Complex.Zero, new Complex(-4, -6) } },
            _ => new[,] { { new Complex(1, 3), Complex.Zero, new Complex(10, 2) } }
        };
        var expectedQuotient = count switch {
            1 => originals[0],
            2 => new[,] { { new Complex(0.5, 1.5), Complex.Zero, new Complex(1, 1.5) } },
            _ => new[,] { { new Complex(1.5, -0.5), Complex.Zero, new Complex(0.25, -1.25) } }
        };

        var product = MatrixFunctions.PointwiseMultiply(inputs);
        var quotient = MatrixFunctions.PointwiseDivide(inputs);
        for (var column = 0; column < 3; column++)
        {
            NUnit.Framework.Assert.That(Complex.Abs(product[0, column] - expectedProduct[0, column]), Is.LessThan(1e-12));
            NUnit.Framework.Assert.That(Complex.Abs(quotient[0, column] - expectedQuotient[0, column]), Is.LessThan(1e-12));
        }

        product[0, 0] = 123;
        quotient[0, 0] = 456;
        for (var index = 0; index < count; index++)
            NUnit.Framework.Assert.That(inputs[index].ToArray(), Is.EqualTo(originals[index]));
    }

    [TestCase("PointwiseMultiply(matrix({{2}}),matrix({{3}}))[0,0]", 6.0)]
    [TestCase("PointwiseDivide(matrix({{6}}),matrix({{2}}))[0,0]", 3.0)]
    [TestCase("PointwiseMultiply(matrix({{2}}),matrix({{3}}),matrix({{4}}))[0,0]", 24.0)]
    [TestCase("PointwiseDivide(matrix({{24}}),matrix({{3}}),matrix({{4}}))[0,0]", 2.0)]
    [TestCase("PointwiseDivide(matrix({{0,6}}))[0,0]", 0.0)]
    public void PortableExpressionsUseTheCorrectedFolds(string expression, double expected) =>
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(expression), Is.EqualTo(expected).Within(1e-12));

    private static Matrix<T> Create<T>(T[,] values, bool sparse)
        where T : struct, IEquatable<T>, IFormattable =>
        sparse ? Matrix<T>.Build.SparseOfArray(values) : Matrix<T>.Build.DenseOfArray(values);
}
