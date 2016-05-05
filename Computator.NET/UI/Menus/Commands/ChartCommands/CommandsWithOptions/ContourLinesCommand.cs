using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    internal class ContourLinesCommand : DummyCommand
    {
        public ContourLinesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts)
            : base(MenuStrings.contourLinesMode_Text)
        {
            Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Complex;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Complex);


            var list = new List<IToolbarCommand>();

            foreach (var val in Enum.GetValues(typeof(CountourLinesMode))
                .Cast<CountourLinesMode>())
            {
                list.Add(new ContourLinesOption(charts, val));
            }
            ChildrenCommands = list;
        }

        private class ContourLinesOption : ChartOption
        {
            private readonly CountourLinesMode contourLinesMode;

            public ContourLinesOption(ReadOnlyDictionary<CalculationsMode, IChart> charts,
                CountourLinesMode contourLinesMode) : base(contourLinesMode, charts)
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