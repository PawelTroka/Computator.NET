using System.Collections.Generic;
using Computator.NET.Core.Helpers;
using Computator.NET.Core.Menu.Commands.DummyCommands;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Charts;
using Computator.NET.DataTypes.Events;

namespace Computator.NET.Core.Menu.Commands.TopMenuCommands
{
    public class TransformCommand : DummyCommand
    {
        private readonly ISharedViewState _sharedViewState;
        public TransformCommand(ISharedViewState sharedViewState, IChart2D chart2d, IComplexChart complexChart, IChart3D chart3d) : base(MenuStrings.transformToolStripMenuItem_Text)
        {
            _sharedViewState = sharedViewState;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CurrentView),
                () => IsEnabled = _sharedViewState.CurrentView == ViewName.Charting);

            ChildrenCommands = new List<IToolbarCommand>
            {
                new TransformOptionCommand(MenuStrings.FFT_Text, MenuStrings.FFT_ToolTip_Text,
                    chart2d,complexChart,chart3d, sharedViewState),
                new TransformOptionCommand(MenuStrings.IFFT_Text, MenuStrings.IFFT_ToolTipText,
                    chart2d,complexChart,chart3d, sharedViewState),
                new TransformOptionCommand(MenuStrings.DST_Text, MenuStrings.DST_ToolTipText,
                    chart2d,complexChart,chart3d, sharedViewState),
                new TransformOptionCommand(MenuStrings.IDST_Text, MenuStrings.IDST_ToolTipText,
                    chart2d,complexChart,chart3d, sharedViewState),
                new TransformOptionCommand(MenuStrings.DCT_Text, MenuStrings.DCT_ToolTipText,
                    chart2d,complexChart,chart3d, sharedViewState),
                new TransformOptionCommand(MenuStrings.IDCT_Text, MenuStrings.IDCT_ToolTipText,
                    chart2d,complexChart,chart3d, sharedViewState),
                new TransformOptionCommand(MenuStrings.FHT_Text, MenuStrings.FHT_ToolTipText,
                    chart2d,complexChart,chart3d, sharedViewState),
                new TransformOptionCommand(MenuStrings.IFHT_Text, MenuStrings.IFHT_ToolTipText,
                    chart2d,complexChart,chart3d, sharedViewState),
                new TransformOptionCommand(MenuStrings.DHT_Text, MenuStrings.DHT_ToolTipText,
                    chart2d,complexChart,chart3d, sharedViewState)
                //      new TransformOptionCommand(MenuStrings.,MenuStrings.IFHT_ToolTipText,charts),
            };
        }
    }
}