using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Evaluation;

namespace Computator.NET.UI.Controls
{
    public class ChartAreaValuesPresenter
    {
        private IChartAreaValuesView _view;
        public ChartAreaValuesPresenter(IChartAreaValuesView view)
        {
            _view = view;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(OnCalculationsModeChangedEvent);
        }

        private void OnCalculationsModeChangedEvent(CalculationsModeChangedEvent e)
        {
            _view.AddChartLabel = e.CalculationsMode == CalculationsMode.Complex ? Strings.DrawChart : Strings.AddToChart;
        }
    }
}