using Computator.NET.UI.Dialogs;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class BugReportingCommand : CommandBase
    {
        public BugReportingCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            this.Text = MenuStrings.BugReporting_Text;
            this.ToolTip = MenuStrings.BugReporting_Text;
        }


        public override void Execute()
        {
            new BugReportingForm().ShowDialog(/*this*/);
        }
    }
}