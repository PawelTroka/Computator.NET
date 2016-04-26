using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.Transformations;
using Computator.NET.UI.Menus.Commands.ChartCommands;

namespace Computator.NET.UI.Menus.Commands
{
    internal class TransformOptionCommand : BaseCommandForCharts
    {
        public TransformOptionCommand(string text, string toolTip, ReadOnlyDictionary<CalculationsMode, IChart> charts)
            : base(charts)
        {
            Text = text;
            ToolTip = toolTip;
        }

        public override void Execute()
        {
            if (SharedViewState.Instance.CalculationsMode == CalculationsMode.Real)

                chart2d.Transform(
                    points => MathematicalTransformations.Transform(points, Text),
                    Text);
            //  else if (complexNumbersModeRadioBox.Checked)
            //    else if(fxyModeRadioBox.Checked)
        }
    }
}