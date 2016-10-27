using Computator.NET.Config;

namespace Computator.NET.UI.Menus.Commands.ToolsCommands
{
    internal class OptionsCommand : CommandBase
    {
        public OptionsCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.optionsToolStripMenuItem1_Text;
            ToolTip = MenuStrings.optionsToolStripMenuItem1_Text;
        }


        public override void Execute()
        {
            new SettingsForm().ShowDialog( /*this*/);
        }
    }
}