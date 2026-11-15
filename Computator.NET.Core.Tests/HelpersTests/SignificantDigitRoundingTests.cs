using System;
using Computator.NET.Core.Helpers;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.HelpersTests
{
    [TestFixture]
    public class SignificantDigitRoundingTests
    {
        [Test]
        public void ValuesNearLargePowersOfTenDoNotOverflowDuringMagnitudeDetection()
        {
            const decimal justBelowTenTo28 = 9999999999999999999999999999m;

            NUnit.Framework.Assert.That(justBelowTenTo28.RoundToSignificantDigits(28),
                Is.EqualTo(justBelowTenTo28));
            NUnit.Framework.Assert.That(justBelowTenTo28.RoundToSignificantDigits(1),
                Is.EqualTo(10000000000000000000000000000m));
            NUnit.Framework.Assert.That(decimal.MaxValue.RoundToSignificantDigits(29),
                Is.EqualTo(decimal.MaxValue));
        }

        [Test]
        public void RoundsAroundDecimalPowerBoundariesWithoutDoubleConversion()
        {
            NUnit.Framework.Assert.That(0.00995m.RoundToSignificantDigits(2), Is.EqualTo(0.010m));
            NUnit.Framework.Assert.That(999.5m.RoundToSignificantDigits(3), Is.EqualTo(1000m));
            NUnit.Framework.Assert.That(0.0000000000000000000000000001m.RoundToSignificantDigits(2),
                Is.EqualTo(0.0000000000000000000000000001m));
            NUnit.Framework.Assert.That(0m.RoundToSignificantDigits(3), Is.EqualTo(0m));
        }

        [Test]
        public void PreservesMidpointToEvenForPositiveAndNegativeValues()
        {
            NUnit.Framework.Assert.That(2.45m.RoundToSignificantDigits(2), Is.EqualTo(2.4m));
            NUnit.Framework.Assert.That(2.55m.RoundToSignificantDigits(2), Is.EqualTo(2.6m));
            NUnit.Framework.Assert.That((-2.45m).RoundToSignificantDigits(2), Is.EqualTo(-2.4m));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(30)]
        public void RejectsUnsupportedSignificantDigitCounts(int digits)
        {
            NUnit.Framework.Assert.That(
                () => 1.234m.RoundToSignificantDigits(digits),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
