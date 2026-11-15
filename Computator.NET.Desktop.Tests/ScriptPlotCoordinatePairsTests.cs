using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ScriptPlotCoordinatePairsTests
    {
        private static readonly double[][] Coordinates =
        {
            new[] { 1.0, 2.0 }, new[] { 10.0, 20.0 },
            new[] { -40.0, -20.0, -10.0 }, new[] { -1.0, 1.0, 2.0 },
            new[] { 0.5 }, new[] { -30.0 }
        };

        [TestCase(false, 0)]
        [TestCase(true, 0)]
        [TestCase(false, 1)]
        [TestCase(true, 1)]
        [TestCase(false, 2)]
        [TestCase(true, 2)]
        [TestCase(false, 3)]
        [TestCase(true, 3)]
        public void MixedPlotPreservesTheFunctionAndEachOrderedCoordinatePair(bool generated, int pairs)
        {
            var sequences = Coordinates.Take(2 * pairs).Cast<IEnumerable<double>>().ToArray();
            WithNewWindows(() => Plot(generated, sequences), windows =>
            {
                NUnit.Framework.Assert.That(windows.Length, Is.EqualTo(1));
                var chart = windows[0].Controls.OfType<Chart2D>().Single();
                NUnit.Framework.Assert.That(chart.Series.Count, Is.EqualTo(1 + pairs),
                    "Each x/y pair must contribute exactly one point series alongside the function.");
                for (var pair = 0; pair < pairs; pair++)
                    AssertPoints(chart, pair + 1, Coordinates[2 * pair], Coordinates[2 * pair + 1]);

                // The existing automatic axes do not provide a finite function-sampling range.
                // Setting that range must reveal the retained function without changing the pairs.
                chart.SetChartAreaValues(-50, 50, -100, 100);
                chart.Quality = 1; // Refresh sampled functions with a finite range.
                NUnit.Framework.Assert.That(chart.Series.Count, Is.EqualTo(1 + pairs));
                NUnit.Framework.Assert.That(chart.Series[0].Points.Count, Is.GreaterThan(0));
                foreach (var point in chart.Series[0].Points)
                    NUnit.Framework.Assert.That(point.YValues[0], Is.EqualTo(2 * point.XValue));
                for (var pair = 0; pair < pairs; pair++)
                    AssertPoints(chart, pair + 1, Coordinates[2 * pair], Coordinates[2 * pair + 1]);
            });
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TwoSequencePlotUsesTheSameCoordinateOrder(bool generated)
        {
            WithNewWindows(() =>
            {
                if (generated)
                    ExecuteScript("plot(" + Sequence(Coordinates[0]) + "," + Sequence(Coordinates[1]) + ");");
                else ScriptingFunctions.plot(Coordinates[0], Coordinates[1]);
            }, windows =>
            {
                NUnit.Framework.Assert.That(windows.Length, Is.EqualTo(1));
                var chart = windows[0].Controls.OfType<Chart2D>().Single();
                NUnit.Framework.Assert.That(chart.Series.Count, Is.EqualTo(1));
                AssertPoints(chart, 0, Coordinates[0], Coordinates[1]);
            });
        }

        [TestCase(false, -1)]
        [TestCase(true, -1)]
        [TestCase(false, 1)]
        [TestCase(true, 1)]
        [TestCase(false, 3)]
        [TestCase(true, 3)]
        public void MissingCoordinatePairsAreRejectedBeforeOpeningAPlot(bool generated, int sequenceCount)
        {
            var sequences = sequenceCount < 0 ? null :
                Coordinates.Take(sequenceCount).Cast<IEnumerable<double>>().ToArray();
            WithNewWindows(() =>
            {
                Exception error = null;
                try { Plot(generated, sequences); }
                catch (Exception caught) { error = caught; }
                // ScriptFunction preserves the original exception as its inner cause.
                while (error != null && error.InnerException != null) error = error.InnerException;
                NUnit.Framework.Assert.That(error, Is.TypeOf(sequenceCount < 0 ?
                    typeof(ArgumentNullException) : typeof(ArgumentException)));
                NUnit.Framework.Assert.That(((ArgumentException)error).ParamName, Is.EqualTo("xys"));
            }, windows => NUnit.Framework.Assert.That(windows, Is.Empty));
        }

        private static void Plot(bool generated, IEnumerable<double>[] sequences)
        {
            if (generated)
            {
                var argument = sequences == null ? "null" :
                    "new System.Collections.Generic.IEnumerable<double>[] {" +
                    string.Join(",", sequences.Select(Sequence)) + "}";
                ExecuteScript("plot((real t) => 2*t, (System.Collections.Generic.IEnumerable<double>[])" + argument + ");");
            }
            else ScriptingFunctions.plot(t => 2 * t, sequences);
        }

        private static string Sequence(IEnumerable<double> values) =>
            "new double[] {" + string.Join(",", values.Select(value => value.ToString("R", CultureInfo.InvariantCulture))) + "}";

        private static void ExecuteScript(string source) =>
            new ScriptEvaluator().Evaluate(source, "").Evaluate(_ => { });

        private static void AssertPoints(Chart2D chart, int series, double[] x, double[] y)
        {
            var points = chart.Series[series].Points;
            NUnit.Framework.Assert.That(points.Count, Is.EqualTo(x.Length));
            for (var i = 0; i < points.Count; i++)
            {
                NUnit.Framework.Assert.That(points[i].XValue, Is.EqualTo(x[i]));
                NUnit.Framework.Assert.That(points[i].YValues[0], Is.EqualTo(y[i]));
            }
        }

        private static void WithNewWindows(Action action, Action<Form[]> inspect)
        {
            var previous = new HashSet<Form>(Application.OpenForms.Cast<Form>());
            var directory = Directory.GetCurrentDirectory();
            try
            {
                action();
                Application.DoEvents();
                inspect(Application.OpenForms.Cast<Form>().Where(window => !previous.Contains(window)).ToArray());
            }
            finally
            {
                foreach (var window in Application.OpenForms.Cast<Form>().Where(window => !previous.Contains(window)).ToArray())
                {
                    window.Close();
                    window.Dispose();
                }
                Directory.SetCurrentDirectory(directory);
            }
        }
    }
}
