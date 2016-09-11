using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class ChartingView : UserControl, IChartingView
    {
        public ChartingView(ChartAreaValuesView chartAreaValuesView, ReadOnlyDictionary<CalculationsMode, IChart> charts) : this()
        {
            chartAreaValuesView.Dock=DockStyle.Right;
            ChartAreaValuesView = chartAreaValuesView;
            Charts = charts;

            var el = new ElementHost { Child = chart3d, Dock = DockStyle.Fill };
            chart3d.ParentControl = el;

            panel2.Controls.AddRange(new[]
            {
                chart2d,
                complexChart,
                el,
                (Control) chartAreaValuesView,
            });
        }

        private ChartingView()
        {
            InitializeComponent();
        }

        private Chart2D chart2d => Charts[CalculationsMode.Real] as Chart2D;
        private Chart3DControl chart3d => Charts[CalculationsMode.Fxy] as Chart3DControl;
        private ComplexChart complexChart => Charts[CalculationsMode.Complex] as ComplexChart;

        public ReadOnlyDictionary<CalculationsMode, IChart> Charts { get; }

        public IChartAreaValuesView ChartAreaValuesView { get; }
    }
}