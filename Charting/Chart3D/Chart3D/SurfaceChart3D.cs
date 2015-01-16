// class of general surface chart, not ready yet
// a few function will be used in child class
// version 0.1


using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D
{
    internal class SurfaceChart3D : Chart3D
    {
        // selection
        public override void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3d)
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            double xMin = rect.XMin();
            double xMax = rect.XMax();
            double yMin = rect.YMin();
            double yMax = rect.YMax();

            for (int i = 0; i < nDotNo; i++)
            {
                Point pt = matrix.VertexToViewportPt(new Point3D(m_vertices[i].x, m_vertices[i].y, m_vertices[i].z),
                    viewport3d);

                if ((pt.X > xMin) && (pt.X < xMax) && (pt.Y > yMin) && (pt.Y < yMax))
                {
                    m_vertices[i].selected = true;
                }
                else
                {
                    m_vertices[i].selected = false;
                }
            }
        }

        // highlight the selection
        public override void HighlightSelection(MeshGeometry3D meshGeometry, Color selectColor)
        {
            int nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            Point mapPt;
            for (int i = 0; i < nDotNo; i++)
            {
                if (m_vertices[i].selected)
                {
                    mapPt = TextureMapping.GetMappingPosition(selectColor, true);
                }
                else
                {
                    mapPt = TextureMapping.GetMappingPosition(m_vertices[i].color, true);
                }
                int nMin = m_vertices[i].nMinI;
                meshGeometry.TextureCoordinates[nMin] = mapPt;
            }
        }
    }
}