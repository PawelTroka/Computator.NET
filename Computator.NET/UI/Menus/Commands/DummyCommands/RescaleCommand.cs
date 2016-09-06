using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    internal class RescaleCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public RescaleCommand(ISharedViewState sharedViewState) : base(MenuStrings.rescale_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Fxy;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Fxy);
        }
    }
}