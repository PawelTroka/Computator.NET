using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.Transformations;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class TransformOptionCommand : BaseCommandForCharts
    {
        public override void Execute()
        {
            if (SharedViewState.Instance.CalculationsMode == CalculationsMode.Real)
 
                chart2d.Transform(
                    points => MathematicalTransformations.Transform(points, this.Text),
                    this.Text);
            //  else if (complexNumbersModeRadioBox.Checked)
            //    else if(fxyModeRadioBox.Checked)
        }

        public TransformOptionCommand(string text, string toolTip,ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.Text = text;
            this.ToolTip = toolTip;
        }
    }
}