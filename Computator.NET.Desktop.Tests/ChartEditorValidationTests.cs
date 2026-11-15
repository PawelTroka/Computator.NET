using System.Threading;
using Computator.NET.TestSupport;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ChartEditorValidationTests
    {
        [TestCaseSource(typeof(ChartEditorValidationContract), nameof(ChartEditorValidationContract.InvalidCases))]
        public void InvalidInputKeepsChartAndDialog(bool complex, string field, string value) =>
            ChartEditorValidationContract.InvalidInputKeepsChartAndDialog(complex, field, value);

        [TestCase(false, "en-US")]
        [TestCase(true, "en-US")]
        [TestCase(false, "pl-PL")]
        [TestCase(true, "pl-PL")]
        [TestCase(false, "de-DE")]
        [TestCase(true, "de-DE")]
        public void CorrectedValuesSaveTogether(bool complex, string culture) =>
            ChartEditorValidationContract.CorrectedValuesSaveTogether(complex, culture);

        [Test]
        public void RealChartRetainsAutomaticAxesAndIntervals() =>
            ChartEditorValidationContract.RealChartRetainsAutomaticAxesAndIntervals();
    }
}
