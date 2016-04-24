using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Logging;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class LogsCommand : CommandBase
    {
        public LogsCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            this.Text = MenuStrings.Logs_Text;
            this.ToolTip = MenuStrings.Logs_Text;
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