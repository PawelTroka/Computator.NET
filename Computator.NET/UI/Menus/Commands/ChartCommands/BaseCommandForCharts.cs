using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.ChartCommands
{
    internal abstract class BaseCommandForCharts : CommandBase
    {
        protected readonly ReadOnlyDictionary<CalculationsMode, IChart> _charts;

        protected BaseCommandForCharts(ReadOnlyDictionary<CalculationsMode, IChart> charts)
        {
            _charts = charts;
        }

        protected IChart currentChart => _charts[_sharedViewState.CalculationsMode];

        protected Chart2D chart2d => _charts[CalculationsMode.Real] as Chart2D;
        protected Chart3DControl chart3d => _charts[CalculationsMode.Fxy] as Chart3DControl;
        protected ComplexChart complexChart => _charts[CalculationsMode.Complex] as ComplexChart;
    }
}