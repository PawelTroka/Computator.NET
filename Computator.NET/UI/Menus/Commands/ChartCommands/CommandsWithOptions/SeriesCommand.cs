using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    //TODO: make it work somehow with good API from Chart2D

    internal class SeriesCommand : DummyCommand
    {
        private ISharedViewState _sharedViewState;
        /*private class SeriesOption : ChartOption
 {

     public override void Execute()
     {
         throw new System.NotImplementedException();
     }

     public SeriesOption(ReadOnlyDictionary<CalculationsMode, IChart> charts) : base(charts)
     {
         this.IsOption = true;
     }
 }*/

        public SeriesCommand(ISharedViewState sharedViewState) : base(MenuStrings.series_Text)
        {
            _sharedViewState = sharedViewState;
            BindingUtils.OnPropertyChanged(_sharedViewState, nameof(_sharedViewState.CalculationsMode),
                () => Visible = _sharedViewState.CalculationsMode == CalculationsMode.Real);
        }
    }
}