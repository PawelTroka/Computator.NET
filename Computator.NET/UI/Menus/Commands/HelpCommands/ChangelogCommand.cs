using Computator.NET.UI.Dialogs;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    public class ChangelogCommand : CommandBase
    {
        public ChangelogCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.Changelog_Text;
            ToolTip = MenuStrings.Changelog_Text;
        }


        public override void Execute()
        {
            new ChangelogForm().ShowDialog( /*this*/);
        }
    }
}