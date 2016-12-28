using Computator.NET.Core.UI.Menus;
using Computator.NET.UI.Dialogs;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    public class ChangelogCommand : CommandBase
    {
        public ChangelogCommand(IDialogFactory dialogFactory)
        {
            _dialogFactory = dialogFactory;
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.Changelog_Text;
            ToolTip = MenuStrings.Changelog_Text;
        }

        private IDialogFactory _dialogFactory;

        public override void Execute()
        {
            _dialogFactory.ShowDialog("changelog");
        }
    }
}