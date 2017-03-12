using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Menu.Commands.ChartCommands
{
    public class FitAxesCommand : CommandBase
    {
        private readonly IChart3D _chart3D;

        public FitAxesCommand(IChart3D chart3D)
        {
            _chart3D = chart3D;
            Text = MenuStrings.fitAxes_Text;
            ToolTip = MenuStrings.fitAxes_Text;
            CheckOnClick = true;
            Checked = !chart3D.EqualAxes;
            BindingUtils.OnPropertyChanged(chart3D, nameof(chart3D.EqualAxes), () => Checked = !chart3D.EqualAxes);
        }

        public override void Execute()
        {
            Checked = !Checked;
            _chart3D.EqualAxes = !Checked;
        }
    }
}