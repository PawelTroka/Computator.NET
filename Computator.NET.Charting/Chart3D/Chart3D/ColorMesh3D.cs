// class for a color 3d mesh model. (each vertex can have different color)
// version 0.1

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D
{
    public class ColorMesh3D : Mesh3D
    {
        private Color[] m_colors; // color information of each vertex
        // override the set vertex number, since we include the color information for each vertex
        public override void SetVertexNo(int nSize)
        {
            m_points = new Point3D[nSize];
            m_colors = new Color[nSize];
        }

        // get color information of each vertex
        public override Color GetColor(int nV)
        {
            return m_colors[nV];
        }

        // set color information of each vertex
        public void SetColor(int nV, byte r, byte g, byte b)
        {
            m_colors[nV] = Color.FromRgb(r, g, b);
        }

        public void SetColor(int nV, Color color)
        {
            m_colors[nV] = color;
        }
    }
}