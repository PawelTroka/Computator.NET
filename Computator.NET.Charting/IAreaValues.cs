using System;

namespace Computator.NET.UI.Controls
{
    public interface IAreaValues
    {
        double XMin { get; set; }
        double XMax { get; set; }
        double YMin { get; set; }
        double YMax { get; set; }

        event EventHandler XMinChanged;
        event EventHandler XMaxChanged;
        event EventHandler YMinChanged;
        event EventHandler YMaxChanged;
    }
}