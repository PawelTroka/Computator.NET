using Computator.NET.UI.Dialogs;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class ChangelogCommand : CommandBase
    {
        public ChangelogCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            this.Text = MenuStrings.Changelog_Text;
            this.ToolTip = MenuStrings.Changelog_Text;
        }


        public override void Execute()
        {
            new ChangelogForm().ShowDialog(/*this*/);
        }
    }
}