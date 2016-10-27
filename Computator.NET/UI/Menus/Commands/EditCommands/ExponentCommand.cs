using Computator.NET.Properties;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    internal class ExponentCommand : CommandBase
    {
        public ExponentCommand()
        {
            CheckOnClick = true;
            ShortcutKeyString = "Shift+6";
            SharedViewState.Instance.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(SharedViewState.Instance.IsExponent))
                    Checked = SharedViewState.Instance.IsExponent;
            };

            Icon = Resources.exponentation;
            Text = MenuStrings.exponentiationToolStripMenuItem_Text;
            ToolTip = MenuStrings.exponentiationToolStripMenuItem_Text;
        }


        public override void Execute()
        {
            SharedViewState.Instance.IsExponent = !SharedViewState.Instance.IsExponent;
        }
    }
}