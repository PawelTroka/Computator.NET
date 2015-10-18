using System;
using System.Windows.Forms;

namespace Computator.NET.Charting
{
    public partial class EditChartProperties : Form
    {
        private object chart;

        public EditChartProperties(object cchart)
        {
            InitializeComponent();
            chart = cchart;
            propertyGrid1.SelectedObject = cchart;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart = propertyGrid1.SelectedObject;
            // chart.Invalidate();
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}