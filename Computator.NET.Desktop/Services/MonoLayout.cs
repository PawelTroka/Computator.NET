#if __MonoCS__
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Desktop.Services
{
    internal static class MonoLayout
    {
        // Auto-sized groups containing fill-docked tables with percentage rows
        // grow repeatedly on Mono. Size their small input grids from the actual
        // fonts instead, reserving a usable width for each numeric input.
        internal static void ConfigureNumericGroup(GroupBox group, TableLayoutPanel table)
        {
            group.AutoSize = false;
            table.AutoSize = false;
            var widths = new int[table.ColumnCount];
            var heights = new int[table.RowCount];
            var minimumInputWidth = new Size(100, 0).DpiScale().Width;
            foreach (Control control in table.Controls)
            {
                if (control is NumericUpDown number)
                {
                    number.AutoSize = false;
                    number.MinimumSize = new Size(minimumInputWidth, number.Font.Height + 7);
                    number.Size = new Size(Math.Max(minimumInputWidth, number.Width), number.PreferredHeight);
                    number.Dock = DockStyle.Fill;
                }
                var width = control is Label ? control.PreferredSize.Width : control.Width;
                var height = control is Label ? control.PreferredSize.Height : control.Height;
                var column = table.GetColumn(control);
                var row = table.GetRow(control);
                if (column >= 0) widths[column] = Math.Max(widths[column], width + control.Margin.Horizontal);
                if (row >= 0) heights[row] = Math.Max(heights[row], height + control.Margin.Vertical);
            }
            var totalWidth = group.Padding.Horizontal + 8;
            var totalHeight = group.Font.Height + group.Padding.Vertical + 6;
            for (var column = 0; column < widths.Length; column++)
            {
                var style = table.ColumnStyles[column];
                widths[column] = Math.Max(widths[column], style.SizeType == SizeType.Absolute ? (int)style.Width : 0);
                style.SizeType = SizeType.Absolute;
                style.Width = widths[column];
                totalWidth += widths[column];
            }
            for (var row = 0; row < heights.Length; row++)
            {
                var style = table.RowStyles[row];
                heights[row] = Math.Max(heights[row], style.SizeType == SizeType.Absolute ? (int)style.Height : 0);
                style.SizeType = SizeType.Absolute;
                style.Height = heights[row];
                totalHeight += heights[row];
            }
            group.Size = new Size(Math.Max(totalWidth, TextRenderer.MeasureText(group.Text, group.Font).Width + 16), totalHeight);
        }
    }
}
#endif
