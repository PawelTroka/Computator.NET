using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class RescaleCommand : DummyCommand
    {
        public RescaleCommand() : base(MenuStrings.rescale_Text)
        {
            this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Fxy;
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Fxy);
        }
    }
}