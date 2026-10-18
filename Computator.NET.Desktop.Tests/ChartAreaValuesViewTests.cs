using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Views;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ChartAreaValuesViewTests
    {
        [TestCase("en", "Minimum real part of z", "Maximum imaginary part of z")]
        [TestCase("pl", "Minimalna wartość części rzeczywistej z", "Maksymalna wartość części urojonej z")]
        public void VisibleLabelsAndAccessibleInputsFollowBothAxisChanges(string culture,
            string minimumRealName, string maximumImaginaryName)
        {
            var previousCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
                using var view = new ChartAreaValuesView();
                view.XMin = -12;
                view.YMax = 17;
                AssertLabels(view, "x", "y");

                view.XAxisLabel = "Re(z)";
                view.YAxisLabel = "Im(z)";
                AssertLabels(view, "Re(z)", "Im(z)");
                NUnit.Framework.Assert.That(Find(view, "x0NumericUpDown").AccessibilityObject.Name,
                    Is.EqualTo(minimumRealName));
                NUnit.Framework.Assert.That(Find(view, "yNNumericUpDown").AccessibilityObject.Name,
                    Is.EqualTo(maximumImaginaryName));

                view.XAxisLabel = "x";
                view.YAxisLabel = "y";
                AssertLabels(view, "x", "y");
                NUnit.Framework.Assert.That(view.XMin, Is.EqualTo(-12));
                NUnit.Framework.Assert.That(view.YMax, Is.EqualTo(17));
            }
            finally { CultureInfo.CurrentUICulture = previousCulture; }
        }

        [TestCase(1f)]
        [TestCase(1.5f)]
        [TestCase(2f)]
        public void SidebarAccommodatesComplexBoundsAtScaledFontAndControlSizes(float scale)
        {
            using var host = new Form { ClientSize = new Size(1000, 750), AutoScaleMode = AutoScaleMode.None };
            using var view = new ChartAreaValuesView { Dock = DockStyle.Right };
            var fonts = new List<Font>();
            try
            {
                host.Controls.Add(view);
                _ = host.Handle;
                _ = view.Handle;
                var minimumWidths = Descendants(view).OfType<ScientificNumericUpDown>()
                    .ToDictionary(input => (Control)input, input => input.MinimumSize.Width);
                view.Scale(new SizeF(scale, scale));
                foreach (var label in Descendants(view).OfType<ChartAreaBoundLabel>())
                {
                    var font = new Font(label.Font.FontFamily, 10.2f * scale);
                    fonts.Add(font);
                    label.Font = font;
                }
                foreach (var input in Descendants(view).OfType<ScientificNumericUpDown>())
                {
                    var font = new Font(input.Font.FontFamily, 9.75f * scale);
                    fonts.Add(font);
                    ((Control)input).Font = font;
                }
                var table = (TableLayoutPanel)Find(view, "tableLayoutPanel1");
                foreach (var complex in new[] { true, false, true })
                {
                    view.XAxisLabel = complex ? "Re(z)" : "x";
                    view.YAxisLabel = complex ? "Im(z)" : "y";
                    host.PerformLayout();
                    view.PerformLayout();
                    table.PerformLayout();

                    foreach (var label in Descendants(view).OfType<ChartAreaBoundLabel>())
                    {
                        NUnit.Framework.Assert.That(label.Width, Is.GreaterThanOrEqualTo(label.PreferredSize.Width));
                        NUnit.Framework.Assert.That(label.Height, Is.GreaterThanOrEqualTo(label.PreferredSize.Height));
                        NUnit.Framework.Assert.That(table.ClientRectangle.Contains(label.Bounds), Is.True,
                            $"The full label {label.Text} must fit inside the bounds panel.");
                        var row = table.GetRow(label);
                        var input = table.GetControlFromPosition(2, row);
                        NUnit.Framework.Assert.That(input.Left, Is.GreaterThanOrEqualTo(label.Right));
                        NUnit.Framework.Assert.That(input.Width,
                            Is.GreaterThanOrEqualTo(Math.Floor(minimumWidths[input] * scale)));
                        NUnit.Framework.Assert.That(table.ClientRectangle.Contains(input.Bounds), Is.True);
                    }
                }

                using var bitmap = new Bitmap(view.Width, view.Height);
                view.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
                var path = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                    $"chart-area-complex-labels-{(int)(scale * 100)}-percent.png");
                bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                TestContext.AddTestAttachment(path, "Complex bounds with scaled labels and numeric inputs");
            }
            finally
            {
                view.Dispose();
                foreach (var font in fonts) font.Dispose();
            }
        }

        [Test]
        public void PaintedMinimumIsBelowTheAxisBaseline()
        {
            using var font = new Font("Cambria", 24f);
            using var label = new ChartAreaBoundLabel("x", "min")
            {
                Font = font, BackColor = Color.White, ForeColor = Color.Black
            };
            label.Size = label.PreferredSize;
            using var bitmap = new Bitmap(label.Width, label.Height);
            label.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));

            // Separate ink by its blank columns, without duplicating the
            // renderer's font measurements or subscript-position calculation.
            // The first band is x, the last is =, and min lies between them.
            var bands = InkBands(bitmap).ToArray();
            NUnit.Framework.Assert.That(bands.Length, Is.GreaterThanOrEqualTo(3),
                "The rendered label must include an axis, a bound, and an equals sign.");
            var subscriptBottom = bands.Skip(1).Take(bands.Length - 2).Max(band => band.Bottom);
            NUnit.Framework.Assert.That(subscriptBottom, Is.GreaterThan(bands[0].Bottom),
                "The min letters must be visibly lowered, not ordinary baseline text.");
        }

        private static IEnumerable<Rectangle> InkBands(Bitmap bitmap)
        {
            var start = -1;
            var top = bitmap.Height;
            var bottom = 0;
            for (var x = 0; x <= bitmap.Width; x++)
            {
                var hasInk = false;
                if (x < bitmap.Width)
                    for (var y = 0; y < bitmap.Height; y++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        if (pixel.R >= 160 || pixel.G >= 160 || pixel.B >= 160) continue;
                        hasInk = true;
                        top = Math.Min(top, y);
                        bottom = Math.Max(bottom, y + 1);
                    }
                if (hasInk && start < 0) start = x;
                else if (!hasInk && start >= 0)
                {
                    yield return Rectangle.FromLTRB(start, top, x, bottom);
                    start = -1;
                    top = bitmap.Height;
                    bottom = 0;
                }
            }
        }

        private static void AssertLabels(Control view, string xAxis, string yAxis)
        {
            NUnit.Framework.Assert.That(Find(view, "xMinLabel").Text, Is.EqualTo($"{xAxis} min ="));
            NUnit.Framework.Assert.That(Find(view, "xMaxLabel").Text, Is.EqualTo($"{xAxis} max ="));
            NUnit.Framework.Assert.That(Find(view, "yMinLabel").Text, Is.EqualTo($"{yAxis} min ="));
            NUnit.Framework.Assert.That(Find(view, "yMaxLabel").Text, Is.EqualTo($"{yAxis} max ="));
        }

        private static Control Find(Control root, string name) => root.Controls.Find(name, true).Single();

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }
    }
}
