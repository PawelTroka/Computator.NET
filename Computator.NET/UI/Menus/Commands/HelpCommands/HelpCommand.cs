using Computator.NET.Properties;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class HelpCommand : CommandBase
    {


        public HelpCommand()
        {
            this.Icon = Resources.helpToolStripButtonImage;
            this.Text = MenuStrings.helpToolStripButton_Text;
            this.ToolTip = MenuStrings.helpToolStripButton_Text;
        }


        public override void Execute()
        {
            new AboutBox1().ShowDialog();
        }
    }
}