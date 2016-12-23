using System.Windows.Forms;
using Computator.NET.Core.UI.Menus;
using Computator.NET.DataTypes.Localization;
using Computator.NET.UI.ErrorHandling;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    public class FeaturesCommand : CommandBase
    {
        private IMessagingService _messagingService;
        public FeaturesCommand()
        {
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