using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Core.UI.Menus;
using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    public class TypeOfChartCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public TypeOfChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(MenuStrings.type_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real);


            var list = new List<IToolbarCommand>();

            foreach (var chartType in Enum.GetValues(typeof(SeriesChartType))
                .Cast<SeriesChartType>())
            {
                list.Add(new TypeOption(charts, chartType,sharedViewState));
            }
            ChildrenCommands = list;
        }

        private class TypeOption : ChartOption
        {
            private readonly SeriesChartType chartType;

            public TypeOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, SeriesChartType chartType, ISharedViewState sharedViewState)
                : base(chartType, charts,sharedViewState)
            {
                this.chartType = chartType;
                IsOption = true;
                Checked = chart2d.ChartType == chartType;
            }

            public override void Execute()
            {
                chart2d.ChartType = chartType;
            }
        }
    }
}