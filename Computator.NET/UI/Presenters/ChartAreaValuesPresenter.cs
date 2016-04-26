using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Presenters
{
    public class ChartAreaValuesPresenter
    {
        private readonly IChartAreaValuesView _view;

        public ChartAreaValuesPresenter(IChartAreaValuesView view)
        {
            _view = view;
            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(OnCalculationsModeChangedEvent);
        }

        private void OnCalculationsModeChangedEvent(CalculationsModeChangedEvent e)
        {
            _view.AddChartLabel = e.CalculationsMode == CalculationsMode.Complex
                ? Strings.DrawChart
                : Strings.AddToChart;
        }
    }
}