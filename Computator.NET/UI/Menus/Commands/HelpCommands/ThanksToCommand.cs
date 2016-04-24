using System;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.DataTypes.Localization;
using Computator.NET.UI.Menus;

namespace Computator.NET.UI.Commands
{
    class ThanksToCommand : CommandBase
    {
        public ThanksToCommand()
        {
            //this.ShortcutKeyString = "Shift+6";
            //this.Icon = Resources.exponentation;
            this.Text = MenuStrings.ThanksTo_Text;
            this.ToolTip = MenuStrings.ThanksTo_Text;
        }


        public override void Execute()
        {
            MessageBox.Show(
                GlobalConfig.betatesters + Environment.NewLine + Environment.NewLine + GlobalConfig.translators +
                Environment.NewLine + Environment.NewLine +
                GlobalConfig.libraries + Environment.NewLine + Environment.NewLine +
                GlobalConfig.others, Strings.SpecialThanksTo);
        }
    }
}