using Computator.NET.Evaluation;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    //TODO: make it work somehow with good API from Chart2D

    class SeriesCommand : DummyCommand
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
                () => this.Visible = SharedViewState.Instance.CalculationsMode == CalculationsMode.Real);
        }
    }
}