using System.Windows.Forms;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class ExitCommand : CommandBase
    {


        public ExitCommand()
        {
            this.Text = MenuStrings.exitToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.exitToolStripMenuItem_Text;
        }


        public override void Execute()
        {
            Application.Exit();
        }
    }
}