using System.Windows.Forms;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.UI.Menus.Commands.HelpCommands
{
    public class FeaturesCommand : CommandBase
    {
        public FeaturesCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            Text = MenuStrings.Features_Text;
            ToolTip = MenuStrings.Features_Text;
        }


        public override void Execute()
        {
            MessageBox.Show(Strings.featuresInclude, Strings.Features);
        }
    }
}