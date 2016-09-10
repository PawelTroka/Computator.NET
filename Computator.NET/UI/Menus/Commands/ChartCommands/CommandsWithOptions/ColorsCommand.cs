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
        private ISharedViewState _sharedViewState;
        public ColorsCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(MenuStrings.color_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real);

            var list = new List<IToolbarCommand>();

            foreach (var chartType in Enum.GetValues(typeof(ChartColorPalette))
                .Cast<ChartColorPalette>())
            {
                list.Add(new ColorOption(charts, chartType,sharedViewState));
            }
            ChildrenCommands = list;
        }

        private class ColorOption : ChartOption
        {
            private readonly ChartColorPalette color;

            public ColorOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, ChartColorPalette color, ISharedViewState sharedViewState)
                : base(color, charts,sharedViewState)
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