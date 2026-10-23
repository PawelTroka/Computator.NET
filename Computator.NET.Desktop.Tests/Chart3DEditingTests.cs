using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.Controls;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes.Functions;
using Computator.NET.Desktop.Views;
using NUnit.Framework;
using MediaColor = System.Windows.Media.Color;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class Chart3DEditingTests
    {
        [TestCase(0)]
        [TestCase(128)]
        [TestCase(255)]
        public void ColorSwatchPreservesAlphaAndCompositesOnlyOnce(int alpha)
        {
            var fill = Color.FromArgb(alpha, 208, 144, 64);
            using var parent = new Panel { BackColor = Color.FromArgb(32, 64, 96), Size = new Size(80, 80) };
            using var swatch = new ColorSwatch { FillColor = fill, Bounds = new Rectangle(10, 10, 40, 40) };
            parent.Controls.Add(swatch);
            parent.CreateControl();
            swatch.CreateControl();

            NUnit.Framework.Assert.That(swatch.FillColor.ToArgb(), Is.EqualTo(fill.ToArgb()));
            NUnit.Framework.Assert.That(swatch.BackColor.ToArgb(), Is.EqualTo(fill.ToArgb()));
            AssertPaintedColor(swatch, parent.BackColor, fill);

            // Transparent previews must reflect their new parent background,
            // without flattening or accumulating another layer on repaint.
            parent.BackColor = Color.FromArgb(224, 192, 160);
            AssertPaintedColor(swatch, parent.BackColor, fill);
            NUnit.Framework.Assert.That(swatch.FillColor.ToArgb(), Is.EqualTo(fill.ToArgb()));
        }

        [Test]
        public void ChartingViewInTransparentTabCanOpenSaveAndReopen3DEditor()
        {
            var chart = new Chart3DControl();
            using var view = new ChartingView(new ChartAreaValuesView(), new Chart2D(), new ComplexChart(), chart);
            using var window = new Form { ShowInTaskbar = false, ClientSize = new Size(900, 700) };
            using var tabs = new TabControl { Dock = DockStyle.Fill };
            // MainView's themed TabPage returns this color under visual styles.
            // Make it explicit so the regression also runs without a themed desktop.
            using var page = new TabPage { BackColor = Color.Transparent };
            page.Controls.Add(view);
            tabs.TabPages.Add(page);
            window.Controls.Add(tabs);
            window.Show();
            PlotSurface(chart);

            var host = chart.ParentControl;
            NUnit.Framework.Assert.That(host, Is.Not.Null);
            NUnit.Framework.Assert.That(host.BackColor.A, Is.Zero,
                "The real ChartingView must inherit the tab's transparent background.");
            var background = host.BackColor.ToArgb();
            using (var dialog = new EditChart3DWindow(chart, host))
            {
                dialog.Show(window);
                NUnit.Framework.Assert.That(Find<ColorSwatch>(dialog, "backgroundColorRectangleShape")
                    .FillColor.ToArgb(), Is.EqualTo(background));
                Find<TextBox>(dialog, "xLabelTextBox").Text = "Edited x axis";
                dialog.AcceptButton.PerformClick();
            }

            NUnit.Framework.Assert.That(chart.AxisLabels.LabelX, Is.EqualTo("Edited x axis"));
            NUnit.Framework.Assert.That(host.BackColor.ToArgb(), Is.EqualTo(background));
            using (var dialog = new EditChart3DWindow(chart, host))
            {
                dialog.Show(window);
                NUnit.Framework.Assert.That(Find<TextBox>(dialog, "xLabelTextBox").Text,
                    Is.EqualTo("Edited x axis"));
                Find<TextBox>(dialog, "xLabelTextBox").Text = "Discarded change";
                dialog.CancelButton.PerformClick();
            }

            NUnit.Framework.Assert.That(chart.AxisLabels.LabelX, Is.EqualTo("Edited x axis"));
            NUnit.Framework.Assert.That(host.BackColor.ToArgb(), Is.EqualTo(background));
        }

        [TestCase(0)]
        [TestCase(128)]
        [TestCase(255)]
        public void SavingOtherSettingsPreservesBackgroundAxesAndLabelAlpha(int alpha)
        {
            var chart = new Chart3DControl
            {
                AxesColor = MediaColor.FromArgb((byte)alpha, 40, 80, 120)
            };
            chart.AxisLabels.Color = MediaColor.FromArgb((byte)alpha, 160, 100, 60);
            var axesColor = chart.AxesColor;
            var labelsColor = chart.AxisLabels.Color;
            var background = Color.FromArgb(alpha, 80, 120, 160);
            using var host = new ElementHost { Child = chart, BackColor = background };
            PlotSurface(chart);
            using var dialog = new EditChart3DWindow(chart, host);
            dialog.Show();

            NUnit.Framework.Assert.That(Find<ColorSwatch>(dialog, "axesColorRectangleShape").FillColor.A,
                Is.EqualTo(alpha));
            NUnit.Framework.Assert.That(Find<ColorSwatch>(dialog, "labelsColorRectangleShape").FillColor.A,
                Is.EqualTo(alpha));
            Find<NumericUpDown>(dialog, "scaleNumericUpDown").Value = 0.75m;
            dialog.AcceptButton.PerformClick();

            NUnit.Framework.Assert.That(chart.Scale, Is.EqualTo(0.75));
            NUnit.Framework.Assert.That(host.BackColor.ToArgb(), Is.EqualTo(background.ToArgb()));
            NUnit.Framework.Assert.That(chart.AxesColor, Is.EqualTo(axesColor));
            NUnit.Framework.Assert.That(chart.AxisLabels.Color, Is.EqualTo(labelsColor));
        }

        [TestCase(true, false, DialogResult.OK)]
        [TestCase(true, false, DialogResult.Cancel)]
        [TestCase(true, false, DialogResult.None)]
        [TestCase(false, true, DialogResult.OK)]
        [TestCase(false, true, DialogResult.Cancel)]
        [TestCase(false, true, DialogResult.None)]
        public void Modal3DEditorPreservesLivePreviewAndCommitsOnlyOnSave(
            bool visibleAxes, bool equalAxes, DialogResult closeAction)
        {
            var chart = new Chart3DControl();
            var background = Color.FromArgb(0, 80, 120, 160);
            var axesColor = MediaColor.FromArgb(128, 40, 80, 120);
            var labelsColor = MediaColor.FromArgb(64, 160, 100, 60);
            using var host = new ElementHost { Child = chart, BackColor = background };
            PlotSurface(chart);
            chart.VisibilityAxes = visibleAxes;
            chart.EqualAxes = equalAxes;
            chart.AxesColor = axesColor;
            chart.AxisLabels.Color = labelsColor;
            chart.AxisLabels.LabelX = "Original x axis";
            chart.Scale = 0.5;

            using (var dialog = new EditChart3DWindow(chart, host))
            {
                var result = ShowModalEditor(dialog, () =>
                {
                    NUnit.Framework.Assert.That(Find<CheckBox>(dialog, "visibleAxesCheckBox").Checked,
                        Is.EqualTo(visibleAxes));
                    NUnit.Framework.Assert.That(Find<CheckBox>(dialog, "equalAxesCheckBox").Checked,
                        Is.EqualTo(equalAxes));
                    Find<CheckBox>(dialog, "visibleAxesCheckBox").Checked = !visibleAxes;
                    Find<CheckBox>(dialog, "equalAxesCheckBox").Checked = !equalAxes;
                    Find<TextBox>(dialog, "xLabelTextBox").Text = "Edited x axis";
                    Find<NumericUpDown>(dialog, "scaleNumericUpDown").Value = 0.75m;

                    NUnit.Framework.Assert.That(chart.VisibilityAxes, Is.EqualTo(!visibleAxes),
                        "Axis visibility must still preview while the editor is open.");
                    NUnit.Framework.Assert.That(chart.EqualAxes, Is.EqualTo(!equalAxes),
                        "Equal-axis scaling must still preview while the editor is open.");
                    NUnit.Framework.Assert.That(chart.AxisLabels.LabelX, Is.EqualTo("Original x axis"));
                    NUnit.Framework.Assert.That(chart.Scale, Is.EqualTo(0.5));

                    if (closeAction == DialogResult.OK)
                        dialog.AcceptButton.PerformClick();
                    else if (closeAction == DialogResult.Cancel)
                        dialog.CancelButton.PerformClick();
                    else
                        dialog.Close(); // The same close path as the title-bar close button.
                });
                NUnit.Framework.Assert.That(result,
                    Is.EqualTo(closeAction == DialogResult.OK ? DialogResult.OK : DialogResult.Cancel));
            }

            var saved = closeAction == DialogResult.OK;
            var expectedVisibility = saved ? !visibleAxes : visibleAxes;
            var expectedEqualAxes = saved ? !equalAxes : equalAxes;
            var expectedLabel = saved ? "Edited x axis" : "Original x axis";
            var expectedScale = saved ? 0.75 : 0.5;
            NUnit.Framework.Assert.That(chart.VisibilityAxes, Is.EqualTo(expectedVisibility));
            NUnit.Framework.Assert.That(chart.EqualAxes, Is.EqualTo(expectedEqualAxes));
            NUnit.Framework.Assert.That(chart.AxisLabels.LabelX, Is.EqualTo(expectedLabel));
            NUnit.Framework.Assert.That(chart.Scale, Is.EqualTo(expectedScale));
            NUnit.Framework.Assert.That(host.BackColor.ToArgb(), Is.EqualTo(background.ToArgb()));
            NUnit.Framework.Assert.That(chart.AxesColor, Is.EqualTo(axesColor));
            NUnit.Framework.Assert.That(chart.AxisLabels.Color, Is.EqualTo(labelsColor));

            using var reopened = new EditChart3DWindow(chart, host);
            ShowModalEditor(reopened, () =>
            {
                NUnit.Framework.Assert.That(Find<CheckBox>(reopened, "visibleAxesCheckBox").Checked,
                    Is.EqualTo(expectedVisibility));
                NUnit.Framework.Assert.That(Find<CheckBox>(reopened, "equalAxesCheckBox").Checked,
                    Is.EqualTo(expectedEqualAxes));
                NUnit.Framework.Assert.That(Find<TextBox>(reopened, "xLabelTextBox").Text,
                    Is.EqualTo(expectedLabel));
                NUnit.Framework.Assert.That(Find<NumericUpDown>(reopened, "scaleNumericUpDown").Value,
                    Is.EqualTo((decimal)expectedScale));
                NUnit.Framework.Assert.That(Find<ColorSwatch>(reopened, "backgroundColorRectangleShape")
                    .FillColor.ToArgb(), Is.EqualTo(background.ToArgb()));
                NUnit.Framework.Assert.That(Find<ColorSwatch>(reopened, "axesColorRectangleShape")
                    .FillColor.ToArgb(), Is.EqualTo(Color.FromArgb(128, 40, 80, 120).ToArgb()));
                NUnit.Framework.Assert.That(Find<ColorSwatch>(reopened, "labelsColorRectangleShape")
                    .FillColor.ToArgb(), Is.EqualTo(Color.FromArgb(64, 160, 100, 60).ToArgb()));
            });
        }

        private static DialogResult ShowModalEditor(EditChart3DWindow dialog, Action edit)
        {
            Exception failure = null;
            var callbackRan = false;
            dialog.Shown += (_, _) => dialog.BeginInvoke(new Action(() =>
            {
                callbackRan = true;
                try
                {
                    edit();
                }
                catch (Exception exception)
                {
                    // Rethrow outside the native modal loop so assertion failures
                    // cannot become swallowed UI exceptions or leave the test open.
                    failure = exception;
                }
                finally
                {
                    if (dialog.DialogResult == DialogResult.None)
                        dialog.Close();
                }
            }));

            var result = dialog.ShowDialog();
            if (failure != null)
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
            NUnit.Framework.Assert.That(callbackRan, Is.True, "The modal editor callback must run.");
            return result;
        }

        private static T Find<T>(Control parent, string name) where T : Control
        {
            return parent.Controls.Find(name, true).OfType<T>().Single();
        }

        private static void PlotSurface(Chart3DControl chart)
        {
            // Editing a plotted surface matches the reported workflow and
            // initializes the chart's projection/data before saving settings.
            chart.Quality = 0;
            chart.AddFunction(new Function(new Func<double, double, double>((x, y) => x + y),
                FunctionType.Real3D));
        }

        private static void AssertPaintedColor(ColorSwatch swatch, Color background, Color fill)
        {
            using var bitmap = new Bitmap(swatch.Width, swatch.Height);
            swatch.DrawToBitmap(bitmap, swatch.ClientRectangle);
            var actual = bitmap.GetPixel(swatch.Width / 2, swatch.Height / 2);
            // Independent straight-alpha composition; allow GDI+ rounding.
            NUnit.Framework.Assert.That(actual.R,
                Is.EqualTo((fill.R * fill.A + background.R * (255 - fill.A)) / 255.0).Within(1));
            NUnit.Framework.Assert.That(actual.G,
                Is.EqualTo((fill.G * fill.A + background.G * (255 - fill.A)) / 255.0).Within(1));
            NUnit.Framework.Assert.That(actual.B,
                Is.EqualTo((fill.B * fill.A + background.B * (255 - fill.A)) / 255.0).Within(1));
        }
    }
}
