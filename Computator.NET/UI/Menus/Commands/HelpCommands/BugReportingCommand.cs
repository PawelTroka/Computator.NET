using Computator.NET.UI.Dialogs;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    internal class BugReportingCommand : CommandBase
    {
        public BugReportingCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.BugReporting_Text;
            ToolTip = MenuStrings.BugReporting_Text;
        }


        public override void Execute()
        {
            new BugReportingForm().ShowDialog( /*this*/);
        }
    }
}