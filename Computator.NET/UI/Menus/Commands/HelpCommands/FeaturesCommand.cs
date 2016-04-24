using System.Windows.Forms;
using Computator.NET.DataTypes.Localization;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class FeaturesCommand : CommandBase
    {
        public FeaturesCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            this.Text = MenuStrings.Features_Text;
            this.ToolTip = MenuStrings.Features_Text;
        }


        public override void Execute()
        {
            MessageBox.Show(Strings.featuresInclude, Strings.Features);
        }
    }
}