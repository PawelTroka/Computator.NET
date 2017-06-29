using System;
using Computator.NET.Core.Model;
using Computator.NET.Core.Properties;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.TopMenuCommands
{
    public class RunCommand : CommandBase
    {
        private readonly ISharedViewState _sharedViewState;
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