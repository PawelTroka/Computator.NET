using Computator.NET.DataTypes;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    internal class RescaleCommand : DummyCommand
    {
        public RescaleCommand() : base(MenuStrings.rescale_Text)
        {
            Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Fxy;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Fxy);
        }
    }
}