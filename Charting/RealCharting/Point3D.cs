namespace Computator.NET.Charting.RealCharting
{
    internal class Point3D : Point2D
    {
        public Point3D()
        {
        }

        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double z { get; set; }
    }
}