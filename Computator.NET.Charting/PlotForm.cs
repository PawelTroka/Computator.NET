using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Evaluation;

namespace Computator.NET.Charting
{
    public partial class PlotForm : Form
    {
        private EditChartMenus editChartMenus;

        public PlotForm(IChart chart)
        {
            InitializeComponent();


            if (chart is Chart3DControl)
                InitializeChart((chart as Chart3DControl).ParentControl);
            else
                InitializeChart(chart as Control);
        }

        private void InitializeChart(Control control)
        {
            Controls.Add(control);
            control.Dock = DockStyle.Fill;
            control.BringToFront();

            editChartMenus = new EditChartMenus(control as Chart2D, control as ComplexChart,
                (control as ElementHost)?.Child as Chart3DControl, control as ElementHost);


            menuStrip1.Items.AddRange(new ToolStripItem[]
            {
                editChartMenus.chartToolStripMenuItem
            });
            SetMode(control.GetType());
        }

        private void SetMode(Type chartType)
        {
            if (chartType == typeof (Chart2D))
            {
                editChartMenus.SetMode(CalculationsMode.Real);
            }
            else if (chartType == typeof (ComplexChart))
            {
                editChartMenus.SetMode(CalculationsMode.Complex);
            }
            else if (chartType == typeof (ElementHost))
            {
                editChartMenus.SetMode(CalculationsMode.Fxy);
            }
        }
    }
}