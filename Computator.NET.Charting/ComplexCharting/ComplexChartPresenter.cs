// Computator.NET.Charting Copyright © 2016 - 2016 Pawel Troka

namespace Computator.NET.Charting.ComplexCharting
{
    public class ComplexChartPresenter
    {
        private readonly IComplexChartView _view;

        public ComplexChartPresenter(IComplexChartView view)
        {
            _view = view;
            _view.Presenter = this;
        }
    }
}