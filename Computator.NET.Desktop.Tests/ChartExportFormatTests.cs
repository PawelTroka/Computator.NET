using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ChartExportFormatTests
    {
        public static IEnumerable<TestCaseData> RasterCases()
        {
            foreach (var kind in ChartExportContract.ChartKinds)
                foreach (var extension in ChartExportContract.RasterExtensions)
                    yield return new TestCaseData(kind, extension);
        }

        [TestCaseSource(nameof(RasterCases))]
        public void SavedImageBytesMatchTheSelectedFormat(string kind, string extension) =>
            ChartExportContract.RoundTrip(kind, extension);

        [Test]
        public void WindowsRealChartsRetainAccuratelyNamedEnhancedMetafileExport() =>
            ChartExportContract.RoundTrip("real", "emf");

        [TestCase("real"), TestCase("complex"), TestCase("surface")]
        public void UnsupportedFormatsCannotOverwriteOrCreateFiles(string kind) =>
            ChartExportContract.UnsupportedFormatsPreserveFiles(kind);

        [Test]
        public void LocalizedPickerChoicesFollowTheRendererAndPreserveSupportedSelections() =>
            ChartExportContract.LocalizedChoicesAndModeSwitches();
    }
}
