using Accord.Collections;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public abstract class BaseCommandForCharts : CommandBase
    {
        private readonly ISharedViewState _sharedViewState;
        protected readonly ReadOnlyDictionary<CalculationsMode, IChart> _charts;

        protected BaseCommandForCharts(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState)
        {
            _charts = charts;
            _sharedViewState = sharedViewState;
        }

        protected IChart currentChart => _charts[_sharedViewState.CalculationsMode];

        protected Chart2D chart2d => _charts[CalculationsMode.Real] as Chart2D;//TODO: get rid of this filthy dependencies on Charting!!!!!!!!!!!!!!!!!!!
        protected Chart3DControl chart3d => _charts[CalculationsMode.Fxy] as Chart3DControl;
        protected ComplexChart complexChart => _charts[CalculationsMode.Complex] as ComplexChart;
    }
}