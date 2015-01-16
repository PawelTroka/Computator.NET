using System.Numerics;

namespace Computator.NET.Charting.ComplexCharting
{
    public class ComplexPoint
    {
        public ComplexPoint(Complex z, Complex fz)
        {
            Z = z;
            Fz = fz;
        }

        public Complex Z { get; set; }
        public Complex Fz { get; set; }
    }
}