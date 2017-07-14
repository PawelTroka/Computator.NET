using System;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Information;
using Computator.NET.Localization;
using Computator.NET.Localization.Menu;

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
                ContributionsInformation.Betatesters + Environment.NewLine + Environment.NewLine + ContributionsInformation.Translators +
                Environment.NewLine + Environment.NewLine +
                ContributionsInformation.Libraries + Environment.NewLine + Environment.NewLine +
                ContributionsInformation.Others, Strings.SpecialThanksTo);
        }
    }
}