using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.TestSupport
{
    // Run the same native dialogs and Save/Cancel buttons on Windows and Mono.
    internal static class ChartEditorValidationContract
    {
        public static IEnumerable<object[]> InvalidCases
        {
            get
            {
                foreach (var complex in new[] { false, true })
                {
                    foreach (var inputName in new[] { "xMinChartTextBox", "xMaxChartTextBox", "yMinChartTextBox", "yMaxChartTextBox" })
                        yield return new object[] { complex, inputName, "" };
                    yield return new object[] { complex, "yMaxChartTextBox", "not a number" };
                    yield return new object[] { complex, "xMaxChartTextBox", "Infinity" };
                    yield return new object[] { complex, "yMinChartTextBox", "-Infinity" };
                    yield return new object[] { complex, "yMaxChartTextBox", "1e9999" };
                    yield return new object[] { complex, "xMinChartTextBox", "10" };
                    yield return new object[] { complex, "yMaxChartTextBox", "-11" };
                }
                yield return new object[] { true, "xMinChartTextBox", "NaN" };
                foreach (var inputName in new[] { "xDeltaChartTextBox", "yDeltaChartTextBox" })
                    foreach (var value in new[] { "", "invalid", "-0.5", "Infinity" })
                        yield return new object[] { false, inputName, value };
            }
        }

        public static void InvalidInputKeepsChartAndDialog(bool complex, string field, string value)
        {
            WithCulture("en-US", () =>
            {
                using (var chart = CreateChart(complex))
                using (var dialog = CreateDialog(chart, complex))
                {
                    var original = Snapshot(chart);
                    var result = ShowModal(dialog, () =>
                    {
                        EditAppearance(dialog, complex);
                        Find<TextBox>(dialog, field).Text = value;
                        dialog.AcceptButton.PerformClick();
                        Require(dialog.DialogResult == DialogResult.None && dialog.Visible,
                            "Invalid numeric input dismissed the editor.");
                        Require(Snapshot(chart) == original, "Rejected input partially changed the chart.");
                        var errors = InputErrors(dialog);
                        Require(!string.IsNullOrEmpty(errors.GetError(Find<TextBox>(dialog, field))),
                            "The rejected field has no visible validation feedback.");
                        dialog.CancelButton.PerformClick();
                    });
                    Require(result == DialogResult.Cancel, "Cancelling invalid edits did not cancel the dialog.");
                    Require(Snapshot(chart) == original, "Cancelled invalid edits changed the chart.");
                }
            });
        }

        public static void CorrectedValuesSaveTogether(bool complex, string culture)
        {
            WithCulture(culture, () =>
            {
                using (var chart = CreateChart(complex))
                using (var dialog = CreateDialog(chart, complex))
                {
                    var original = Snapshot(chart);
                    var result = ShowModal(dialog, () =>
                    {
                        EditAppearance(dialog, complex);
                        Find<TextBox>(dialog, "yMaxChartTextBox").Text = "";
                        dialog.AcceptButton.PerformClick();
                        Require(dialog.DialogResult == DialogResult.None && Snapshot(chart) == original,
                            "The first invalid Save committed or dismissed the dialog.");
                        Find<TextBox>(dialog, "xMinChartTextBox").Text = (-12.5).ToString(CultureInfo.CurrentCulture);
                        Find<TextBox>(dialog, "xMaxChartTextBox").Text = 1234.5.ToString("N1", CultureInfo.CurrentCulture);
                        Find<TextBox>(dialog, "yMinChartTextBox").Text = " -2.25e0 ".Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        Find<TextBox>(dialog, "yMaxChartTextBox").Text = "15";
                        if (!complex)
                        {
                            Find<TextBox>(dialog, "xDeltaChartTextBox").Text = "0";
                            Find<TextBox>(dialog, "yDeltaChartTextBox").Text = 0.5.ToString(CultureInfo.CurrentCulture);
                        }
                        dialog.AcceptButton.PerformClick();
                        Require(InputFields(dialog).All(input => string.IsNullOrEmpty(InputErrors(dialog).GetError(input))),
                            "Corrected fields retained stale errors.");
                    });
                    Require(result == DialogResult.OK, "Corrected settings did not save.");
                    var area = (IAreaValues)chart;
                    Require(area.XMin == -12.5 && area.XMax == 1234.5 && area.YMin == -2.25 && area.YMax == 15,
                        "Culture-aware numeric fields did not save their parsed values.");
                    Require(GetTitle(chart) == "Edited title", "Valid settings did not commit the title.");
                    if (chart is Chart2D real)
                        Require(real.LineThickness == 4 && real.PointsSize == 5 && real.ChartAreas[0].AxisX.Interval == 0 &&
                            real.ChartAreas[0].AxisY.Interval == 0.5, "Valid real chart styles/intervals did not save.");
                    else Require(!((ComplexChart)chart).ShouldDrawAxes, "Valid complex chart style did not save.");
                }
            });
        }

        public static void RealChartRetainsAutomaticAxesAndIntervals()
        {
            WithCulture("en-US", () =>
            {
                using (var chart = (Chart2D)CreateChart(false))
                {
                    chart.YMin = chart.YMax = double.NaN;
                    using (var dialog = CreateDialog(chart, false))
                    {
                        var result = ShowModal(dialog, () =>
                        {
                            Find<TextBox>(dialog, "tittleOfChartTextBox").Text = "Automatic axes";
                            Find<TextBox>(dialog, "xDeltaChartTextBox").Text = "NaN";
                            Find<TextBox>(dialog, "yDeltaChartTextBox").Text = "0";
                            dialog.AcceptButton.PerformClick();
                        });
                        Require(result == DialogResult.OK && double.IsNaN(chart.YMin) && double.IsNaN(chart.YMax),
                            "The existing automatic-axis sentinel was rejected or replaced.");
                        Require(chart.ChartAreas[0].AxisX.Interval == 0 && chart.ChartAreas[0].AxisY.Interval == 0,
                            "Automatic tick spacing was changed.");
                    }
                }
            });
        }

        public static void RunAll()
        {
            foreach (var input in InvalidCases)
                InvalidInputKeepsChartAndDialog((bool)input[0], (string)input[1], (string)input[2]);
            foreach (var complex in new[] { false, true })
                foreach (var culture in new[] { "en-US", "pl-PL", "de-DE" })
                    CorrectedValuesSaveTogether(complex, culture);
            RealChartRetainsAutomaticAxesAndIntervals();
        }

        private static Control CreateChart(bool complex)
        {
            Control chart = complex ? (Control)new ComplexChart() : new Chart2D();
            chart.Visible = false;
            chart.GetType().GetProperty("Title").SetValue(chart, "Original title", null);
            var area = (IAreaValues)chart;
            area.XMin = area.YMin = -10;
            area.XMax = area.YMax = 10;
            if (complex) ((ComplexChart)chart).ShouldDrawAxes = true;
            return chart;
        }

        private static Form CreateDialog(Control chart, bool complex) => (Form)Activator.CreateInstance(
            chart.GetType().Assembly.GetType(complex
                ? "Computator.NET.Charting.ComplexCharting.EditComplexChartWindow"
                : "Computator.NET.Charting.RealCharting.EditChartWindow", true), chart);

        private static void EditAppearance(Form dialog, bool complex)
        {
            Find<TextBox>(dialog, "tittleOfChartTextBox").Text = "Edited title";
            Find<TextBox>(dialog, "xLabelChartTextBox").Text = "Edited x";
            Find<TextBox>(dialog, "yLabelChartTextBox").Text = "Edited y";
            if (complex) Find<CheckBox>(dialog, "drawAxes").Checked = false;
            else
            {
                Find<NumericUpDown>(dialog, "lineThicknessNumericUpDown").Value = 4;
                Find<NumericUpDown>(dialog, "pointsSizeNumericUpDown").Value = 5;
            }
        }

        private static string Snapshot(Control chart)
        {
            var area = (IAreaValues)chart;
            var values = new[] { GetTitle(chart), chart.GetType().GetProperty("XLabel").GetValue(chart, null),
                chart.GetType().GetProperty("YLabel").GetValue(chart, null), area.XMin, area.XMax, area.YMin, area.YMax };
            var common = string.Join("|", values);
            if (chart is Chart2D real)
                return common + "|" + real.LineThickness + "|" + real.PointsSize + "|" +
                    real.ChartAreas[0].AxisX.Interval + "|" + real.ChartAreas[0].AxisY.Interval;
            return common + "|" + ((ComplexChart)chart).ShouldDrawAxes;
        }

        private static string GetTitle(Control chart) => (string)chart.GetType().GetProperty("Title").GetValue(chart, null);
        private static T Find<T>(Control control, string name) where T : Control => (T)control.Controls.Find(name, true).Single();
        private static IEnumerable<TextBox> InputFields(Form dialog) =>
            new[] { "xMinChartTextBox", "xMaxChartTextBox", "yMinChartTextBox", "yMaxChartTextBox", "xDeltaChartTextBox", "yDeltaChartTextBox" }
                .SelectMany(name => dialog.Controls.Find(name, true)).Cast<TextBox>();

        private static ErrorProvider InputErrors(Form dialog)
        {
            var components = (IContainer)dialog.GetType().GetField("components", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialog);
            return components.Components.OfType<ErrorProvider>().Single();
        }

        private static DialogResult ShowModal(Form dialog, Action interaction)
        {
            Exception failure = null;
            dialog.Shown += (sender, args) => dialog.BeginInvoke((Action)(() =>
            {
                try { interaction(); }
                catch (Exception error) { failure = error; }
                finally { if (dialog.DialogResult == DialogResult.None) dialog.Close(); }
            }));
            var result = dialog.ShowDialog();
            if (failure != null) throw new InvalidOperationException("Native chart editor interaction failed.", failure);
            return result;
        }

        private static void WithCulture(string culture, Action action)
        {
            var previous = Thread.CurrentThread.CurrentCulture;
            try { Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture); action(); }
            finally { Thread.CurrentThread.CurrentCulture = previous; }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
