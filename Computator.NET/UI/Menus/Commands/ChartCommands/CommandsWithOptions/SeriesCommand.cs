using Computator.NET.DataTypes;
using Computator.NET.UI.Menus.Commands.DummyCommands;

namespace Computator.NET.UI.Menus.Commands.ChartCommands.CommandsWithOptions
{
    //TODO: make it work somehow with good API from Chart2D

    internal class SeriesCommand : DummyCommand
    {
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

        public SeriesCommand() : base(MenuStrings.series_Text)
        {
            BindingUtils.OnPropertyChanged(SharedViewState.Instance, nameof(SharedViewState.Instance.CalculationsMode),
                () => Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real);
        }
    }
}