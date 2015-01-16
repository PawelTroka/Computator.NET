// class for draw 2D rectangular in Viewport3D.
// version 0.1

//      vertex index
//      0  -------------------- 1
//      |   -----------------   |
//      |   | 4            5 |  |
//      |   |                |  |
//      |   |                |  |
//      |   | 7            6 |  |
//      |   -----------------   |
//      3  -------------------- 2

// the coordinate is in viewport3D coordinate, 

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D
{
    internal class ViewportRect : Mesh3D
    {
        public double m_lineWidth = 0.005; // line width of the rect (% of the window width)
        private double m_x1; // x value of the one corner of the rect
        private double m_x2; // x value of the another corner of the rect
        private double m_y1; // y value of the one corner of the rect
        private double m_y2; // y value of the another corner of the rect
        public double m_zLevel = 1.0; // z value of the rect

        public ViewportRect()
        {
            SetSize(8, 8);
            SetTriangle(0, 0, 4, 1);
            SetTriangle(1, 1, 4, 5);
            SetTriangle(2, 1, 5, 2);
            SetTriangle(3, 2, 5, 6);
            SetTriangle(4, 2, 6, 3);
            SetTriangle(5, 3, 6, 7);
            SetTriangle(6, 0, 3, 7);
            SetTriangle(7, 0, 7, 4);
            SetColor(255, 0, 0);
        }

        // set rectangle (input parameter are the center and size of the rect)
        private void SetRect(double xC, double yC, double w, double h)
        {
            SetPoint(0, xC - w/2, yC + h/2, m_zLevel);
            SetPoint(1, xC + w/2, yC + h/2, m_zLevel);
            SetPoint(2, xC + w/2, yC - h/2, m_zLevel);
            SetPoint(3, xC - w/2, yC - h/2, m_zLevel);
            SetPoint(4, xC - w/2 + m_lineWidth, yC + h/2 - m_lineWidth, m_zLevel);
            SetPoint(5, xC + w/2 - m_lineWidth, yC + h/2 - m_lineWidth, m_zLevel);
            SetPoint(6, xC + w/2 - m_lineWidth, yC - h/2 + m_lineWidth, m_zLevel);
            SetPoint(7, xC - w/2 + m_lineWidth, yC - h/2 + m_lineWidth, m_zLevel);
        }

        private void SetRect()
        {
            double xC = (m_x1 + m_x2)/2;
            double yC = (m_y1 + m_y2)/2;
            double w = Math.Abs(m_x2 - m_x1);
            double h = Math.Abs(m_y2 - m_y1);
            SetRect(xC, yC, w, h);
        }

        public void SetRect(Point pt1, Point pt2)
        {
            m_x1 = pt1.X;
            m_y1 = pt1.Y;
            m_x2 = pt2.X;
            m_y2 = pt2.Y;
            SetRect();
        }

        // return a mesh array of this rect (for display in the Viewport3D)
        public ArrayList GetMeshes()
        {
            var meshs = new ArrayList();
            meshs.Add(this);

            int nVertNo = GetVertexNo();
            for (int i = 0; i < nVertNo; i++)
            {
                m_vertIndices[i] = i;
            }
            return meshs;
        }

        // mouse down message handler
        // input parameter
        // 1. mouse down location
        // 2. Viewport 3D 
        // 3. the model index of the rect in the viewport 
        public void OnMouseDown(Point pt, Viewport3D viewport3d, int nModelIndex)
        {
            if (nModelIndex == -1) return;
            MeshGeometry3D meshGeometry = Model3D.GetGeometry(viewport3d, nModelIndex);
            if (meshGeometry == null) return;

            Point pt1 = TransformMatrix.ScreenPtToViewportPt(pt, viewport3d);

            SetRect(pt1, pt1);
            UpdatePositions(meshGeometry);
        }

        // mouse move message handler
        public void OnMouseMove(Point pt, Viewport3D viewport3d, int nModelIndex)
        {
            if (nModelIndex == -1) return;
            MeshGeometry3D meshGeometry = Model3D.GetGeometry(viewport3d, nModelIndex);
            if (meshGeometry == null) return;
            Point pt2 = TransformMatrix.ScreenPtToViewportPt(pt, viewport3d);
            m_x2 = pt2.X;
            m_y2 = pt2.Y;
            SetRect();
            UpdatePositions(meshGeometry);
        }

        // the data rangel of the rect
        public double XMin()
        {
            if (m_x1 < m_x2) return m_x1;
            return m_x2;
        }

        public double XMax()
        {
            if (m_x1 < m_x2) return m_x2;
            return m_x1;
        }

        public double YMin()
        {
            if (m_y1 < m_y2) return m_y1;
            return m_y2;
        }

        public double YMax()
        {
            if (m_y1 < m_y2) return m_y2;
            return m_y1;
        }
    }
}