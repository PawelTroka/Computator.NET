using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace ComplexPlotter.ComplexCharting
{
    partial class EditComplexChartProperties : Form
    {
        public EditComplexChartProperties(ComplexChart cchart)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US", true);
            InitializeComponent();
            chart = cchart;
            propertyGrid1.SelectedObject = cchart;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart = (ComplexChart)propertyGrid1.SelectedObject;
            chart.Invalidate();
            this.Close();
        }


        ComplexChart chart;

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
