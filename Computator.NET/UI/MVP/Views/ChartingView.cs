using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Controls;

namespace Computator.NET.UI.MVP.Views
{
    public partial class ChartingView : UserControl, IChartingView
    {
        public ChartingView()
        {
            InitializeComponent();

            var el = new ElementHost() {Child = chart3d, Dock = DockStyle.Fill};
            chart3d.ParentControl = el;

            panel2.Controls.AddRange(new[]
            {
                ChartAreaValuesView as Control,
                chart2d,
                complexChart,
                el
            });
        }

        private Chart2D chart2d => Charts[CalculationsMode.Real] as Chart2D;
        private Chart3DControl chart3d => Charts[CalculationsMode.Fxy] as Chart3DControl;
        private ComplexChart complexChart => Charts[CalculationsMode.Complex] as ComplexChart;

        public ReadOnlyDictionary<CalculationsMode, IChart> Charts { get; } =
            new ReadOnlyDictionary<CalculationsMode, IChart>(new Dictionary<CalculationsMode, IChart>
            {
                {CalculationsMode.Real, new Chart2D()},
                {CalculationsMode.Complex, new ComplexChart()},
                {CalculationsMode.Fxy, new Chart3DControl()}
            });

        public IChartAreaValuesView ChartAreaValuesView { get; } = new ChartAreaValuesView {Dock = DockStyle.Right};
    }
}