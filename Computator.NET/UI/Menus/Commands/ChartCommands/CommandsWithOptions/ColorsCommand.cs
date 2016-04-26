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
    internal class ColorsCommand : DummyCommand
    {
        public ColorsCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.color_Text)
        {
            Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real);

            var list = new List<IToolbarCommand>();

            foreach (var chartType in Enum.GetValues(typeof(ChartColorPalette))
                .Cast<ChartColorPalette>())
            {
                list.Add(new ColorOption(charts, chartType));
            }
            ChildrenCommands = list;
        }

        private class ColorOption : ChartOption
        {
            private readonly ChartColorPalette color;

            public ColorOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, ChartColorPalette color)
                : base(color, charts)
            {
                this.color = color;
                Checked = chart2d.Palette == color;
            }


            public override void Execute()
            {
                chart2d.Palette = color;
            }
        }
    }
}