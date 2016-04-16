using Computator.NET.Functions;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class MatrixFunctionsUnitTest
    {
        private readonly DenseMatrix _identityMatrix =
            DenseMatrix.CreateIdentity(10);

        private readonly DenseMatrix _notIdentityMatrix =
            DenseMatrix.OfArray(new[,] {{1.1, 2, 3}, {1, 2, 3}});

        [Test]
        public void IsIndentityTest_shouldReturnTrue()
        {
            Assert.IsTrue(MatrixFunctions.isIndentity(_identityMatrix));
        }

        [Test]
        public void IsIndentityTest_shouldReturnFalse()
        {
            Assert.IsFalse(MatrixFunctions.isIndentity(_notIdentityMatrix));
        }
    }
}