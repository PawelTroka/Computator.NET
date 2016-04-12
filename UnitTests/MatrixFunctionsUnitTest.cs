using Computator.NET.Functions;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class MatrixFunctionsUnitTest
    {
        private readonly DenseMatrix _identityMatrix =
            DenseMatrix.CreateIdentity(10);

        private readonly DenseMatrix _notIdentityMatrix =
            DenseMatrix.OfArray(new[,] {{1.1, 2, 3}, {1, 2, 3}});

        [TestMethod]
        public void IsIndentityTest_shouldReturnTrue()
        {
            Assert.IsTrue(MatrixFunctions.isIndentity(_identityMatrix));
        }

        [TestMethod]
        public void IsIndentityTest_shouldReturnFalse()
        {
            Assert.IsFalse(MatrixFunctions.isIndentity(_notIdentityMatrix));
        }
    }
}