using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class AboutCommand : CommandBase
    {
        public AboutCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            this.Text = MenuStrings.aboutToolStripMenuItem1_Text;
            this.ToolTip = MenuStrings.aboutToolStripMenuItem1_Text;
        }


        public override void Execute()
        {
            new AboutBox1().ShowDialog(/*this*/);
        }
    }
}