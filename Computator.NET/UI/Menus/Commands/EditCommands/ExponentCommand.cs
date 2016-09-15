using Computator.NET.Properties;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    public class ExponentCommand : CommandBase
    {
        private ISharedViewState _sharedViewState;
        public ExponentCommand(ISharedViewState sharedViewState)
        {
            _sharedViewState = sharedViewState;
            CheckOnClick = true;
            ShortcutKeyString = "Shift+6";
            _sharedViewState.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(_sharedViewState.IsExponent))
                    Checked = _sharedViewState.IsExponent;
            };

            Icon = Resources.exponentation;
            Text = MenuStrings.exponentiationToolStripMenuItem_Text;
            ToolTip = MenuStrings.exponentiationToolStripMenuItem_Text;
        }


        public override void Execute()
        {
            _sharedViewState.IsExponent = !_sharedViewState.IsExponent;
        }
    }
}