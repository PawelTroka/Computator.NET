using System.Collections.Generic;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Menu.Commands.ChartCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.DummyCommands
{
    public class RescaleCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public RescaleCommand(ISharedViewState sharedViewState, FitAxesCommand fitAxesCommand, EqualAxesCommand equalAxesCommand) : base(MenuStrings.rescale_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Fxy;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Fxy);

            ChildrenCommands = new List<IToolbarCommand>
            {
                fitAxesCommand,
                equalAxesCommand
            };
        }
    }
}