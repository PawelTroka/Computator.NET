namespace Computator.NET.UI.Controls
{
    public class ChartAreaValuesPresenter
    {
        private IChartAreaValuesView _view;
        public ChartAreaValuesPresenter(IChartAreaValuesView view)
        {
            _view = view;
        }
    }
}