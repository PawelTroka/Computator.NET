using Computator.NET.Core.Abstract;

namespace Computator.NET.Core.Menu.Commands.HelpCommands
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