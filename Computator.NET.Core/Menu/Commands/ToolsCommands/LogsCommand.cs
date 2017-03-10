using Computator.NET.Core.Abstract.Services;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.Core.Menu.Commands.ToolsCommands
{
    public class LogsCommand : CommandBase
    {
        private readonly IMessagingService _messagingService;
        private readonly IProcessRunnerService _processRunnerService;
        public LogsCommand(IMessagingService messagingService, IProcessRunnerService processRunnerService)
        {
            _messagingService = messagingService;
            _processRunnerService = processRunnerService;
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.Logs_Text;
            ToolTip = MenuStrings.Logs_Text;
        }


        public override void Execute()
        {

            try
            {
                _processRunnerService.Run(AppInformation.LogsDirectory);
            }
            catch
            {
                _messagingService.Show(
                    Strings.GUI_logsToolStripMenuItem_Click_You_dont_have_any_logs_yet_,string.Empty);
            }
        }
    }
}