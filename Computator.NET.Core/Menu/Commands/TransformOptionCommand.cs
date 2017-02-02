using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Core.Menu.Commands.ChartCommands;
using Computator.NET.Core.Model;
using Computator.NET.Core.Transformations;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands
{
    internal class TransformOptionCommand : BaseCommandForCharts
    {
        private readonly ISharedViewState _sharedViewState;
        public TransformOptionCommand(string text, string toolTip, IDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState)
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