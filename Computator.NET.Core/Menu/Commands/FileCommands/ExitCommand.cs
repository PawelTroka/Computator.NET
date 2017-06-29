using System.Windows.Forms;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.FileCommands
{
    public class ExitCommand : CommandBase
    {
        public ExitCommand()
        {
            Text = MenuStrings.exitToolStripMenuItem_Text;
            ToolTip = MenuStrings.exitToolStripMenuItem_Text;
        }


        public override void Execute()
        {
            Application.Exit();
        }
    }
}