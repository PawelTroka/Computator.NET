using Computator.NET.Core.UI.Menus;
using Computator.NET.Properties;
using Computator.NET.UI.Dialogs;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    public class HelpCommand : CommandBase
    {
        public HelpCommand(IDialogFactory dialogFactory)
        {
            _dialogFactory = dialogFactory;
            Icon = Resources.helpToolStripButtonImage;
            Text = MenuStrings.helpToolStripButton_Text;
            ToolTip = MenuStrings.helpToolStripButton_Text;
        }
        
        private readonly IDialogFactory _dialogFactory;

        public override void Execute()
        {

            _dialogFactory.ShowDialog("about");
        }
    }
}