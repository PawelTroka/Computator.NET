using System;
using System.ComponentModel;

namespace Computator.NET.UI.Controls
{
    public interface IChartAreaValuesView : INotifyPropertyChanged
    {
        decimal XMin { get; set; }
        decimal XMax { get; set; }
        decimal YMin { get; set; }
        decimal YMax { get; set; }



        string AddChartLabel { set; }

        double Quality { get; }

        event EventHandler AddClicked;
        event EventHandler ClearClicked;
        event EventHandler QualityChanged;
    }
}