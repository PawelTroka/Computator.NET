using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Logging;

namespace Computator.NET.UI.Menus.Commands.ToolsCommands
{
    internal class LogsCommand : CommandBase
    {
        public LogsCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.Logs_Text;
            ToolTip = MenuStrings.Logs_Text;
        }


        public override void Execute()
        {
            if (Directory.Exists(SimpleLogger.LogsDirectory))
                Process.Start(SimpleLogger.LogsDirectory);
            else
                MessageBox.Show(
                    Strings.GUI_logsToolStripMenuItem_Click_You_dont_have_any_logs_yet_);
        }
    }
}