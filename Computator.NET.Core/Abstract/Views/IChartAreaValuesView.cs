using System;
using Computator.NET.DataTypes.Charts;

namespace Computator.NET.Core.Abstract.Views
{
    public interface IChartAreaValuesView : IAreaValues
    {
        string AddChartLabel { set; }

        double Quality { get; }

        event EventHandler AddClicked;
        event EventHandler ClearClicked;
        event EventHandler QualityChanged;

        void SetError(string property, string error);
    }
}