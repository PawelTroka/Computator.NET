using System.Collections.Generic;
using Computator.NET.Core.UI.Menus;
using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.ChartCommands;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
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