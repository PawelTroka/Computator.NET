using System;
using System.Linq;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes.Functions;

internal static partial class Program
{
    private static void RealChartClear()
    {
        var refreshes = new Action<Chart2D>[]
        {
            chart => chart.XMax += 1,
            chart => chart.Quality += 1,
            chart => { chart.Visible = false; chart.Visible = true; }
        };
        foreach (var refresh in refreshes)
        {
            using (var chart = new Chart2D())
            {
                chart.SetChartAreaValues(-2, 2, -1, 5);
                chart.AddFunction(new Function((Func<double, double>)(x => x), false) { Name = "previous" });
                chart.AddDataPoints(new[] { 1.0, 2.0 }, new[] { -1.0, 0.0 });
                chart.AddDataPoints(new[] { 3.0 }, new[] { 1.0 });
                Check(chart.Series.Count == 3, "Expected a function and two point series before clearing.");

                chart.ClearAll();
                Check(chart.Series.Count == 0, "ClearAll must remove visible real-chart series.");
                refresh(chart);
                Check(chart.Series.Count == 0, "Cleared real-chart points reappeared after refreshing.");

                chart.AddDataPoints(new[] { 4.0, 5.0 }, new[] { 0.25, 0.5 });
                refresh(chart);
                Check(chart.Series.Count == 1, "Only newly added real-chart data should remain.");
                Check(chart.Series[0].Points.Select(point => point.XValue).SequenceEqual(new[] { 0.25, 0.5 }) &&
                      chart.Series[0].Points.Select(point => point.YValues[0]).SequenceEqual(new[] { 4.0, 5.0 }),
                    "Fresh points must survive refreshes without restoring cleared data.");

                chart.ClearAll();
                chart.ClearAll();
                refresh(chart);
                Check(chart.Series.Count == 0, "Repeated clear must leave no retained point data.");
            }
        }
    }
}
