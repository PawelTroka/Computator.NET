using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class LegendAligmentCommand : DummyCommand
    {
        private class LegendAligmentOption : ChartOption
        {
            private StringAlignment aligment;

            public override void Execute()
            {
                chart2d.Legends[0].Alignment = aligment;

            }

            public LegendAligmentOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, StringAlignment aligment) : base(aligment, charts)
            {
                this.aligment = aligment;
                this.IsOption = true;
                this.Checked = chart2d.Legends[0].Alignment == aligment;
            }
        }

        public LegendAligmentCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.aligment_Text)
        {
            var list = new List<IToolbarCommand>();

            foreach (var aligment in Enum.GetValues(typeof(StringAlignment))
                .Cast<StringAlignment>())
            {
                list.Add(new LegendAligmentOption(charts, aligment));
            }
            ChildrenCommands = list;
        }
    }
}