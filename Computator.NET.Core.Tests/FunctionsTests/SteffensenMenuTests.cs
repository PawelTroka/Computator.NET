using Computator.NET.Core.NumericalCalculations;
using Computator.NET.Localization;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.FunctionsTests
{
    [TestFixture]
    public class SteffensenMenuTests
    {
        [Test]
        public void TheLocalizedDesktopMenuIncludesSteffensenRootFinding()
        {
            var entry = NumericalMethodsInfo.Instance.FunctionRootMethods[Strings.Steffensen_s_method];
            NUnit.Framework.Assert.That(entry(x => x - 4, 0, 10, 1e-12, 10),
                Is.EqualTo(4).Within(1e-12));
        }
    }
}
