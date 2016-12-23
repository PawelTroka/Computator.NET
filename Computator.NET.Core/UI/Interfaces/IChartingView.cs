using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.DataTypes;

namespace Computator.NET.UI.Interfaces
{
    public interface IChartingView
    {
        IChartAreaValuesView ChartAreaValuesView { get; }
        ReadOnlyDictionary<CalculationsMode, IChart> Charts { get; }
    }
}