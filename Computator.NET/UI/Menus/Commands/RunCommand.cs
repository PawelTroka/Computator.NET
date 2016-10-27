using System;
using Computator.NET.Properties;

namespace Computator.NET.UI.Menus.Commands
{
    internal class RunCommand : CommandBase
    {
        public RunCommand()
        {
            Icon = Resources.runToolStripButtonImage;
            Text = MenuStrings.runToolStripButton_Text;
            ToolTip = MenuStrings.runToolStripButton_Text;
        }


        public override void Execute()
        {
            SharedViewState.Instance.CurrentAction.Invoke(this, new EventArgs());
        }
    }
}