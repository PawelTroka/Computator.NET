using System;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.Core.Menu.Commands.HelpCommands
{
    public class ThanksToCommand : CommandBase
    {
        private IMessagingService _messagingService;
        public ThanksToCommand(IMessagingService messagingService)
        {
            _messagingService = messagingService;
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.ThanksTo_Text;
            ToolTip = MenuStrings.ThanksTo_Text;
        }


        public override void Execute()
        {
            _messagingService.Show(
                GlobalConfig.Betatesters + Environment.NewLine + Environment.NewLine + GlobalConfig.Translators +
                Environment.NewLine + Environment.NewLine +
                GlobalConfig.Libraries + Environment.NewLine + Environment.NewLine +
                GlobalConfig.Others, Strings.SpecialThanksTo);
        }
    }
}