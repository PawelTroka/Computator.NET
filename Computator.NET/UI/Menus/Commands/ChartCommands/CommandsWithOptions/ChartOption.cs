using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;

namespace Computator.NET.UI.Commands
{
    abstract class ChartOption   : BaseCommandForCharts
    {
        protected ChartOption(object value,ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.IsOption = true;
            this.CheckOnClick = true;
            this.Text = value.ToString();
            this.ToolTip = value.ToString();
        }
    }
}