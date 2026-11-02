using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.DataTypes.Text;
using Computator.NET.Desktop.Views;

internal static partial class Program
{
    private static void NumericInputsFitActualFonts()
    {
        var fonts = new List<Font>();
        try
        {
            using (var window = new Form { Text = "Computator.NET numeric font layout acceptance",
                ClientSize = new Size(980, 720), StartPosition = FormStartPosition.Manual, Location = new Point(40, 40) })
            using (var ranges = new ChartAreaValuesView { Dock = DockStyle.Fill })
            {
                window.Controls.Add(ranges);
                window.Show();
                Application.DoEvents();
                var numbers = new[] { "x0NumericUpDown", "xnNumericUpDown", "y0NumericUpDown", "yNNumericUpDown" }
                    .Select(name => (NumericUpDown)ranges.Controls.Find(name, true).Single()).ToArray();
                var group = ranges.Controls.Find("groupBox2", true).Single();
                // Exercise the same real font fallback at larger and smaller
                // sizes. No DPI clamp, replacement font or assertion reduction
                // can conceal a fill-docked row that clips its actual text.
                var stage = 0;
                foreach (var size in new[] { 9.75f, 12f, 16f, 9.75f })
                {
                    var font = CustomFonts.GetMathFont(size);
                    fonts.Add(font);
                    foreach (var number in numbers) number.Font = font;
                    ranges.XAxisLabel = "x";
                    ranges.YAxisLabel = "y";
                    Application.DoEvents();
                    Capture(window, "numeric-input-font-" + (++stage) + "-" + size.ToString(CultureInfo.InvariantCulture) + ".png");
                    foreach (var number in numbers)
                    {
                        using (var graphics = number.CreateGraphics())
                        Console.WriteLine("Numeric layout: " + number.Name + " " + number.Bounds +
                            " client=" + number.ClientSize + " font=" + number.Font.Name + " family=" + number.Font.FontFamily.Name +
                            " size=" + number.Font.Size + " unit=" + number.Font.Unit + " dpi=" + graphics.DpiX + "," + graphics.DpiY +
                            " fontHeight=" + number.Font.Height + " preferred=" + number.PreferredHeight);
                        Check(number.Visible && number.Enabled && number.ClientSize.Width >= 60 &&
                            number.ClientSize.Height >= number.Font.Height,
                            "Numeric input clips its actual font: " + number.Name + " " + number.Bounds +
                            " client=" + number.ClientSize + " fontHeight=" + number.Font.Height);
                        var rectangle = number.RectangleToScreen(number.ClientRectangle);
                        for (var ancestor = number.Parent; ancestor != null; ancestor = ancestor.Parent)
                            Check(ancestor.RectangleToScreen(ancestor.ClientRectangle).Contains(rectangle),
                                "Numeric input is clipped by " + ancestor.GetType().Name + ": " + number.Name);
                    }
                    var stable = group.Size;
                    for (var repeat = 0; repeat < 6; repeat++)
                    {
                        ranges.XAxisLabel = "x";
                        ranges.YAxisLabel = "y";
                        Application.DoEvents();
                        Check(group.Size == stable, "Repeated Mono numeric layout grew its supposedly fixed input grid.");
                    }
                }
                window.Close();
            }
        }
        finally
        {
            foreach (var font in fonts) font.Dispose();
        }
    }
}
