using System.Collections;
using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Abstract.Views
{
    public interface IChartingView
    {
        IChartAreaValuesView ChartAreaValuesView { get; }
        IChart2D Chart2D { get; }
        IComplexChart ComplexChart { get; }
        IChart3D Chart3D { get; }
        IDictionary<CalculationsMode,IChart> Charts { get; }
    }
}