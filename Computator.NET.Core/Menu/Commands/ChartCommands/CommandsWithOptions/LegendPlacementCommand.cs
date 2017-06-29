using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions
{
    public class LegendPlacementCommand : DummyCommand
    {
        public LegendPlacementCommand(IChart2D chart2d, ISharedViewState sharedViewState)
            : base(MenuStrings.placement_Text)
        {
            var list = new List<IToolbarCommand>();

            foreach (var docking in Enum.GetValues(typeof(Docking))
                .Cast<Docking>())
            {
                list.Add(new LegendPlacementOption(chart2d, docking));
            }
            ChildrenCommands = list;
        }

        private class LegendPlacementOption : ChartOption
        {
            private readonly IChart2D _chart2D;
            private readonly Docking placement;

            public LegendPlacementOption(IChart2D chart2d,  Docking placement)
                : base(placement)
            {
                Checked = chart2d.LegendDocking == placement;
                _chart2D = chart2d;
                this.placement = placement;
                IsOption = true;
            }

            public override void Execute()
            {
                _chart2D.LegendDocking = placement;
            }
        }
    }
}