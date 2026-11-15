using System;
using System.ComponentModel;
using System.Drawing;
using Computator.NET.Charting.Controls;
using System.Windows.Forms;
using Computator.NET.DataTypes.Properties;

namespace Computator.NET.Charting.RealCharting
{
    partial class EditChartWindow : Form
    {
        private readonly Chart2D chart;
        private Font axesFont, legendFont;
        private Font titleFont;
        private Font valuesFont;
        private readonly ChartEditorValidation validation;

        public EditChartWindow()
        {
            InitializeComponent();
            validation = new ChartEditorValidation(this, components ?? (components = new Container()));
            this.Icon = GraphicsResources.computator_net_icon;
        }

        public EditChartWindow(Chart2D chart)
        {
            InitializeComponent();
            validation = new ChartEditorValidation(this, components ?? (components = new Container()));
            this.chart = chart;
            loadData();
            this.Icon = GraphicsResources.computator_net_icon;
        }

        private void loadData()
        {
            lineThicknessNumericUpDown.Value = chart.LineThickness;
            pointsSizeNumericUpDown.Value = chart.PointsSize;
            tittleOfChartTextBox.Text = chart.Titles[0].Text;
            yLabelChartTextBox.Text = chart.ChartAreas[0].AxisY.Title;
            xLabelChartTextBox.Text = chart.ChartAreas[0].AxisX.Title;

            xMinChartTextBox.Text = chart.XMin.ToString();
            yMinChartTextBox.Text = chart.YMin.ToString();

            xMaxChartTextBox.Text = chart.XMax.ToString();
            yMaxChartTextBox.Text = chart.YMax.ToString();

            yDeltaChartTextBox.Text = chart.ChartAreas[0].AxisY.Interval.ToString();
            xDeltaChartTextBox.Text = chart.ChartAreas[0].AxisX.Interval.ToString();

            titleFont = titleFontDialog.Font = chart.Titles[0].Font;
            axesFont = axisLabelsFontDialog.Font = chart.ChartAreas[0].AxisX.TitleFont;
            legendFont = legendFontDialog.Font = chart.Legends[0].Font;
            valuesFont = valuesFontDialog.Font = chart.ChartAreas[0].AxisX.LabelStyle.Font;

            if (chart.Series.Count > 0)
                legendaEtykieta1.Text = chart.Series[0].LegendText;
            if (chart.Series.Count > 1)
                legendaEtykieta2.Text = chart.Series[1].LegendText;
            if (chart.Series.Count > 2)
                legendaEtykieta3.Text = chart.Series[2].LegendText;
            if (chart.Series.Count > 3)
                legendaEtykieta4.Text = chart.Series[3].LegendText;
            if (chart.Series.Count > 0)
                legendVisible.Checked = chart.Series[0].IsVisibleInLegend;
        }

        private bool saveData()
        {
            validation.Clear();
            var xMin = validation.ReadNumber(xMinChartTextBox, true);
            var xMax = validation.ReadNumber(xMaxChartTextBox, true);
            var yMin = validation.ReadNumber(yMinChartTextBox, true);
            var yMax = validation.ReadNumber(yMaxChartTextBox, true);
            var xInterval = validation.ReadInterval(xDeltaChartTextBox);
            var yInterval = validation.ReadInterval(yDeltaChartTextBox);
            validation.ValidateRange(xMinChartTextBox, xMin, xMaxChartTextBox, xMax);
            validation.ValidateRange(yMinChartTextBox, yMin, yMaxChartTextBox, yMax);
            if (!validation.CanApply()) return false;

            chart.LineThickness = (int) lineThicknessNumericUpDown.Value;
            chart.PointsSize = (int) pointsSizeNumericUpDown.Value;
            chart.Titles[0].Text = tittleOfChartTextBox.Text;
            chart.ChartAreas[0].AxisY.Title = yLabelChartTextBox.Text;
            chart.ChartAreas[0].AxisX.Title = xLabelChartTextBox.Text;

            chart.XMin = xMin;
            chart.YMin = yMin;

            chart.XMax = xMax;
            chart.YMax = yMax;

            chart.ChartAreas[0].AxisY.Interval = yInterval;
            chart.ChartAreas[0].AxisX.Interval = xInterval;

            chart.Titles[0].Font = titleFont;
            chart.ChartAreas[0].AxisY.TitleFont = chart.ChartAreas[0].AxisX.TitleFont = axesFont;
            chart.Legends[0].Font = legendFont;
            chart.ChartAreas[0].AxisX.LabelStyle.Font = valuesFont;
            chart.ChartAreas[0].AxisY.LabelStyle.Font = valuesFont;

            if (chart.Series.Count > 0)
                chart.Series[0].LegendText = legendaEtykieta1.Text;
            if (chart.Series.Count > 1)
                chart.Series[1].LegendText = legendaEtykieta2.Text;
            if (chart.Series.Count > 2)
                chart.Series[2].LegendText = legendaEtykieta3.Text;
            if (chart.Series.Count > 3)
                chart.Series[3].LegendText = legendaEtykieta4.Text;
            foreach (var serie in chart.Series)
                serie.IsVisibleInLegend = legendVisible.Checked;
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!saveData()) return;
            chart.Invalidate();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var result = titleFontDialog.ShowDialog();
            // See if OK was pressed.
            if (result == DialogResult.OK)
                titleFont = titleFontDialog.Font;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var result = axisLabelsFontDialog.ShowDialog();
            // See if OK was pressed.
            if (result == DialogResult.OK)
                axesFont = axisLabelsFontDialog.Font;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var result = legendFontDialog.ShowDialog();
            // See if OK was pressed.
            if (result == DialogResult.OK)
                legendFont = legendFontDialog.Font;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var result = valuesFontDialog.ShowDialog();
            // See if OK was pressed.
            if (result == DialogResult.OK)
                valuesFont = valuesFontDialog.Font;
        }
    }
}
