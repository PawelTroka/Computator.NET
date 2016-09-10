using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.UI.Menus.Commands.ChartCommands;
using Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions;

namespace Computator.NET.UI.Menus.Commands.DummyCommands
{
    internal class ChartCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public ChartCommand(ReadOnlyDictionary<CalculationsMode, IChart> charts, ISharedViewState sharedViewState) : base(MenuStrings.chartToolStripMenuItem_Text)
        {
            _sharedViewState = sharedViewState;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CurrentView),
                () => IsEnabled = _sharedViewState.CurrentView == ViewName.Charting);

            ChildrenCommands = new List<IToolbarCommand>
            {
                new ExportCommand(charts, sharedViewState),
                null,
                //rozne
                new TypeOfChartCommand(charts, sharedViewState),
                new ColorsCommand(charts, sharedViewState),
                new LegendPositionsCommand(sharedViewState)
                {
                    ChildrenCommands = new List<IToolbarCommand>
                    {
                        new LegendPlacementCommand(charts,sharedViewState),
                        new LegendAligmentCommand(charts, sharedViewState)
                    }
                },
                new ContourLinesCommand(charts, sharedViewState),
                new ColorAssigmentCommand(charts, sharedViewState),
                new RescaleCommand(sharedViewState)
                {
                    ChildrenCommands = new List<IToolbarCommand>
                    {
                        new FitAxesCommand(charts, sharedViewState),
                        new EqualAxesCommand(charts, sharedViewState)
                    }
                },
                null,
                new EditChartCommand(charts, sharedViewState),
                new EditChartPropertiesCommand(charts, sharedViewState),
                null,
                new PrintChartCommand(charts, sharedViewState),
                new PrintPreviewChartCommand(charts, sharedViewState)
            };
        }
    }

    internal class TransformCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        public TransformCommand(ISharedViewState sharedViewState, ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(MenuStrings.transformToolStripMenuItem_Text)
        {
            _sharedViewState = sharedViewState;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CurrentView),
                () => IsEnabled = _sharedViewState.CurrentView == ViewName.Charting);

            ChildrenCommands = new List<IToolbarCommand>
            {
                new TransformOptionCommand(MenuStrings.FFT_Text, MenuStrings.FFT_ToolTip_Text,
                    charts, sharedViewState),
                new TransformOptionCommand(MenuStrings.IFFT_Text, MenuStrings.IFFT_ToolTipText,
                    charts, sharedViewState),
                new TransformOptionCommand(MenuStrings.DST_Text, MenuStrings.DST_ToolTipText,
                    charts, sharedViewState),
                new TransformOptionCommand(MenuStrings.IDST_Text, MenuStrings.IDST_ToolTipText,
                    charts, sharedViewState),
                new TransformOptionCommand(MenuStrings.DCT_Text, MenuStrings.DCT_ToolTipText,
                    charts, sharedViewState),
                new TransformOptionCommand(MenuStrings.IDCT_Text, MenuStrings.IDCT_ToolTipText,
                    charts, sharedViewState),
                new TransformOptionCommand(MenuStrings.FHT_Text, MenuStrings.FHT_ToolTipText,
                    charts, sharedViewState),
                new TransformOptionCommand(MenuStrings.IFHT_Text, MenuStrings.IFHT_ToolTipText,
                    charts, sharedViewState),
                new TransformOptionCommand(MenuStrings.DHT_Text, MenuStrings.DHT_ToolTipText,
                    charts, sharedViewState)
                //      new TransformOptionCommand(MenuStrings.,MenuStrings.IFHT_ToolTipText,charts),
            };
        }
    }
}