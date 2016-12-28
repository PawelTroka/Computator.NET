using System.Collections.Generic;
using Computator.NET.Core.Menu;
using Computator.NET.DataTypes.Events;
using Computator.NET.UI.Menus.Commands.ChartCommands;
using Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    public class ChartCommand : DummyCommand
    {
        private readonly ISharedViewState _sharedViewState;

        public ChartCommand(ISharedViewState sharedViewState, ExportCommand exportCommand,
            TypeOfChartCommand typeOfChartCommand,
            ColorsCommand colorsCommand,
            LegendPositionsCommand legendPositionsCommand, ContourLinesCommand contourLinesCommand,
            ColorAssigmentCommand colorAssigmentCommand,
            RescaleCommand rescaleCommand, EditChartCommand editChartCommand,
            EditChartPropertiesCommand editChartPropertiesCommand,
            PrintChartCommand printChartCommand, PrintPreviewChartCommand printPreviewChartCommand)
            : base(MenuStrings.chartToolStripMenuItem_Text)
        {
            _sharedViewState = sharedViewState;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CurrentView),
                () => IsEnabled = _sharedViewState.CurrentView == ViewName.Charting);

            ChildrenCommands = new List<IToolbarCommand>
            {
                exportCommand,
                null,
                //rozne
                typeOfChartCommand,
                colorsCommand,
                legendPositionsCommand,
                contourLinesCommand,
                colorAssigmentCommand,
                rescaleCommand,
                null,
                editChartCommand,
                editChartPropertiesCommand,
                null,
                printChartCommand,
                printPreviewChartCommand
            };
        }
    }
}