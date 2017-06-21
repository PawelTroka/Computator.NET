using System;
using System.Windows.Forms;
using Computator.NET.DataTypes.Properties;

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
            this.Icon = GraphicsResources.computator_net_icon;
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