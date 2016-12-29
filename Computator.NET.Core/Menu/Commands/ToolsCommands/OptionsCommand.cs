using Computator.NET.Core.Abstract;

namespace Computator.NET.Core.Menu.Commands.ToolsCommands
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