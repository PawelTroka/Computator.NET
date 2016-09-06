using System;
using Computator.NET.Properties;

namespace Computator.NET.UI.Menus.Commands
{
    internal class RunCommand : CommandBase
    {
        private ISharedViewState _sharedViewState;
        public RunCommand(ISharedViewState sharedViewState)
        {
            _sharedViewState = sharedViewState;
            Icon = Resources.runToolStripButtonImage;
            Text = MenuStrings.runToolStripButton_Text;
            ToolTip = MenuStrings.runToolStripButton_Text;
        }


        public override void Execute()
        {
            _sharedViewState.CurrentAction.Invoke(this, new EventArgs());
        }
    }
}