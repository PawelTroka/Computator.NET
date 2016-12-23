using System.Windows.Forms;
using Computator.NET.Core.UI.Menus;

namespace Computator.NET.UI.Menus.Commands.FileCommands
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