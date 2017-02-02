using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public abstract class BaseCommandForCharts : CommandBase
    {
        private readonly ISharedViewState _sharedViewState;
        protected readonly IDictionary<CalculationsMode, IChart> _charts;

        protected BaseCommandForCharts(IDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState)
        {
            _charts = charts;
            _sharedViewState = sharedViewState;
        }

        protected IChart currentChart => _charts[_sharedViewState.CalculationsMode];

        protected IChart2D chart2d => _charts[CalculationsMode.Real] as IChart2D;
        protected IChart3D chart3d => _charts[CalculationsMode.Fxy] as IChart3D;
        protected IComplexChart complexChart => _charts[CalculationsMode.Complex] as IComplexChart;
    }
}