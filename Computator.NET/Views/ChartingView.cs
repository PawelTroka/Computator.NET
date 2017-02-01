using System.Windows.Forms;
using Accord.Collections;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Views
{
    public partial class ChartingView : UserControl, IChartingView
    {
        public ChartingView(ChartAreaValuesView chartAreaValuesView, ReadOnlyDictionary<CalculationsMode, IChart> charts) : this()
        {
            chartAreaValuesView.Dock=DockStyle.Right;
            ChartAreaValuesView = chartAreaValuesView;
            Charts = charts;
#if !__MonoCS__
            var el = new System.Windows.Forms.Integration.ElementHost { Child = (Charts[CalculationsMode.Fxy] as Chart3DControl), Dock = DockStyle.Fill };
            (Charts[CalculationsMode.Fxy] as Chart3DControl).ParentControl = el;
#endif

            panel2.Controls.AddRange(new[]
            {
                Charts[CalculationsMode.Real] as Chart2D,
                Charts[CalculationsMode.Complex] as ComplexChart,
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

        public ReadOnlyDictionary<CalculationsMode, IChart> Charts { get; }

        public IChartAreaValuesView ChartAreaValuesView { get; }
    }
}