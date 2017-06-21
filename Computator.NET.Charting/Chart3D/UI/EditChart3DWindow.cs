using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using Computator.NET.DataTypes.Properties;
#if !__MonoCS__
using System.Windows.Forms.Integration;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using Point = System.Windows.Point;
#endif

namespace Computator.NET.Charting.Chart3D.UI
{
    public partial class EditChart3DWindow : Form
    {
#if !__MonoCS__
        private readonly Chart3DControl chart3d;
        private readonly ElementHost elementHost;
        private Color colorAxes;
        private Color colorBackground;
        private Color colorLabels; //, colorSpline;
        private Font font;
        private bool fontChanged;

        public EditChart3DWindow(Chart3DControl chart3d, ElementHost elementHost)
        {
            InitializeComponent();
            this.chart3d = chart3d;
            this.elementHost = elementHost;

            chartTypeComboBox.Items.AddRange(Enum.GetNames(typeof(Chart3DMode)));

            loadData();
            this.Icon = GraphicsResources.computator_net_icon;
        }

        private void loadData()
        {
            chartTypeComboBox.SelectedItem = chart3d.Mode.ToString();

            //bools:
            equalAxesCheckBox.Checked = chart3d.EqualAxes;
            visibleAxesCheckBox.Checked = chart3d.VisibilityAxes;

            labelsVisibilityCheckBox.Checked = chart3d.AxisLabels.ActiveLabels;
            xActiveCheckBox.Checked = chart3d.AxisLabels.ActiveXLabel;
            yActiveCheckBox.Checked = chart3d.AxisLabels.ActiveYLabel;
            zActiveCheckBox.Checked = chart3d.AxisLabels.ActiveZLabel;

            //floating point vars:
            scaleNumericUpDown.Value = (decimal) chart3d.Scale;
            dotSizeNumericUpDown.Value = (decimal) chart3d.DotSize;

            xOffsetNumericUpDown.Value = (decimal) chart3d.AxisLabels.Offset.X;
            yOffsetNumericUpDown.Value = (decimal) chart3d.AxisLabels.Offset.Y;

            xMinChartTextBox.Text = chart3d.XMin.ToString(CultureInfo.InvariantCulture);
            xMaxChartTextBox.Text = chart3d.XMax.ToString(CultureInfo.InvariantCulture);

            yMinChartTextBox.Text = chart3d.YMin.ToString(CultureInfo.InvariantCulture);
            yMaxChartTextBox.Text = chart3d.YMax.ToString(CultureInfo.InvariantCulture);

            //strings:
            xLabelTextBox.Text = chart3d.AxisLabels.LabelX;
            yLabelTextBox.Text = chart3d.AxisLabels.LabelY;
            zLabelTextBox.Text = chart3d.AxisLabels.LabelZ;


            //colors:
            colorLabels = chart3d.AxisLabels.Color;
            labelsColorDialog.Color = drawingColorToMedia(chart3d.AxisLabels.Color);
            labelsColorRectangleShape.FillColor = drawingColorToMedia(chart3d.AxisLabels.Color);


            colorAxes = chart3d.AxesColor;
            axesColorDialog.Color = drawingColorToMedia(chart3d.AxesColor);
            axesColorRectangleShape.FillColor = drawingColorToMedia(chart3d.AxesColor);


            backgroundColorDialog.Color = elementHost.BackColor;
            backgroundColorRectangleShape.FillColor = elementHost.BackColor;
            colorBackground = Color.FromArgb(elementHost.BackColor.A, elementHost.BackColor.R, elementHost.BackColor.G,
                elementHost.BackColor.B);
        }

        private void saveData()
        {
            chart3d.Mode = (Chart3DMode) Enum.Parse(typeof(Chart3DMode), chartTypeComboBox.SelectedItem.ToString());
            //bools:
            chart3d.EqualAxes = equalAxesCheckBox.Checked;
            chart3d.VisibilityAxes = visibleAxesCheckBox.Checked;

            chart3d.AxisLabels.ActiveLabels = labelsVisibilityCheckBox.Checked;
            chart3d.AxisLabels.ActiveXLabel = xActiveCheckBox.Checked;
            chart3d.AxisLabels.ActiveYLabel = yActiveCheckBox.Checked;
            chart3d.AxisLabels.ActiveZLabel = zActiveCheckBox.Checked;


            //floating point values
            chart3d.Scale = (double) scaleNumericUpDown.Value;
            chart3d.DotSize = (double) dotSizeNumericUpDown.Value*unitsComboBox.MultiplierRelativToSI;

            chart3d.AxisLabels.Offset = new Point((double) xOffsetNumericUpDown.Value,
                (double) yOffsetNumericUpDown.Value);


            chart3d.XMin = double.Parse(xMinChartTextBox.Text, CultureInfo.InvariantCulture);
            chart3d.XMax = double.Parse(xMaxChartTextBox.Text, CultureInfo.InvariantCulture);

            chart3d.YMin = double.Parse(yMinChartTextBox.Text, CultureInfo.InvariantCulture);
            chart3d.YMax = double.Parse(yMaxChartTextBox.Text, CultureInfo.InvariantCulture);


            //strings:
            chart3d.AxisLabels.LabelX = xLabelTextBox.Text;
            chart3d.AxisLabels.LabelY = yLabelTextBox.Text;
            chart3d.AxisLabels.LabelZ = zLabelTextBox.Text;


            //colors:
            chart3d.AxisLabels.Color = colorLabels;
            chart3d.AxesColor = colorAxes;
            elementHost.BackColor = System.Drawing.Color.FromArgb(colorBackground.A, colorBackground.R,
                colorBackground.G, colorBackground.B);

            //fonts:
            if (fontChanged)
            {
                var fs = convertFontStyleToWindowsFontStyle(font.Style);

                chart3d.AxisLabels.setProporties(
                    new FontFamily(font.FontFamily.ToString().Replace("[FontFamily: Name=", "").Replace("]", "")),
                    font.Size, colorLabels, fs, null);
            }
        }

        private FontStyle convertFontStyleToWindowsFontStyle(System.Drawing.FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case System.Drawing.FontStyle.Bold:
                    return FontStyles.Oblique;

                case System.Drawing.FontStyle.Italic:
                    return FontStyles.Italic;

                case System.Drawing.FontStyle.Regular:
                    return FontStyles.Normal;

                case System.Drawing.FontStyle.Strikeout:
                    break;

                case System.Drawing.FontStyle.Underline:
                    break;

                case System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Bold:


                default:
                    return FontStyles.Normal;
            }
            return FontStyles.Normal;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            saveData();
            Close();
        }

        private void anulujButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void visibleAxesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            chart3d.VisibilityAxes = visibleAxesCheckBox.Checked;
        }

        private void equalAxesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            chart3d.EqualAxes = equalAxesCheckBox.Checked;
        }

        private void backgroundColorRectangleShape_Click(object sender, EventArgs e)
        {
            var result = backgroundColorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                colorBackground = Color.FromArgb(backgroundColorDialog.Color.A, backgroundColorDialog.Color.R,
                    backgroundColorDialog.Color.G, backgroundColorDialog.Color.B);
                backgroundColorRectangleShape.FillColor = backgroundColorDialog.Color;
            }
        }

        private void labelsColorRectangleShape_Click(object sender, EventArgs e)
        {
            var result = labelsColorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                colorLabels = Color.FromArgb(labelsColorDialog.Color.A, labelsColorDialog.Color.R,
                    labelsColorDialog.Color.G, labelsColorDialog.Color.B);
                labelsColorRectangleShape.FillColor = labelsColorDialog.Color;
            }
        }

        private void axesColorRectangleShape_Click(object sender, EventArgs e)
        {
            var result = axesColorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                colorAxes = Color.FromArgb(axesColorDialog.Color.A, axesColorDialog.Color.R, axesColorDialog.Color.G,
                    axesColorDialog.Color.B);
                axesColorRectangleShape.FillColor = axesColorDialog.Color;
            }
        }

        private Color mediaColorToDrawing(System.Drawing.Color c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        private System.Drawing.Color drawingColorToMedia(Color c)
        {
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var result = labelsFontDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                font = labelsFontDialog.Font;
                fontChanged = true;
            }
        }
#else

        private void backgroundColorRectangleShape_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void axesColorRectangleShape_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void anulujButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void labelsColorRectangleShape_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void visibleAxesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void equalAxesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
#endif
    }
}