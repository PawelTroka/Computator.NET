using Computator.NET.Properties;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class ExponentCommand : CommandBase
    {
        public ExponentCommand()
        {
            this.CheckOnClick = true;
            this.ShortcutKeyString = "Shift+6";
            SharedViewState.Instance.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(SharedViewState.Instance.IsExponent))
                    this.Checked = SharedViewState.Instance.IsExponent;
            };

            this.Icon = Resources.exponentation;
            this.Text = MenuStrings.exponentiationToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.exponentiationToolStripMenuItem_Text;
        }


        public override void Execute()
        {
            SharedViewState.Instance.IsExponent = !SharedViewState.Instance.IsExponent;
        }
    }
}