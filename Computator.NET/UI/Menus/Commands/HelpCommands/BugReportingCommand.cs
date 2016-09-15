using System.Diagnostics;
using Computator.NET.DataTypes;
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
            Process.Start(GlobalConfig.IssuesUrl);
            //new BugReportingForm().ShowDialog( /*this*/);
        }
    }
}