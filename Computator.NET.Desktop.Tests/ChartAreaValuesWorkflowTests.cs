using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Model;
using Computator.NET.Core.Presenters;
using Computator.NET.DataTypes;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Views;
using Computator.NET.Localization;
using Moq;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ChartAreaValuesWorkflowTests
    {
        [TestCase(CalculationsMode.Real, "x", "y")]
        [TestCase(CalculationsMode.Fxy, "x", "y")]
        [TestCase(CalculationsMode.Complex, "Re(z)", "Im(z)")]
        public void InitialLabelsUseTheCurrentMode(CalculationsMode mode, string horizontal, string vertical)
        {
            using var view = new ChartAreaValuesView();
            using var presenter = new ChartAreaValuesPresenter(view, State(mode));
            AssertLabels(view, horizontal, vertical);
            NUnit.Framework.Assert.That(Find<Button>(view, "addToChartButton").Text,
                Is.EqualTo(mode == CalculationsMode.Complex ? Strings.DrawChart : Strings.AddToChart));
        }

        [Test]
        public void SwitchingModesUpdatesEveryLabelWithoutChangingValuesOrRaisingValueEvents()
        {
            using var view = new ChartAreaValuesView();
            using var presenter = new ChartAreaValuesPresenter(view, State(CalculationsMode.Real));
            view.XMin = -12;
            view.XMax = 7;
            view.YMin = -4;
            view.YMax = 9;
            var valueChanges = 0;
            view.XMinChanged += (_, _) => valueChanges++;
            view.XMaxChanged += (_, _) => valueChanges++;
            view.YMinChanged += (_, _) => valueChanges++;
            view.YMaxChanged += (_, _) => valueChanges++;

            foreach (var mode in new[] { CalculationsMode.Complex, CalculationsMode.Fxy, CalculationsMode.Real,
                         CalculationsMode.Complex })
            {
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
                AssertLabels(view, mode == CalculationsMode.Complex ? "Re(z)" : "x",
                    mode == CalculationsMode.Complex ? "Im(z)" : "y");
                NUnit.Framework.Assert.That(new[] { view.XMin, view.XMax, view.YMin, view.YMax },
                    Is.EqualTo(new[] { -12.0, 7, -4, 9 }));
            }
            NUnit.Framework.Assert.That(valueChanges, Is.Zero);
        }

        [TestCase("en")]
        [TestCase("pl")]
        [TestCase("de")]
        [TestCase("cs")]
        public void ExistingRangeErrorsFollowTheModeAndKeepLocalizedExplanations(string culture)
        {
            var previousCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                using var view = new ChartAreaValuesView();
                using var presenter = new ChartAreaValuesPresenter(view, State(CalculationsMode.Real));
                var errors = (ErrorProvider)typeof(ChartAreaValuesView)
                    .GetField("_errorProvider", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
                view.XMin = 6;
                view.YMax = -4;
                NUnit.Framework.Assert.That(errors.GetError(Find<Control>(view, "x0NumericUpDown")),
                    Is.EqualTo($"x min {Strings.NeedsToBeLessThan} x max"));

                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Complex));
                NUnit.Framework.Assert.That(errors.GetError(Find<Control>(view, "x0NumericUpDown")),
                    Is.EqualTo($"Re(z) min {Strings.NeedsToBeLessThan} Re(z) max"));
                NUnit.Framework.Assert.That(errors.GetError(Find<Control>(view, "xnNumericUpDown")),
                    Is.EqualTo($"Re(z) max {Strings.NeedsToBeGreaterThan} Re(z) min"));
                NUnit.Framework.Assert.That(errors.GetError(Find<Control>(view, "y0NumericUpDown")),
                    Is.EqualTo($"Im(z) min {Strings.NeedsToBeLessThan} Im(z) max"));
                NUnit.Framework.Assert.That(errors.GetError(Find<Control>(view, "yNNumericUpDown")),
                    Is.EqualTo($"Im(z) max {Strings.NeedsToBeGreaterThan} Im(z) min"));

                view.XMin = -5;
                view.YMax = 3;
                foreach (var name in InputNames)
                    NUnit.Framework.Assert.That(errors.GetError(Find<Control>(view, name)), Is.Empty);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previousCulture;
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DisposedPresenterOrViewNoLongerReceivesUpdates(bool disposeView)
        {
            var view = new Mock<IChartAreaValuesView>();
            view.SetupAllProperties();
            var presenter = new ChartAreaValuesPresenter(view.Object, State(CalculationsMode.Real));
            if (disposeView) view.Raise(value => value.Disposed += null, EventArgs.Empty);
            else presenter.Dispose();
            view.Invocations.Clear();

            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Complex));
            view.Raise(value => value.XMinChanged += null, EventArgs.Empty);
            view.VerifyNoOtherCalls();
        }

        [TestCase(1.0f)]
        [TestCase(1.5f)]
        [TestCase(2.0f)]
        public void ActualRangeControlsFitAndRenderAtScaledSizes(float scale)
        {
            using var view = new ChartAreaValuesView { Dock = DockStyle.Right };
            using var presenter = new ChartAreaValuesPresenter(view, State(CalculationsMode.Real));
            using var window = new Form { ClientSize = new Size(800, 800), ShowInTaskbar = false };
            window.Controls.Add(view);
            window.Show();
            view.Scale(new SizeF(scale, scale));

            foreach (var mode in new[] { CalculationsMode.Real, CalculationsMode.Complex, CalculationsMode.Fxy })
            {
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
                window.PerformLayout();
                view.PerformLayout();
                Application.DoEvents();
                // Retain the actual frame even if a following containment check fails.
                using var bitmap = new Bitmap(view.Width, view.Height);
                view.DrawToBitmap(bitmap, view.ClientRectangle);
                var path = Path.Combine(FindRepositoryRoot(), "TestResults", "chart-area-labels",
                    $"{mode.ToString().ToLowerInvariant()}-{(int)(scale * 100)}percent.png");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                bitmap.Save(path, ImageFormat.Png);
                TestContext.AddTestAttachment(path,
                    "Actual WinForms controls; font/layout scaling, not physical monitor DPI switching");

                var table = Find<TableLayoutPanel>(view, "tableLayoutPanel1");
                foreach (var name in LabelNames)
                {
                    var label = Find<ChartRangeLabel>(view, name);
                    var preferred = label.GetPreferredSize(Size.Empty);
                    NUnit.Framework.Assert.That(label.Width, Is.GreaterThanOrEqualTo(preferred.Width), name);
                    NUnit.Framework.Assert.That(label.Height, Is.GreaterThanOrEqualTo(preferred.Height), name);
                    NUnit.Framework.Assert.That(table.ClientRectangle.Contains(label.Bounds), Is.True, name);
                }
                foreach (var name in InputNames)
                {
                    var input = Find<ScientificNumericUpDown>(view, name);
                    NUnit.Framework.Assert.That(input.Width, Is.GreaterThanOrEqualTo(input.MinimumSize.Width), name);
                    // Leave room for the existing range-validation icon on the right.
                    NUnit.Framework.Assert.That(input.Right + 16 * scale,
                        Is.LessThanOrEqualTo(table.ClientSize.Width), name);
                }
            }
        }

        private static readonly string[] LabelNames = { "xMinLabel", "xMaxLabel", "yMinLabel", "yMaxLabel" };
        private static readonly string[] InputNames = { "x0NumericUpDown", "xnNumericUpDown", "y0NumericUpDown", "yNNumericUpDown" };

        private static void AssertLabels(ChartAreaValuesView view, string horizontal, string vertical)
        {
            var axes = new[] { horizontal, horizontal, vertical, vertical };
            for (var i = 0; i < LabelNames.Length; i++)
            {
                var label = Find<ChartRangeLabel>(view, LabelNames[i]);
                var input = Find<ScientificNumericUpDown>(view, InputNames[i]);
                NUnit.Framework.Assert.That(label.Text, Is.EqualTo(axes[i]));
                NUnit.Framework.Assert.That(label.IsMaximum, Is.EqualTo(i % 2 == 1));
                var accessibleName = $"{axes[i]} {(i % 2 == 1 ? "max" : "min")}";
                NUnit.Framework.Assert.That(label.AccessibilityObject.Name, Is.EqualTo(accessibleName));
                NUnit.Framework.Assert.That(input.AccessibilityObject.Name, Is.EqualTo(accessibleName));
            }
        }

        private static ISharedViewState State(CalculationsMode mode)
        {
            var state = new Mock<ISharedViewState>();
            state.SetupGet(value => value.CalculationsMode).Returns(mode);
            return state.Object;
        }

        private static T Find<T>(Control view, string name) where T : Control =>
            (T)view.Controls.Find(name, true).Single();

        private static string FindRepositoryRoot()
        {
            for (var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                 directory != null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "Computator.NET.slnx")))
                    return directory.FullName;
            throw new DirectoryNotFoundException("Cannot locate repository for range-label rendering evidence");
        }
    }
}
