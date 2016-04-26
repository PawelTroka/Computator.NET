using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    internal class LegendPositionsCommand : DummyCommand
    {
        public LegendPositionsCommand() : base(MenuStrings.legendPositions_Text)
        {
            Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real);
        }
    }
}