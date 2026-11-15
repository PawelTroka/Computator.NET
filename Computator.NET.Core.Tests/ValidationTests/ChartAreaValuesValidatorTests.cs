using Computator.NET.Core.Validation;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ValidationTests
{
    [TestFixture]
    class ChartAreaValuesValidatorTests
    {
        [Test]
        public void AllWrong_IsNotValid()
        {
            Assert.IsFalse(ChartAreaValuesValidator.IsValid(99,0,99,0));
        }
        [Test]
        public void AllSame_IsNotValid()
        {
            Assert.IsFalse(ChartAreaValuesValidator.IsValid(1, 1, 1, 1));
        }
        [Test]
        public void XMinGreaterThanXMax_IsNotValid()
        {
            Assert.IsFalse(ChartAreaValuesValidator.IsValid(222, 1, 1, 2));
        }
        [Test]
        public void YMinGreaterThanYMax_IsNotValid()
        {
            Assert.IsFalse(ChartAreaValuesValidator.IsValid(1, 2, 222, 0));
        }
        [Test]
        public void YMinSameAsYMax_IsNotValid()
        {
            Assert.IsFalse(ChartAreaValuesValidator.IsValid(1, 2, 222, 222));
        }
        [Test]
        public void XMinSameAsXMax_IsNotValid()
        {
            Assert.IsFalse(ChartAreaValuesValidator.IsValid(22, 22, 111, 222));
        }
        [Test]
        public void XMinLessThanXMaxAndYMinLessThanYMax_IsValid()
        {
            Assert.IsTrue(ChartAreaValuesValidator.IsValid(-2, 0, -2, 0));
        }

        [TestCase(double.NegativeInfinity, 1, -1, 1)]
        [TestCase(0, double.PositiveInfinity, -1, 1)]
        [TestCase(0, 1, double.NegativeInfinity, 1)]
        [TestCase(0, 1, -1, double.PositiveInfinity)]
        [TestCase(double.NaN, 1, -1, 1)]
        [TestCase(0, double.NaN, -1, 1)]
        [TestCase(0, 1, double.NaN, 1)]
        [TestCase(0, 1, -1, double.NaN)]
        public void NonFiniteChartLimits_AreNotValid(double xmin, double xmax, double ymin, double ymax)
        {
            Assert.IsFalse(ChartAreaValuesValidator.IsValid(xmin, xmax, ymin, ymax));
        }
    }
}
