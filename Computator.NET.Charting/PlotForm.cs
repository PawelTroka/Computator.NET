using System;
using System.Windows.Forms;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.Controls;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Charting
{
    public partial class PlotForm : Form
    {
        private EditChartMenus editChartMenus;

        public PlotForm(IChart chart)
        {
            InitializeComponent();

#if !__MonoCS__
            if (chart is Chart3DControl)
            {
                var el = new System.Windows.Forms.Integration.ElementHost { Child = chart as Chart3DControl};
                (chart as Chart3DControl).ParentControl = el;
                InitializeChart(el);
            }
            else
#endif
                InitializeChart(chart as Control);
        }

        private void InitializeChart(Control control)
        {
            Controls.Add(control);
            control.Dock = DockStyle.Fill;
            control.BringToFront();
            //TODO: use menu builder and command pattern for menu instead of dirty EditChartMenus
            editChartMenus = new EditChartMenus(control as Chart2D, control as ComplexChart,

#if __MonoCS__
                (control as Chart3DControl)
#else          
                 (control as System.Windows.Forms.Integration.ElementHost)?.Child as Chart3DControl, control as System.Windows.Forms.Integration.ElementHost
#endif
                );


            menuStrip1.Items.AddRange(new ToolStripItem[]
            {
                editChartMenus.chartToolStripMenuItem
            });
            SetMode(control.GetType());
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
            else if (chartType ==
#if !__MonoCS__
                typeof(System.Windows.Forms.Integration.ElementHost)
#else
                typeof(Chart3DControl)
#endif

                )
            {
                editChartMenus.SetMode(CalculationsMode.Fxy);
            }
        }
    }
}