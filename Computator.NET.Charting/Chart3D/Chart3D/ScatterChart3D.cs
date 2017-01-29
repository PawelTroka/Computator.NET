#if !__MonoCS__
// class of 3d scatter plot .
// version 0.1

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    internal class ScatterChart3D : Chart3D
    {
        public ScatterPlotItem Get(int n)
        {
            return (ScatterPlotItem) m_vertices[n];
        }

        public void SetVertex(int n, ScatterPlotItem value)
        {
            m_vertices[n] = value;
        }

        public ScatterPlotItem GetVertex(int n)
        {
            return (ScatterPlotItem) m_vertices[n];
        }

        // convert the 3D scatter plot into a array of Mesh3D object
        public ArrayList GetMeshes()
        {
            var nDotNo = GetDataNo();
            if (nDotNo == 0) return null;
            var meshs = new ArrayList();

            var nVertIndex = 0;
            for (var i = 0; i < nDotNo; i++)
            {
                var plotItem = Get(i);
                var nType = plotItem.shape%SHAPE_NO;
                var w = plotItem.w;
                var h = plotItem.h;
                Mesh3D dot;
                m_vertices[i].nMinI = nVertIndex;
                switch (nType)
                {
                    case (int) SHAPE.BAR:
                        dot = new Bar3D(0, 0, 0, w, w, h);
                        break;
                    case (int) SHAPE.CONE:
                        dot = new Cone3D(w, w, h, 7);
                        break;
                    case (int) SHAPE.CYLINDER:
                        dot = new Cylinder3D(w, w, h, 14);
                        break;
                    case (int) SHAPE.ELLIPSE:
                        dot = new Ellipse3D(w, w, h, 7);
                        break;
                    case (int) SHAPE.PYRAMID:
                        dot = new Pyramid3D(w, w, h);
                        break;
                    default:
                        dot = new Bar3D(0, 0, 0, w, w, h);
                        break;
                }
                nVertIndex += dot.GetVertexNo();
                m_vertices[i].nMaxI = nVertIndex - 1;

                TransformMatrix.Transform(dot, new Point3D(plotItem.x, plotItem.y, plotItem.z), plotItem.aX, plotItem.aZ);
                dot.SetColor(plotItem.color);
                meshs.Add(dot);
            }
            AddAxesMeshes(meshs);

            return meshs;
        }

        // selection
        public override void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3d)
        {
            var nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            var xMin = rect.XMin();
            var xMax = rect.XMax();
            var yMin = rect.YMin();
            var yMax = rect.YMax();

            for (var i = 0; i < nDotNo; i++)
            {
                var plotItem = Get(i);
                var pt = matrix.VertexToViewportPt(new Point3D(plotItem.x, plotItem.y, plotItem.z),
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
            var nDotNo = GetDataNo();
            if (nDotNo == 0) return;

            Point mapPt;
            for (var i = 0; i < nDotNo; i++)
            {
                if (m_vertices[i].selected)
                {
                    mapPt = TextureMapping.GetMappingPosition(selectColor, false);
                }
                else
                {
                    mapPt = TextureMapping.GetMappingPosition(m_vertices[i].color, false);
                }
                var nMin = m_vertices[i].nMinI;
                var nMax = m_vertices[i].nMaxI;
                for (var j = nMin; j <= nMax; j++)
                {
                    meshGeometry.TextureCoordinates[j] = mapPt;
                }
            }
        }
    }
}
#endif