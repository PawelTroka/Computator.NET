using Computator.NET.Config;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class OptionsCommand : CommandBase
    {
        public OptionsCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            this.Text = MenuStrings.optionsToolStripMenuItem1_Text;
            this.ToolTip = MenuStrings.optionsToolStripMenuItem1_Text;
        }


        public override void Execute()
        {
            new SettingsForm().ShowDialog(/*this*/);
        }
    }
}