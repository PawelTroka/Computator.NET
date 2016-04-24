using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class FitAxesCommand : BaseCommandForCharts
    {
        public FitAxesCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
        {
            this.Text = MenuStrings.fitAxes_Text;
            this.ToolTip = MenuStrings.fitAxes_Text;
            this.CheckOnClick = true;
            this.Checked = !chart3d.EqualAxes;
            BindingUtils.OnPropertyChanged(chart3d,nameof(chart3d.EqualAxes),()=> Checked = !chart3d.EqualAxes);
        }

        public override void Execute()
        {
            this.Checked = !this.Checked;
            chart3d.EqualAxes = !this.Checked;
        }
    }
}