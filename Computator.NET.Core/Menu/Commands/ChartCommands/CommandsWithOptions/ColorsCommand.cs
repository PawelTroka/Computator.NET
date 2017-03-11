using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions
{
    public class ColorsCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public ColorsCommand(IChart2D chart2d, ISharedViewState sharedViewState) : base(MenuStrings.color_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real);

            var list = new List<IToolbarCommand>();

            foreach (var chartType in Enum.GetValues(typeof(ChartColorPalette))
                .Cast<ChartColorPalette>())
            {
                list.Add(new ColorOption(chart2d, chartType));
            }
            ChildrenCommands = list;
        }

        private class ColorOption : ChartOption
        {
            private readonly IChart2D _chart2D;
            private readonly ChartColorPalette color;

            public ColorOption(IChart2D chart2d, ChartColorPalette color)
                : base(color)
            {
                _chart2D = chart2d;
                this.color = color;
                Checked = chart2d.Palette == color;
            }


            public override void Execute()
            {
                _chart2D.Palette = color;
            }
        }
    }
}