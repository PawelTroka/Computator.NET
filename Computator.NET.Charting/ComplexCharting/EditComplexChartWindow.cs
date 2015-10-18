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
            tittleOfChartTextBox.Text = chart.title;
            yLabelChartTextBox.Text = chart.yLabel;
            xLabelChartTextBox.Text = chart.xLabel;

            xMinChartTextBox.Text = chart.XMin.ToString();
            yMinChartTextBox.Text = chart.YMin.ToString();

            xMaxChartTextBox.Text = chart.XMax.ToString();
            yMaxChartTextBox.Text = chart.YMax.ToString();

            titleFont = titleFontDialog.Font = chart.titleFont;
            axesFont = axisLabelsFontDialog.Font = chart.labelsFont;
            /////////////
            contourLinesStepNumericUpDown.Value = (decimal) (chart.countourLinesStep);
            drawAxes.Checked = chart.shouldDrawAxes;


            //enums
            var vartosci =
                Enum.GetValues(typeof (CountourLinesMode)).Cast<CountourLinesMode>().ToList<CountourLinesMode>();
            foreach (var v in vartosci)
                contourLinesComboBox.Items.Add(v);
            contourLinesComboBox.SelectedItem = chart.countourMode;

            var vartosci2 =
                Enum.GetValues(typeof (AssignmentOfColorMethod))
                    .Cast<AssignmentOfColorMethod>()
                    .ToList<AssignmentOfColorMethod>();
            foreach (var v in vartosci2)
                colorAssigmentComboBox.Items.Add(v);
            colorAssigmentComboBox.SelectedItem = chart.colorAssignmentMethod;

            //colors
            axesColorRectangleShape.FillColor = chart.axesColor;
            labelsColorRectangleShape.FillColor = chart.labelsColor;
            titlesColorRectangleShape.FillColor = chart.titleColor;
        }

        private void saveData()
        {
            chart.title = tittleOfChartTextBox.Text;
            chart.yLabel = yLabelChartTextBox.Text;
            chart.xLabel = xLabelChartTextBox.Text;

            chart.YMin = double.Parse(yMinChartTextBox.Text);
            chart.YMax = double.Parse(yMaxChartTextBox.Text);

            chart.XMax = double.Parse(xMaxChartTextBox.Text);
            chart.XMin = double.Parse(xMinChartTextBox.Text);

            chart.titleFont = titleFont;
            chart.labelsFont = axesFont;

            chart.countourLinesStep = (double) (contourLinesStepNumericUpDown.Value);
            chart.shouldDrawAxes = drawAxes.Checked;


            //enums
            chart.countourMode = (CountourLinesMode) contourLinesComboBox.SelectedItem;
            chart.colorAssignmentMethod = (AssignmentOfColorMethod) colorAssigmentComboBox.SelectedItem;

            //colors
            chart.axesColor = axesColorRectangleShape.FillColor;
            chart.labelsColor = labelsColorRectangleShape.FillColor;
            chart.titleColor = titlesColorRectangleShape.FillColor;
            chart.reDraw();
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