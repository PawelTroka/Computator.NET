using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class LegendPlacementCommand : DummyCommand
    {

        private class LegendPlacementOption : ChartOption
        {
            private Docking placement;
            public override void Execute()
            {
                chart2d.Legends[0].Docking = placement;
            }

            public LegendPlacementOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, Docking placement) : base(placement, charts)
            {
                this.Checked = chart2d.Legends[0].Docking == placement;
                this.placement = placement;
                this.IsOption = true;
            }
        }

        public LegendPlacementCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.placement_Text)
        {

            var list = new List<IToolbarCommand>();

            foreach (var docking in Enum.GetValues(typeof(Docking))
                .Cast<Docking>())
            {
                list.Add(new LegendPlacementOption(charts, docking));
            }
            ChildrenCommands = list;
        }
    }
}