using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Computator.NET.Charting.ComplexCharting
{
    public partial class EditComplexChartWindow : Form
    {
        private readonly ComplexChart chart;
        private Font axesFont, legendFont;
        private Font titleFont;
        private Font valuesFont;

        public EditComplexChartWindow()
        {
            InitializeComponent();
        }

        public EditComplexChartWindow(ComplexChart chart)
        {
            InitializeComponent();
            this.chart = chart;
            loadData();
        }

        private void loadData()
        {
            tittleOfChartTextBox.Text = chart.Title;
            yLabelChartTextBox.Text = chart.YLabel;
            xLabelChartTextBox.Text = chart.XLabel;

            xMinChartTextBox.Text = chart.XMin.ToString();
            yMinChartTextBox.Text = chart.YMin.ToString();

            xMaxChartTextBox.Text = chart.XMax.ToString();
            yMaxChartTextBox.Text = chart.YMax.ToString();

            titleFont = titleFontDialog.Font = chart.TitleFont;
            axesFont = axisLabelsFontDialog.Font = chart.LabelsFont;
            /////////////
            contourLinesStepNumericUpDown.Value = (decimal) chart.CountourLinesStep;
            drawAxes.Checked = chart.ShouldDrawAxes;


            //enums
            var vartosci =
                Enum.GetValues(typeof(CountourLinesMode)).Cast<CountourLinesMode>().ToList();
            foreach (var v in vartosci)
                contourLinesComboBox.Items.Add(v);
            contourLinesComboBox.SelectedItem = chart.CountourMode;

            var vartosci2 =
                Enum.GetValues(typeof(AssignmentOfColorMethod))
                    .Cast<AssignmentOfColorMethod>()
                    .ToList();
            foreach (var v in vartosci2)
                colorAssigmentComboBox.Items.Add(v);
            colorAssigmentComboBox.SelectedItem = chart.ColorAssignmentMethod;

            //colors
            axesColorRectangleShape.FillColor = chart.AxesColor;
            labelsColorRectangleShape.FillColor = chart.LabelsColor;
            titlesColorRectangleShape.FillColor = chart.TitleColor;
        }

        private void saveData()
        {
            chart.Title = tittleOfChartTextBox.Text;
            chart.YLabel = yLabelChartTextBox.Text;
            chart.XLabel = xLabelChartTextBox.Text;

            chart.YMin = double.Parse(yMinChartTextBox.Text);
            chart.YMax = double.Parse(yMaxChartTextBox.Text);

            chart.XMax = double.Parse(xMaxChartTextBox.Text);
            chart.XMin = double.Parse(xMinChartTextBox.Text);

            chart.TitleFont = titleFont;
            chart.LabelsFont = axesFont;

            chart.CountourLinesStep = (double) contourLinesStepNumericUpDown.Value;
            chart.ShouldDrawAxes = drawAxes.Checked;


            //enums
            chart.CountourMode = (CountourLinesMode) contourLinesComboBox.SelectedItem;
            chart.ColorAssignmentMethod = (AssignmentOfColorMethod) colorAssigmentComboBox.SelectedItem;

            //colors
            chart.AxesColor = axesColorRectangleShape.FillColor;
            chart.LabelsColor = labelsColorRectangleShape.FillColor;
            chart.TitleColor = titlesColorRectangleShape.FillColor;
            chart.Redraw();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveData();
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

        private void axesColorRectangleShape_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = axesColorRectangleShape.FillColor;
            var result = colorDialog1.ShowDialog();
            {
                if (result == DialogResult.OK)
                    axesColorRectangleShape.FillColor = colorDialog1.Color;
            }
        }

        private void labelsColorRectangleShape_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = labelsColorRectangleShape.FillColor;
            var result = colorDialog1.ShowDialog();
            {
                if (result == DialogResult.OK)
                    labelsColorRectangleShape.FillColor = colorDialog1.Color;
            }
        }

        private void titlesColorRectangleShape_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = titlesColorRectangleShape.FillColor;
            var result = colorDialog1.ShowDialog();
            {
                if (result == DialogResult.OK)
                    titlesColorRectangleShape.FillColor = colorDialog1.Color;
            }
        }
    }
}