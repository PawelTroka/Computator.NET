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
        public TransformOptionCommand(string text, string toolTip, IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d, ISharedViewState sharedViewState)
            : base(chart2d,complexChart, chart3d, sharedViewState)
        {
            _sharedViewState = sharedViewState;
            Text = text;
            ToolTip = toolTip;
        }

        public override void Execute()
        {
            if (_sharedViewState.CalculationsMode == CalculationsMode.Real)

                chart2D.Transform(
                    points => MathematicalTransformations.Transform(points, Text),
                    Text);
            //  else if (complexNumbersModeRadioBox.Checked)
            //    else if(fxyModeRadioBox.Checked)
        }
    }
}