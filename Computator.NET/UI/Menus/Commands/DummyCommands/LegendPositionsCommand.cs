using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class LegendPositionsCommand : DummyCommand
    {
        public LegendPositionsCommand() : base(MenuStrings.legendPositions_Text)
        {
            this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real);
        }
    }
}