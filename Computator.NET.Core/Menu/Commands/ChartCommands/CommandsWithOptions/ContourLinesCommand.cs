using System;
using System.Collections.Generic;
using System.Linq;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions
{
    public class ContourLinesCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public ContourLinesCommand(IComplexChart complexChart, ISharedViewState sharedViewState)
            : base(MenuStrings.contourLinesMode_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Complex;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Complex);


            var list = new List<IToolbarCommand>();

            foreach (var val in Enum.GetValues(typeof(CountourLinesMode))
                .Cast<CountourLinesMode>())
            {
                list.Add(new ContourLinesOption(complexChart, val));
            }
            ChildrenCommands = list;
        }

        private class ContourLinesOption : ChartOption
        {
            private readonly IComplexChart _complexChart;
            private readonly CountourLinesMode contourLinesMode;

            public ContourLinesOption(IComplexChart complexChart,
                CountourLinesMode contourLinesMode) : base(contourLinesMode)
            {
                _complexChart = complexChart;
                this.contourLinesMode = contourLinesMode;
                IsOption = true;
                Checked = complexChart.CountourMode == contourLinesMode;

                BindingUtils.OnPropertyChanged(complexChart, nameof(complexChart.CountourMode), () =>
                    Checked = complexChart.CountourMode == contourLinesMode);
            }

            public override void Execute()
            {
                _complexChart.CountourMode = contourLinesMode;
                _complexChart.Redraw();
            }
        }
    }
}