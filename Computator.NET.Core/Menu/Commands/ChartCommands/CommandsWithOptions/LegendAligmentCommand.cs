using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions
{
    public class LegendAligmentCommand : DummyCommand
    {
        public LegendAligmentCommand(IChart2D chart2d)
            : base(MenuStrings.aligment_Text)
        {
            var list = new List<IToolbarCommand>();

            foreach (var aligment in Enum.GetValues(typeof(StringAlignment))
                .Cast<StringAlignment>())
            {
                list.Add(new LegendAligmentOption(chart2d, aligment));
            }
            ChildrenCommands = list;
        }

        private class LegendAligmentOption : ChartOption
        {
            private readonly IChart2D _chart2D;
            private readonly StringAlignment aligment;

            public LegendAligmentOption(IChart2D chart2d, StringAlignment aligment)
                : base(aligment)
            {
                _chart2D = chart2d;
                this.aligment = aligment;
                IsOption = true;
                Checked = chart2d.LegendAlignment == aligment;
            }

            public override void Execute()
            {
                _chart2D.LegendAlignment = aligment;
            }
        }
    }
}