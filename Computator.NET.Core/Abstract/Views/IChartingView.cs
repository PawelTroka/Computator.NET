using Accord.Collections;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Abstract.Views
{
    public interface IChartingView
    {
        IChartAreaValuesView ChartAreaValuesView { get; }
        ReadOnlyDictionary<CalculationsMode, IChart> Charts { get; }
    }
}