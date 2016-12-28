using System.Diagnostics;
using Computator.NET.Core.Menu;
using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    public class BugReportingCommand : CommandBase
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