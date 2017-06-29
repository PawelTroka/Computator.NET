using System.Collections.Generic;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public abstract class BaseCommandForCharts : CommandBase
    {
        protected readonly IChart2D chart2D;
        protected readonly IComplexChart complexChart;
        protected readonly IChart3D chart3D;
        private readonly ISharedViewState _sharedViewState;
        protected readonly IDictionary<CalculationsMode, IChart> _charts;

        protected BaseCommandForCharts(IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d, ISharedViewState sharedViewState)
        {
            this.chart2D = chart2d;
            this.complexChart = complexChart;
            this.chart3D = chart3d;
            _charts = new Dictionary<CalculationsMode, IChart>()
            {
                {CalculationsMode.Real, chart2d },
                {CalculationsMode.Complex, complexChart},
                {CalculationsMode.Fxy, chart3D},
            };
            _sharedViewState = sharedViewState;
        }

        protected IChart currentChart => _charts[_sharedViewState.CalculationsMode];


    }
}