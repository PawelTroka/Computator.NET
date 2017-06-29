using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions
{
    public class TypeOfChartCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public TypeOfChartCommand(IChart2D chart2d,ISharedViewState sharedViewState) : base(MenuStrings.type_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real);


            var list = new List<IToolbarCommand>();

            foreach (var chartType in Enum.GetValues(typeof(SeriesChartType))
                .Cast<SeriesChartType>())
            {
                list.Add(new TypeOption(chart2d, chartType));
            }
            ChildrenCommands = list;
        }

        private class TypeOption : ChartOption
        {
            private readonly IChart2D _chart2D;
            private readonly SeriesChartType chartType;

            public TypeOption(IChart2D chart2d, SeriesChartType chartType)
                : base(chartType)
            {
                _chart2D = chart2d;
                this.chartType = chartType;
                IsOption = true;
                Checked = chart2d.ChartType == chartType;
            }

            public override void Execute()
            {
                _chart2D.ChartType = chartType;
            }
        }
    }
}