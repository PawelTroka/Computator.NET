using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Core.UI.Menus;
using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    public class LegendPlacementCommand : DummyCommand
    {
        public LegendPlacementCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState)
            : base(MenuStrings.placement_Text)
        {
            var list = new List<IToolbarCommand>();

            foreach (var docking in Enum.GetValues(typeof(Docking))
                .Cast<Docking>())
            {
                list.Add(new LegendPlacementOption(charts, docking, sharedViewState));
            }
            ChildrenCommands = list;
        }

        private class LegendPlacementOption : ChartOption
        {
            private readonly Docking placement;

            public LegendPlacementOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, Docking placement, ISharedViewState sharedViewState)
                : base(placement, charts,sharedViewState)
            {
                Checked = chart2d.Legends[0].Docking == placement;
                this.placement = placement;
                IsOption = true;
            }

            public override void Execute()
            {
                chart2d.Legends[0].Docking = placement;
            }
        }
    }
}