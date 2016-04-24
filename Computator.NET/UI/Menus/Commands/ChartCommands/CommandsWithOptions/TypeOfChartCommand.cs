using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class TypeOfChartCommand : DummyCommand
    {

        private  class TypeOption : ChartOption
        {
            private SeriesChartType chartType;
            public override void Execute()
            {
                chart2d.ChartType = chartType;
            }

            public TypeOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, SeriesChartType chartType) : base(chartType, charts)
            {
                this.chartType = chartType;
                this.IsOption = true;
                this.Checked = chart2d.ChartType == chartType;
            }
        }
        public TypeOfChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.type_Text)
        {
            this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real);


            var list = new List<IToolbarCommand>();

            foreach (var chartType in Enum.GetValues(typeof(SeriesChartType))
                .Cast<SeriesChartType>())
            {
                list.Add(new TypeOption(charts, chartType));
            }
            ChildrenCommands = list;
        }
    }
}