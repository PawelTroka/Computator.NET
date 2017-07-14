namespace Computator.NET.DataTypes.Charts
{
    public class Point3D
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
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }
}