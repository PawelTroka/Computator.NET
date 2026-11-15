using System;
using System.Linq;
using System.Threading;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class RealChartClearTests
    {
        [TestCase("range")]
        [TestCase("quality")]
        [TestCase("visibility")]
        public void ClearedPointsStayRemovedAfterRefreshAndChartReuse(string refresh)
        {
            using var chart = new Chart2D();
            chart.SetChartAreaValues(-2, 2, -1, 5);
            chart.AddFunction(new Function((Func<double, double>)(x => x), false) { Name = "previous" });
            chart.AddDataPoints(new[] { 1.0, 2.0 }, new[] { -1.0, 0.0 });
            chart.AddDataPoints(new[] { 3.0 }, new[] { 1.0 });
            NUnit.Framework.Assert.That(chart.Series.Count, Is.EqualTo(3));

            chart.ClearAll();
            NUnit.Framework.Assert.That(chart.Series.Count, Is.Zero);
            Refresh(chart, refresh);
            NUnit.Framework.Assert.That(chart.Series.Count, Is.Zero, "Cleared data must not return after a refresh.");

            chart.AddDataPoints(new[] { 4.0, 5.0 }, new[] { 0.25, 0.5 });
            Refresh(chart, refresh);
            NUnit.Framework.Assert.That(chart.Series.Count, Is.EqualTo(1), "Only the newly added data should remain.");
            NUnit.Framework.Assert.That(chart.Series[0].Points.Select(point => point.XValue), Is.EqualTo(new[] { 0.25, 0.5 }));
            NUnit.Framework.Assert.That(chart.Series[0].Points.Select(point => point.YValues[0]), Is.EqualTo(new[] { 4.0, 5.0 }));

            chart.ClearAll();
            chart.ClearAll();
            Refresh(chart, refresh);
            NUnit.Framework.Assert.That(chart.Series.Count, Is.Zero);
        }

        private static void Refresh(Chart2D chart, string operation)
        {
            switch (operation)
            {
                case "range":
                    chart.XMax += 1;
                    break;
                case "quality":
                    chart.Quality += 1;
                    break;
                case "visibility":
                    chart.Visible = false;
                    chart.Visible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }
}
