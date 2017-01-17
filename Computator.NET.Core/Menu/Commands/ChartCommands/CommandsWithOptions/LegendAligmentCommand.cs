using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Accord.Collections;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions
{
    public class LegendAligmentCommand : DummyCommand
    {
        public LegendAligmentCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState)
            : base(MenuStrings.aligment_Text)
        {
            var list = new List<IToolbarCommand>();

            foreach (var aligment in Enum.GetValues(typeof(StringAlignment))
                .Cast<StringAlignment>())
            {
                list.Add(new LegendAligmentOption(charts, aligment,sharedViewState));
            }
            ChildrenCommands = list;
        }

        private class LegendAligmentOption : ChartOption
        {
            private readonly StringAlignment aligment;

            public LegendAligmentOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, StringAlignment aligment, ISharedViewState sharedViewState)
                : base(aligment, charts,sharedViewState)
            {
                this.aligment = aligment;
                IsOption = true;
                Checked = chart2d.LegendAlignment == aligment;
            }

            public override void Execute()
            {
                chart2d.LegendAlignment = aligment;
            }
        }
    }
}