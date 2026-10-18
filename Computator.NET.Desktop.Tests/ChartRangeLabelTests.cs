using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Presenters;
using Computator.NET.DataTypes;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Views;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ChartRangeLabelTests
    {
        [Test]
        public void VisibleRangesAndAccessibleNamesFollowRealComplexAndSurfaceModes()
        {
            using var window = new Form { ClientSize = new Size(700, 500), ShowInTaskbar = false };
            using var view = new ChartAreaValuesView { Dock = DockStyle.Right };
            using var presenter = new ChartAreaValuesPresenter(view);
            window.Controls.Add(view);
            window.Show();
            view.XMin = -2.5;
            view.XMax = 12.5;
            view.YMin = -4;
            view.YMax = 6;

            foreach (var mode in new[] { CalculationsMode.Real, CalculationsMode.Complex, CalculationsMode.Fxy })
            {
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
                window.PerformLayout();
                Application.DoEvents();

                var expected = mode == CalculationsMode.Complex
                    ? new[] { "Re(z) min", "Re(z) max", "Im(z) min", "Im(z) max" }
                    : new[] { "x min", "x max", "y min", "y max" };
                var names = new[] { "x0NumericUpDown", "xnNumericUpDown", "y0NumericUpDown", "yNNumericUpDown" };
                var fields = names.Select(name => view.Controls.Find(name, true).Single()).ToArray();
                NUnit.Framework.Assert.That(fields.Select(field => field.AccessibilityObject.Name), Is.EqualTo(expected));
                NUnit.Framework.Assert.That(new[] { view.XMin, view.XMax, view.YMin, view.YMax },
                    Is.EqualTo(new double[] { -2.5, 12.5, -4, 6 }));

                var table = (TableLayoutPanel)view.Controls.Find("tableLayoutPanel1", true).Single();
                foreach (var field in fields)
                    NUnit.Framework.Assert.That(table.ClientRectangle.Contains(field.Bounds), Is.True,
                        $"The longer labels must keep the entire {field.AccessibleName} input visible.");
                var labels = table.Controls.OfType<AxisRangeLabel>().ToArray();
                NUnit.Framework.Assert.That(labels.Select(label => label.AccessibilityObject.Name), Is.EquivalentTo(expected));
                foreach (var label in labels)
                {
                    NUnit.Framework.Assert.That(label.Width, Is.GreaterThanOrEqualTo(label.PreferredSize.Width));
                    NUnit.Framework.Assert.That(label.Height, Is.GreaterThanOrEqualTo(label.PreferredSize.Height));
                    NUnit.Framework.Assert.That(table.ClientRectangle.Contains(label.Bounds), Is.True,
                        $"{label.Text} must remain within the bounds panel.");
                    NUnit.Framework.Assert.That(fields.Any(field => field.Bounds.IntersectsWith(label.Bounds)), Is.False,
                        $"{label.Text} must not overlap any range input.");
                }

                using var image = new Bitmap(view.Width, view.Height);
                image.SetResolution(view.DeviceDpi, view.DeviceDpi);
                view.DrawToBitmap(image, view.ClientRectangle);
                SaveEvidence(image, "chart-range-" + mode + ".png");
            }
        }

        [TestCase(10.2f)]
        [TestCase(15.3f)]
        [TestCase(20.4f)]
        public void SubscriptInkFitsItsMeasuredLabelAtLargerTextSizes(float pointSize)
        {
            using var font = new Font("Cambria", pointSize, GraphicsUnit.Point);
            using var label = new AxisRangeLabel
            {
                CoordinateName = "Re(z)", IsMaximum = true, Font = font,
                AutoSize = true, ForeColor = Color.Black, BackColor = Color.White
            };
            label.Size = label.GetPreferredSize(Size.Empty);
            label.CreateControl();
            using var image = new Bitmap(label.Width, label.Height);
            image.SetResolution(label.DeviceDpi, label.DeviceDpi);
            label.DrawToBitmap(image, label.ClientRectangle);

            int darkPixels = 0;
            for (int y = 0; y < image.Height; y++)
            for (int x = 0; x < image.Width; x++)
            {
                var color = image.GetPixel(x, y);
                if (color.R > 160 || color.G > 160 || color.B > 160)
                    continue;
                darkPixels++;
                NUnit.Framework.Assert.That(x, Is.GreaterThan(0).And.LessThan(image.Width - 1),
                    "Measured width must include the full base, subscript and equals sign.");
                NUnit.Framework.Assert.That(y, Is.GreaterThan(0).And.LessThan(image.Height - 1),
                    "Lowering the subscript must not clip its glyphs.");
            }
            NUnit.Framework.Assert.That(darkPixels, Is.GreaterThan(30), "The label must actually paint readable text.");
            SaveEvidence(image, "chart-range-text-" + pointSize.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + ".png");
        }

        private static void SaveEvidence(Bitmap image, string filename)
        {
            var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (root != null && !File.Exists(Path.Combine(root.FullName, "Computator.NET.slnx")))
                root = root.Parent;
            if (root == null)
                throw new DirectoryNotFoundException("Cannot locate the test checkout for rendering evidence.");
            var directory = Path.Combine(root.FullName, "TestResults", "chart-range-labels");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, filename);
            image.Save(path, ImageFormat.Png);
            TestContext.AddTestAttachment(path, "Actual WinForms chart range labels");
        }
    }
}
