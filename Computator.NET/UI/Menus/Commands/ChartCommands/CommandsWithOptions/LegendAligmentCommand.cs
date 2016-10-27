using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    internal class LegendAligmentCommand : DummyCommand
    {
        public LegendAligmentCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts)
            : base(MenuStrings.aligment_Text)
        {
            var list = new List<IToolbarCommand>();

            foreach (var aligment in Enum.GetValues(typeof(StringAlignment))
                .Cast<StringAlignment>())
            {
                list.Add(new LegendAligmentOption(charts, aligment));
            }
            ChildrenCommands = list;
        }

        private class LegendAligmentOption : ChartOption
        {
            private readonly StringAlignment aligment;

            public LegendAligmentOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, StringAlignment aligment)
                : base(aligment, charts)
            {
                this.aligment = aligment;
                IsOption = true;
                Checked = chart2d.Legends[0].Alignment == aligment;
            }

            public override void Execute()
            {
                chart2d.Legends[0].Alignment = aligment;
            }
        }
    }
}