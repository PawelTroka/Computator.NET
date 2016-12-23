using Computator.NET.Core.UI.Menus;
using Computator.NET.Properties;
using Computator.NET.UI.Dialogs;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    public class HelpCommand : CommandBase
    {
        public HelpCommand()
        {
            Icon = Resources.helpToolStripButtonImage;
            Text = MenuStrings.helpToolStripButton_Text;
            ToolTip = MenuStrings.helpToolStripButton_Text;
        }
        
        private IDialogFactory _dialogFactory;

        public override void Execute()
        {

            _dialogFactory.ShowDialog("about");
        }
    }
}