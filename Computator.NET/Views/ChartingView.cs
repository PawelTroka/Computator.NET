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

            var el = new ElementHost { Child = (Charts[CalculationsMode.Fxy] as Chart3DControl), Dock = DockStyle.Fill };
            (Charts[CalculationsMode.Fxy] as Chart3DControl).ParentControl = el;

            panel2.Controls.AddRange(new[]
            {
                Charts[CalculationsMode.Real] as Chart2D,
                Charts[CalculationsMode.Complex] as ComplexChart,
                el,
                (Control) chartAreaValuesView,
            });
        }

        private ChartingView()
        {
            InitializeComponent();
        }

        public ReadOnlyDictionary<CalculationsMode, IChart> Charts { get; }

        public IChartAreaValuesView ChartAreaValuesView { get; }
    }
}