using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class ColorAssigmentCommand : DummyCommand
    {

        private class ColorAssigmentOption : ChartOption
        {
            private AssignmentOfColorMethod assignmentOfColorMethod;
            public override void Execute()
            {
                complexChart.colorAssignmentMethod = assignmentOfColorMethod;
                complexChart.Redraw();
            }

            public ColorAssigmentOption(ReadOnlyDictionary<CalculationsMode, IChart> charts, AssignmentOfColorMethod assignmentOfColorMethod) : base(assignmentOfColorMethod, charts)
            {
                this.assignmentOfColorMethod = assignmentOfColorMethod;
                this.IsOption = true;
                this.Checked = complexChart.colorAssignmentMethod == assignmentOfColorMethod;

                BindingUtils.OnPropertyChanged(complexChart,nameof(complexChart.colorAssignmentMethod),()=>
                    this.Checked = complexChart.colorAssignmentMethod == assignmentOfColorMethod);
            }
        }
        public ColorAssigmentCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.colorAssignmentToolStripMenuItem_Text)
        {
            this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Complex;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Complex);


            var list = new List<IToolbarCommand>();

            foreach (var colorAssigment in Enum.GetValues(typeof(AssignmentOfColorMethod))
                .Cast<AssignmentOfColorMethod>())
            {
                list.Add(new ColorAssigmentOption(charts, colorAssigment));
            }
            ChildrenCommands = list;
        }
    }
}