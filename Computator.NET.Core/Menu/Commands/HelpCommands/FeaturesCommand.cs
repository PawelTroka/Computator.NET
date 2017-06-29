using Computator.NET.Core.Abstract.Services;
using Computator.NET.Localization;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.HelpCommands
{
    public class FeaturesCommand : CommandBase
    {
        private readonly IMessagingService _messagingService;
        public FeaturesCommand(IMessagingService messagingService)
        {
            _messagingService = messagingService;
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.Features_Text;
            ToolTip = MenuStrings.Features_Text;
        }


        public override void Execute()
        {
            _messagingService.Show(Strings.featuresInclude, Strings.Features);
        }
    }
}