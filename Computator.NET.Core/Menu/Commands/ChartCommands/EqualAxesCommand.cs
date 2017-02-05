using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class EqualAxesCommand : BaseCommandForCharts
    {
        public EqualAxesCommand(IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d, ISharedViewState sharedViewState) : base(chart2d,complexChart, chart3d, sharedViewState)
        {
            Text = MenuStrings.equalAxes_Text;
            ToolTip = MenuStrings.equalAxes_Text;
            CheckOnClick = true;
            Checked = chart3D.EqualAxes;
            BindingUtils.TwoWayBinding(this, nameof(Checked), chart3D, nameof(chart3D.EqualAxes));
        }

        public override void Execute()
        {
            Checked = !Checked;
        }
    }
}