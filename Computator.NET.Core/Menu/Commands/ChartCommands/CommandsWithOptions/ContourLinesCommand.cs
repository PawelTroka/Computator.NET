using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
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
        public ContourLinesCommand(IDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState)
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
                list.Add(new ContourLinesOption(charts, val,sharedViewState));
            }
            ChildrenCommands = list;
        }

        private class ContourLinesOption : ChartOption
        {
            private readonly CountourLinesMode contourLinesMode;

            public ContourLinesOption(IDictionary<CalculationsMode, IChart> charts,
                CountourLinesMode contourLinesMode, ISharedViewState sharedViewState) : base(contourLinesMode, charts,sharedViewState)
            {
                this.contourLinesMode = contourLinesMode;
                IsOption = true;
                Checked = complexChart.CountourMode == contourLinesMode;

                BindingUtils.OnPropertyChanged(complexChart, nameof(complexChart.CountourMode), () =>
                    Checked = complexChart.CountourMode == contourLinesMode);
            }

            public override void Execute()
            {
                complexChart.CountourMode = contourLinesMode;
                complexChart.Redraw();
            }
        }
    }
}