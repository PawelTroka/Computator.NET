using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class ContourLinesCommand : DummyCommand
    {

        private class ContourLinesOption : ChartOption
        {
            private CountourLinesMode contourLinesMode;
            public override void Execute()
            {
                complexChart.countourMode = contourLinesMode;
                complexChart.Redraw();
            }

            public ContourLinesOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, CountourLinesMode contourLinesMode) : base(contourLinesMode, charts)
            {
                this.contourLinesMode = contourLinesMode;
                this.IsOption = true;
                this.Checked = complexChart.countourMode == contourLinesMode;

                BindingUtils.OnPropertyChanged(complexChart, nameof(complexChart.countourMode), () =>
                    this.Checked = complexChart.countourMode == contourLinesMode);
            }
        }
        public ContourLinesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.contourLinesMode_Text)
        {
            this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Complex;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Complex);


            var list = new List<IToolbarCommand>();

            foreach (var val in Enum.GetValues(typeof(CountourLinesMode))
                .Cast<CountourLinesMode>())
            {
                list.Add(new ContourLinesOption(charts, val));
            }
            ChildrenCommands = list;
        }
    }
}