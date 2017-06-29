using Computator.NET.Core.Abstract;
using Computator.NET.Core.Properties;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.HelpCommands
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