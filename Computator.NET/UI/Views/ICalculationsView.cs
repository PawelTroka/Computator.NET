// Computator.NET Copyright © 2016 - 2016 Pawel Troka

using System;
using Computator.NET.Evaluation;

namespace Computator.NET.UI.Views
{
    public interface ICalculationsView
    {
        CalculationsMode CalculationsMode { get; set; }
        event EventHandler ModeChanged;

        string XLabel { set; }
        string YLabel { set; }

        bool YVisible { set; }
    }
}