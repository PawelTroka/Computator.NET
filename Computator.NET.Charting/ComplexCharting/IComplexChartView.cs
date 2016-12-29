// Computator.NET.Charting Copyright © 2016 - 2016 Pawel Troka

using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Charting.ComplexCharting
{
    public interface IComplexChartView : IChart
    {
        ComplexChartPresenter Presenter { get; set; }
    }
}