using Computator.NET.Config;
using Computator.NET.Core.UI.Menus;
using Computator.NET.UI.Dialogs;

namespace Computator.NET.UI.Menus.Commands.ToolsCommands
{
    public class OptionsCommand : CommandBase
    {

        private IDialogFactory _dialogFactory;
        public OptionsCommand(IDialogFactory dialogFactory)
        {
            _dialogFactory = dialogFactory;
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.optionsToolStripMenuItem1_Text;
            ToolTip = MenuStrings.optionsToolStripMenuItem1_Text;
        }


        public override void Execute()
        {
            _dialogFactory.ShowDialog("settings");
        }
    }
}