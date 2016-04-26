// a dot in scatter plot.
// version 0.1


namespace Computator.NET.Charting.Chart3D.Chart3D
{
    internal class ScatterPlotItem : Vertex3D
    {
        public double aX = 0, aZ = 0;
        public float h; // size of the dot, (xy direction and z direction)
        public int shape; // shape of the dot
        public float w; // size of the dot, (xy direction and z direction)
    }
}