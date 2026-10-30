using System.IO;
using Computator.NET.Localization;
using Computator.NET.TestSupport;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    public class LocalizationSatelliteTests
    {
        [TestCase("de", "de-DE")]
        [TestCase("cs", "cs-CZ")]
        public void EveryUiStringLoadsFromItsCompiledSatelliteAndRegionalFallback(string culture, string regionalCulture)
        {
            var count = LocalizationResourceContract.Verify(typeof(Strings).Assembly,
                Path.Combine(TestContext.CurrentContext.TestDirectory, "Localization"), culture, regionalCulture);
            NUnit.Framework.Assert.That(count, Is.GreaterThan(0));
            TestContext.WriteLine("Verified {0} compiled {1} UI strings and {2} regional fallbacks.", count, culture, regionalCulture);
        }
    }
}
