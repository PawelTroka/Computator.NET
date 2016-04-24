using System;
using Computator.NET.Properties;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class RunCommand : CommandBase
    {


        public RunCommand()
        {
            this.Icon = Resources.runToolStripButtonImage;
            this.Text = MenuStrings.runToolStripButton_Text;
            this.ToolTip = MenuStrings.runToolStripButton_Text;
        }


        public override void Execute()
        {
            SharedViewState.Instance.CurrentAction.Invoke(this, new EventArgs());
        }
    }
}