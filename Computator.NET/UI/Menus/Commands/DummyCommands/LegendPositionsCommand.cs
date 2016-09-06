using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    internal class LegendPositionsCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public LegendPositionsCommand(ISharedViewState sharedViewState) : base(MenuStrings.legendPositions_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real);
        }
    }
}