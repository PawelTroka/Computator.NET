using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;
using EditChartWindow = Computator.NET.Charting.RealCharting.EditChartWindow;

namespace Computator.NET.Charting
{
    public partial class PlotForm : Form
    {

        public PlotForm(Control control)
        {
            InitializeComponent();
            InitializeChart(control);
        }

        private void InitializeChart(Control control)
        {
            Controls.Add(control);
            control.Dock = DockStyle.Fill;
            control.BringToFront();


            var chart3DElementHost = control as ElementHost;

            editChartMenus = new EditChartMenus(control as Chart2D, control as ComplexChart, chart3DElementHost?.Child as Chart3DControl, chart3DElementHost);
            SetMode(control.GetType());

            menuStrip1.Items.AddRange(new ToolStripItem[]
            {
                editChartMenus.chartToolStripMenuItem,
                editChartMenus.comlexChartToolStripMenuItem,
                editChartMenus.chart3DToolStripMenuItem
            });
        }

        private void SetMode(Type chartType)
        {
            if (chartType == typeof(Chart2D))
            {
                editChartMenus.SetMode(CalculationsMode.Real);
            }
            else if (chartType == typeof(ComplexChart))
            {
                editChartMenus.SetMode(CalculationsMode.Complex);
            }
            else if (chartType == typeof(ElementHost))
            {
                editChartMenus.SetMode(CalculationsMode.Fxy);
            }
        }

        private EditChartMenus editChartMenus;
    }
}