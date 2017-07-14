using Computator.NET.Core.Functions;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;

namespace Computator.NET.Tests.FunctionsTests
{
    [TestFixture]
    public class MatrixFunctionsTests
    {
        [Test]
        public void IsIndentityTest_shouldReturnFalse()
        {
            var notIdentityMatrix =
                DenseMatrix.OfArray(new[,] { { 1.1, 2, 3 }, { 1, 2, 3 } });

            Assert.IsFalse(MatrixFunctions.isIndentity(notIdentityMatrix));
        }

        [Test]
        public void IsIndentityTest_shouldReturnTrue()
        {
            var identityMatrix =
            DenseMatrix.CreateIdentity(10);

            Assert.IsTrue(MatrixFunctions.isIndentity(identityMatrix));
        }
    }
}