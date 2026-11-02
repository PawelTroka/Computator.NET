using System.Numerics;
using Computator.NET.Core.Compilation.Builtins;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    public class UnitaryMatrixPrecompiledTests
    {
        [TestCase(1, 2, false)]
        [TestCase(2, 3, false)]
        [TestCase(3, 2, false)]
        [TestCase(1, 1, true)]
        [TestCase(2, 2, true)]
        public void ShippedScriptFacadeRequiresSquareUnitaryMatrices(int rows, int columns, bool expected)
        {
            var real = Matrix<double>.Build.DenseIdentity(rows, columns);
            var complex = Matrix<Complex>.Build.DenseIdentity(rows, columns) * Complex.ImaginaryOne;
            NUnit.Framework.Assert.That(PrecompiledScriptMathSurface.isUnitary(real), Is.EqualTo(expected));
            NUnit.Framework.Assert.That(PrecompiledScriptMathSurface.isUnitary(complex), Is.EqualTo(expected));
        }

        [Test]
        public void ShippedScriptFacadeStillRejectsSquareNonorthogonalMatrices()
        {
            var matrix = Matrix<double>.Build.DenseOfArray(new double[,] { { 1, 1 }, { 0, 1 } });
            NUnit.Framework.Assert.That(PrecompiledScriptMathSurface.isUnitary(matrix), Is.False);
        }
    }
}
