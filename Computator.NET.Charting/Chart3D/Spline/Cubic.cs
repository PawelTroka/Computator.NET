namespace Computator.NET.Charting.Chart3D.Spline
{
    public class Cubic
    {
        private readonly double a;
        private readonly double b;
        private readonly double c;
        private readonly double d;

        public Cubic(double a, double b, double c, double d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public double eval(double u)
        {
            return ((d*u + c)*u + b)*u + a;
        }
    }
}