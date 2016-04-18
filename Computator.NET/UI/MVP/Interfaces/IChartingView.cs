using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Evaluation;
using Computator.NET.UI.Controls;

namespace Computator.NET.UI.MVP.Views
{
    public interface IChartingView
    {
        IChartAreaValuesView ChartAreaValuesView { get; }
        ReadOnlyDictionary<CalculationsMode, IChart> Charts { get; }
    }
}