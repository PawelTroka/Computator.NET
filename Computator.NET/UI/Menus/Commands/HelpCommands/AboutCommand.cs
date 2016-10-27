using Computator.NET.UI.Dialogs;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    internal class AboutCommand : CommandBase
    {
        public AboutCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.aboutToolStripMenuItem1_Text;
            ToolTip = MenuStrings.aboutToolStripMenuItem1_Text;
        }


        public override void Execute()
        {
            new AboutBox1().ShowDialog( /*this*/);
        }
    }
}