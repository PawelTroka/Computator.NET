using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class EqualAxesCommand : BaseCommandForCharts
    {
        public EqualAxesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.Text = MenuStrings.equalAxes_Text;
            this.ToolTip = MenuStrings.equalAxes_Text;
            this.CheckOnClick = true;
            this.Checked = chart3d.EqualAxes;
            BindingUtils.TwoWayBinding(this,nameof(Checked),chart3d,nameof(chart3d.EqualAxes));
        }

        public override void Execute()
        {
            this.Checked = !this.Checked;
        }
    }
}