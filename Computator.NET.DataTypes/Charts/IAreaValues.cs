using System;

namespace Computator.NET.DataTypes.Charts
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