using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.Transformations;
using Computator.NET.UI.Menus.Commands.ChartCommands;

namespace Computator.NET.UI.Menus.Commands
{
    internal class TransformOptionCommand : BaseCommandForCharts
    {
        private ISharedViewState _sharedViewState;
        public TransformOptionCommand(string text, string toolTip, ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState)
            : base(charts,sharedViewState)
        {
            _sharedViewState = sharedViewState;
            Text = text;
            ToolTip = toolTip;
        }

        public override void Execute()
        {
            if (_sharedViewState.CalculationsMode == CalculationsMode.Real)

                chart2d.Transform(
                    points => MathematicalTransformations.Transform(points, Text),
                    Text);
            //  else if (complexNumbersModeRadioBox.Checked)
            //    else if(fxyModeRadioBox.Checked)
        }
    }
}