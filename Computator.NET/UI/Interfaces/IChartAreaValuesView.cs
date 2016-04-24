using System;

namespace Computator.NET.UI.Controls
{
    public interface IChartAreaValuesView : IAreaValues
    {
        string AddChartLabel { set; }

        double Quality { get; }

        event EventHandler AddClicked;
        event EventHandler ClearClicked;
        event EventHandler QualityChanged;
    }
}