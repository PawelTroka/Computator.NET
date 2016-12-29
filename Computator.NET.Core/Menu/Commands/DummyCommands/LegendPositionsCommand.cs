using System.Collections.Generic;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Menu.Commands.ChartCommands.CommandsWithOptions;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Menu.Commands.DummyCommands
{
    public class LegendPositionsCommand : DummyCommand
    {
        private readonly ISharedViewState _sharedViewState;
        public LegendPositionsCommand(ISharedViewState sharedViewState, LegendPlacementCommand legendPlacementCommand, LegendAligmentCommand legendAligmentCommand) : base(MenuStrings.legendPositions_Text)
        {
            _sharedViewState = sharedViewState;
            Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real);

            ChildrenCommands = new List<IToolbarCommand>
            {
                legendPlacementCommand,
                legendAligmentCommand
            };
        }
    }
}