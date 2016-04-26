using System;
using Computator.NET.Charting;

namespace Computator.NET.UI.Interfaces
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