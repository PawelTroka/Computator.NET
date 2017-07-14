using System.Collections.Generic;
using System.Windows.Forms;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Desktop.Views
{
    public partial class ChartingView : UserControl, IChartingView
    {
        public ChartingView(ChartAreaValuesView chartAreaValuesView, Chart2D chart2D, ComplexChart complexChart, Chart3DControl chart3D) : this()
        {
            chartAreaValuesView.Dock = DockStyle.Right;
            ChartAreaValuesView = chartAreaValuesView;
            Chart2D = chart2D;
            ComplexChart = complexChart;
            Chart3D = chart3D;
            Charts = new Dictionary<CalculationsMode, IChart>()
            {
                {CalculationsMode.Real, Chart2D},
                {CalculationsMode.Complex, ComplexChart},
                {CalculationsMode.Fxy, chart3D}
            };
#if !__MonoCS__
            var el = new System.Windows.Forms.Integration.ElementHost { Child = chart3D, Dock = DockStyle.Fill };
            chart3D.ParentControl = el;
#endif

            panel2.Controls.AddRange(new[]
            {
                chart2D,
                complexChart,
#if !__MonoCS__
                el,
#endif
                (Control) chartAreaValuesView,
            });
        }

        private ChartingView()
        {
            InitializeComponent();
        }
        
        public IChartAreaValuesView ChartAreaValuesView { get; }
        public IChart2D Chart2D { get; }
        public IComplexChart ComplexChart { get; }
        public IChart3D Chart3D { get; }
        public IDictionary<CalculationsMode, IChart> Charts { get; }
    }
}