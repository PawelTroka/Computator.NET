using Computator.NET.Core.NumericalCalculations;
using Computator.NET.Localization;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    public class GaussKronrod61MenuTests
    {
        [Test]
        public void LocalizedDesktopMenuExposesTheSixtyOnePointRule()
        {
            var integrate = NumericalMethodsInfo.Instance.IntegrationMethods[Strings.Gauss_Kronrod_61_point_method];
            NUnit.Framework.Assert.That(integrate(x => x*x, 0, 1, 10000),
                Is.EqualTo(1.0 / 3).Within(1e-10));
        }
    }
}
