using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    internal class TypeOfChartCommand : DummyCommand
    {
        public TypeOfChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.type_Text)
        {
            Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real);


            var list = new List<IToolbarCommand>();

            foreach (var chartType in Enum.GetValues(typeof(SeriesChartType))
                .Cast<SeriesChartType>())
            {
                list.Add(new TypeOption(charts, chartType));
            }
            ChildrenCommands = list;
        }

        private class TypeOption : ChartOption
        {
            private readonly SeriesChartType chartType;

            public TypeOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, SeriesChartType chartType)
                : base(chartType, charts)
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