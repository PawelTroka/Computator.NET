using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    internal class ColorAssigmentCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public ColorAssigmentCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState)
            : base(MenuStrings.colorAssignmentToolStripMenuItem_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Complex;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Complex);


            var list = new List<IToolbarCommand>();

            foreach (var colorAssigment in Enum.GetValues(typeof(AssignmentOfColorMethod))
                .Cast<AssignmentOfColorMethod>())
            {
                list.Add(new ColorAssigmentOption(charts, colorAssigment,_sharedViewState));
            }
            ChildrenCommands = list;
        }

        private class ColorAssigmentOption : ChartOption
        {
            private readonly AssignmentOfColorMethod assignmentOfColorMethod;

            public ColorAssigmentOption(ReadOnlyDictionary<CalculationsMode, IChart> charts,
                AssignmentOfColorMethod assignmentOfColorMethod, ISharedViewState sharedViewState) : base(assignmentOfColorMethod, charts,sharedViewState)
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