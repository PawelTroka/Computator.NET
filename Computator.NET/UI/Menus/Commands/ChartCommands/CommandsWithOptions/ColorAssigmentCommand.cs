using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    internal class ColorAssigmentCommand : DummyCommand
    {
        public ColorAssigmentCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts)
            : base(MenuStrings.colorAssignmentToolStripMenuItem_Text)
        {
            Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Complex;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Complex);


            var list = new List<IToolbarCommand>();

            foreach (var colorAssigment in Enum.GetValues(typeof(AssignmentOfColorMethod))
                .Cast<AssignmentOfColorMethod>())
            {
                list.Add(new ColorAssigmentOption(charts, colorAssigment));
            }
            ChildrenCommands = list;
        }

        private class ColorAssigmentOption : ChartOption
        {
            private readonly AssignmentOfColorMethod assignmentOfColorMethod;

            public ColorAssigmentOption(ReadOnlyDictionary<CalculationsMode, IChart> charts,
                AssignmentOfColorMethod assignmentOfColorMethod) : base(assignmentOfColorMethod, charts)
            {
                this.assignmentOfColorMethod = assignmentOfColorMethod;
                IsOption = true;
                Checked = complexChart.ColorAssignmentMethod == assignmentOfColorMethod;

                BindingUtils.OnPropertyChanged(complexChart, nameof(complexChart.ColorAssignmentMethod), () =>
                    Checked = complexChart.ColorAssignmentMethod == assignmentOfColorMethod);
            }

            public override void Execute()
            {
                complexChart.ColorAssignmentMethod = assignmentOfColorMethod;
                complexChart.Redraw();
            }
        }
    }
}