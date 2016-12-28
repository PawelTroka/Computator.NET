using System;
using System.Windows.Forms;
using Computator.NET.Core.UI.Menus;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.UI.ErrorHandling;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
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