#if !__MonoCS__
// base class for 3D chart
// version 0.1

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    internal class Chart3D
    {
        public enum SHAPE
        {
            BAR,
            ELLIPSE,
            CYLINDER,
            CONE,
            PYRAMID
        }

        public static int SHAPE_NO = 5;
        private readonly float m_axisLengthWidthRatio = 200; // axis length / width ratio
        public Color m_axisColor = Color.FromRgb(0, 0, 196); // axis color
        public Color m_axisXColor = Colors.DarkGreen; // axis color
        public Color m_axisYColor = Colors.DarkRed; // axis color
        public Color m_axisZColor = Color.FromRgb(0, 0, 196); // axis color
        protected Vertex3D[] m_vertices; // 3d plot data
        private double m_xAxisCenter; // axis start point
        private double m_xAxisLength; // axis length
        protected float m_xMax; // data range
        protected float m_xMin; // data range
        private double m_yAxisCenter; // axis start point
        private double m_yAxisLength; // axis length
        protected float m_yMax; // data range
        protected float m_yMin; // data range
        private double m_zAxisCenter; // axis start point
        private double m_zAxisLength; // axis length
        protected float m_zMax; // data range
        protected float m_zMin; // data range

        public Vertex3D this[int n]
        {
            get { return m_vertices[n]; }
            set { m_vertices[n] = value; }
        }

        public bool UseAxes { get; set; }

        public float XCenter()
        {
            return (m_xMin + m_xMax)/2;
        }

        public float YCenter()
        {
            return (m_yMin + m_yMax)/2;
        }

        public float XRange()
        {
            return m_xMax - m_xMin;
        }

        public float YRange()
        {
            return m_yMax - m_yMin;
        }

        public float ZRange()
        {
            return m_zMax - m_zMin;
        }

        public float XMin()
        {
            return m_xMin;
        }

        public float XMax()
        {
            return m_xMax;
        }

        public float YMin()
        {
            return m_yMin;
        }

        public float YMax()
        {
            return m_yMax;
        }

        public float ZMin()
        {
            return m_zMin;
        }

        public float ZMax()
        {
            return m_zMax;
        }

        public int GetDataNo()
        {
            if (m_vertices == null)
                return 0;
            return m_vertices.Length;
        }

        public void SetDataNo(int nSize)
        {
            m_vertices = new Vertex3D[nSize];
        }

        public void IncreaseDataSize(int additionalSize)
        {
            if (m_vertices == null)
            {
                SetDataNo(additionalSize);
                return;
            }
            var n_vertices = new Vertex3D[m_vertices.Length + additionalSize];
            for (var i = 0; i < m_vertices.Length; i++)
                n_vertices[i] = m_vertices[i];
            m_vertices = n_vertices;
        }

        public void GetDataRange()
        {
            var nDataNo = GetDataNo();
            if (nDataNo == 0) return;
            m_xMin = float.MaxValue;
            m_yMin = float.MaxValue;
            m_zMin = float.MaxValue;
            m_xMax = float.MinValue;
            m_yMax = float.MinValue;
            m_zMax = float.MinValue;
            for (var i = 0; i < nDataNo; i++)
            {
                var xV = this[i].x;
                var yV = this[i].y;
                var zV = this[i].z;
                if (m_xMin > xV) m_xMin = xV;
                if (m_yMin > yV) m_yMin = yV;
                if (m_zMin > zV) m_zMin = zV;
                if (m_xMax < xV) m_xMax = xV;
                if (m_yMax < yV) m_yMax = yV;
                if (m_zMax < zV) m_zMax = zV;
            }
        }

        public void SetAxes(float x0, float y0, float z0, float xL, float yL, float zL)
        {
            SetAxes((double)x0, y0, z0, xL, yL, zL);
        }

        private void SetAxes(double x0, double y0, double z0, double xL, double yL, double zL)
        {
            m_xAxisLength = xL;
            m_yAxisLength = yL;
            m_zAxisLength = zL;
            m_xAxisCenter = x0;
            m_yAxisCenter = y0;
            m_zAxisCenter = z0;
            //m_bUseAxes = true;
        }

        public void SetAxes()
        {
            SetAxes(0.05f);
        }

        public void SetAxes(float margin)
        {
            // Mesh points are stored as floats, but their difference and the
            // margin may exceed a float even when both endpoints are finite.
            var xRange = (double)m_xMax - m_xMin;
            var yRange = (double)m_yMax - m_yMin;
            var zRange = (double)m_zMax - m_zMin;

            var xC = m_xMin - margin*xRange;
            var yC = m_yMin - margin*yRange;
            var zC = m_zMin - margin*zRange;
            var xL = (1 + 2*margin)*xRange;
            var yL = (1 + 2*margin)*yRange;
            var zL = (1 + 2*margin)*zRange;

            SetAxes(xC, yC, zC, xL, yL, zL);
        }

        internal IReadOnlyList<Chart3DAxis> GetAxes()
        {
            var start = new Point3D(m_xAxisCenter, m_yAxisCenter, m_zAxisCenter);
            // The independent projection scales each coordinate separately. A
            // single average world radius would become enormous on a short axis
            // when another range is large. Equal spans retain the old radius.
            var fallbackLength = Math.Max(m_xAxisLength, Math.Max(m_yAxisLength, m_zAxisLength));
            var radii = new Vector3D(
                (m_xAxisLength > 0 ? m_xAxisLength : fallbackLength) / m_axisLengthWidthRatio,
                (m_yAxisLength > 0 ? m_yAxisLength : fallbackLength) / m_axisLengthWidthRatio,
                (m_zAxisLength > 0 ? m_zAxisLength : fallbackLength) / m_axisLengthWidthRatio);
            var xEnd = new Point3D(m_xAxisCenter + m_xAxisLength, m_yAxisCenter, m_zAxisCenter);
            var yEnd = new Point3D(m_xAxisCenter, m_yAxisCenter + m_yAxisLength, m_zAxisCenter);
            var zEnd = new Point3D(m_xAxisCenter, m_yAxisCenter, m_zAxisCenter + m_zAxisLength);
            return new[]
            {
                new Chart3DAxis(0, start.X, xEnd.X, start, xEnd, radii),
                new Chart3DAxis(1, start.Y, yEnd.Y, start, yEnd, radii),
                new Chart3DAxis(2, start.Z, zEnd.Z, start, zEnd, radii)
            };
        }

        public void SetAxesColor(Color? color = null)
        {
            if (color.HasValue)
                m_axisColor = m_axisXColor = m_axisYColor = m_axisZColor = color.Value; // axis color
            else
                m_axisColor = m_axisXColor = m_axisYColor = m_axisZColor = Color.FromRgb(0, 0, 196); // axis color
        }

        // Use the same principal-axis geometry as the numeric annotation bounds.
        public void AddAxesMeshes(ArrayList meshs)
        {
            if (!UseAxes) return;
            foreach (var axis in GetAxes())
            {
                var color = axis.Index == 0 ? m_axisXColor : axis.Index == 1 ? m_axisYColor : m_axisZColor;
                meshs.Add(axis.CreateCylinder(color));
                meshs.Add(axis.CreateArrow(color));
            }
        }

        private void AddAxesLabelsMeshes(ArrayList meshs)
        {
            var radius = (m_xAxisLength + m_yAxisLength + m_zAxisLength)/(3*m_axisLengthWidthRatio);

            Mesh3D xAxisCylinder = new Cylinder3D(radius, radius, m_xAxisLength/10, 6);
            xAxisCylinder.SetColor(m_axisXColor);
            TransformMatrix.Transform(xAxisCylinder,
                new Point3D(m_xAxisCenter/15 + m_xAxisLength/10/2, m_yAxisCenter/15, m_zAxisCenter/15), 0, 90);
            meshs.Add(xAxisCylinder);

            Mesh3D xAxisCone = new Cone3D(2*radius, 2*radius, radius*5, 6);
            xAxisCone.SetColor(m_axisXColor);
            TransformMatrix.Transform(xAxisCone,
                new Point3D(m_xAxisCenter/15 + m_xAxisLength/10, m_yAxisCenter/15, m_zAxisCenter/15), 0, 90);
            meshs.Add(xAxisCone);

            Mesh3D yAxisCylinder = new Cylinder3D(radius, radius, m_yAxisLength/10, 6);
            yAxisCylinder.SetColor(m_axisYColor);
            TransformMatrix.Transform(yAxisCylinder,
                new Point3D(m_xAxisCenter/15, m_yAxisCenter/15 + m_yAxisLength/10/2, m_zAxisCenter/15), 90, 90);
            meshs.Add(yAxisCylinder);

            Mesh3D yAxisCone = new Cone3D(2*radius, 2*radius, radius*5, 6);
            yAxisCone.SetColor(m_axisYColor);
            TransformMatrix.Transform(yAxisCone,
                new Point3D(m_xAxisCenter/15, m_yAxisCenter/15 + m_yAxisLength/10, m_zAxisCenter/15), 90, 90);
            meshs.Add(yAxisCone);

            Mesh3D zAxisCylinder = new Cylinder3D(radius, radius, m_zAxisLength/10, 6);
            zAxisCylinder.SetColor(m_axisZColor);
            TransformMatrix.Transform(zAxisCylinder,
                new Point3D(m_xAxisCenter/15, m_yAxisCenter/15, m_zAxisCenter/15 + m_zAxisLength/10/2), 0, 0);
            meshs.Add(zAxisCylinder);

            Mesh3D zAxisCone = new Cone3D(2*radius, 2*radius, radius*5, 6);
            zAxisCone.SetColor(m_axisZColor);
            TransformMatrix.Transform(zAxisCone,
                new Point3D(m_xAxisCenter/15, m_yAxisCenter/15, m_zAxisCenter/15 + m_zAxisLength/10), 0, 0);
            meshs.Add(zAxisCone);
        }

        // select 
        public virtual void Select(ViewportRect rect, TransformMatrix matrix, Viewport3D viewport3d)
        {
        }

        // highlight selected model
        public virtual void HighlightSelection(MeshGeometry3D meshGeometry, Color selectColor)
        {
        }
    }
}
#endif
