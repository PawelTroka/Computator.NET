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
    class ColorsCommand : DummyCommand
    {
        private class ColorOption : ChartOption
        {

            private ChartColorPalette color;



            public override void Execute()
            {
                chart2d.Palette = color;
            }

            public ColorOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, ChartColorPalette color) : base(color, charts)
            {
                this.color = color;
                this.Checked = chart2d.Palette == color;
            }
        }

        public ColorsCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.color_Text)
        {
            this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real);

            var list = new List<IToolbarCommand>();

            foreach (var chartType in Enum.GetValues(typeof(ChartColorPalette))
                .Cast<ChartColorPalette>())
            {
                list.Add(new ColorOption(charts,chartType));
            }
            ChildrenCommands = list;
        }
    }
}