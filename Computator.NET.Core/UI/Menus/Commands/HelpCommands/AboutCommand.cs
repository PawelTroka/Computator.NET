using Computator.NET.Core.UI.Menus;
using Computator.NET.UI.Dialogs;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    public class AboutCommand : CommandBase
    {
        public AboutCommand(IDialogFactory dialogFactory)
        {
            _dialogFactory = dialogFactory;
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.aboutToolStripMenuItem1_Text;
            ToolTip = MenuStrings.aboutToolStripMenuItem1_Text;
        }
        
        private IDialogFactory _dialogFactory;

        public override void Execute()
        {
            _dialogFactory.ShowDialog("about");
        }
    }
}