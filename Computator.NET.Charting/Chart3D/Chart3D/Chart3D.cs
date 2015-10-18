// base class for 3D chart
// version 0.1

using System.Collections;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D
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
        };

        public static int SHAPE_NO = 5;
        private readonly float m_axisLengthWidthRatio = 200; // axis length / width ratio
        public Color m_axisColor = Color.FromRgb(0, 0, 196); // axis color
        public Color m_axisXColor = Colors.DarkGreen; // axis color
        public Color m_axisYColor = Colors.DarkRed; // axis color
        public Color m_axisZColor = Color.FromRgb(0, 0, 196); // axis color
        protected Vertex3D[] m_vertices; // 3d plot data
        private float m_xAxisCenter; // axis start point
        private float m_xAxisLength; // axis length
        protected float m_xMax; // data range
        protected float m_xMin; // data range
        private float m_yAxisCenter; // axis start point
        private float m_yAxisLength; // axis length
        protected float m_yMax; // data range
        protected float m_yMin; // data range
        private float m_zAxisCenter; // axis start point
        private float m_zAxisLength; // axis length
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
            return m_vertices.Length;
        }

        public void SetDataNo(int nSize)
        {
            m_vertices = new Vertex3D[nSize];
        }

        public void IncreaseDataSize(int additionalSize)
        {
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
            var xRange = m_xMax - m_xMin;
            var yRange = m_yMax - m_yMin;
            var zRange = m_zMax - m_zMin;

            var xC = m_xMin - margin*xRange;
            var yC = m_yMin - margin*yRange;
            var zC = m_zMin - margin*zRange;
            var xL = (1 + 2*margin)*xRange;
            var yL = (1 + 2*margin)*yRange;
            var zL = (1 + 2*margin)*zRange;

            SetAxes(xC, yC, zC, xL, yL, zL);
        }

        public void SetAxesColor(Color? color = null)
        {
            if (color.HasValue)
                m_axisColor = m_axisXColor = m_axisYColor = m_axisZColor = color.Value; // axis color
            else
                m_axisColor = m_axisXColor = m_axisYColor = m_axisZColor = Color.FromRgb(0, 0, 196); // axis color
        }

        // add the axes mesh to the Mesh3D array
        // if you are using the projection matrix which is not uniform along all the axess, you need change this function
        public void AddAxesMeshes(ArrayList meshs)
        {
            if (!UseAxes) return;

            var radius = (m_xAxisLength + m_yAxisLength + m_zAxisLength)/(3*m_axisLengthWidthRatio);

            Mesh3D xAxisCylinder = new Cylinder3D(radius, radius, m_xAxisLength, 6);
            xAxisCylinder.SetColor(m_axisXColor);
            TransformMatrix.Transform(xAxisCylinder,
                new Point3D(m_xAxisCenter + m_xAxisLength/2, m_yAxisCenter, m_zAxisCenter), 0, 90);
            meshs.Add(xAxisCylinder);

            Mesh3D xAxisCone = new Cone3D(2*radius, 2*radius, radius*5, 6);
            xAxisCone.SetColor(m_axisXColor);
            TransformMatrix.Transform(xAxisCone,
                new Point3D(m_xAxisCenter + m_xAxisLength, m_yAxisCenter, m_zAxisCenter), 0, 90);
            meshs.Add(xAxisCone);

            Mesh3D yAxisCylinder = new Cylinder3D(radius, radius, m_yAxisLength, 6);
            yAxisCylinder.SetColor(m_axisYColor);
            TransformMatrix.Transform(yAxisCylinder,
                new Point3D(m_xAxisCenter, m_yAxisCenter + m_yAxisLength/2, m_zAxisCenter), 90, 90);
            meshs.Add(yAxisCylinder);

            Mesh3D yAxisCone = new Cone3D(2*radius, 2*radius, radius*5, 6);
            yAxisCone.SetColor(m_axisYColor);
            TransformMatrix.Transform(yAxisCone,
                new Point3D(m_xAxisCenter, m_yAxisCenter + m_yAxisLength, m_zAxisCenter), 90, 90);
            meshs.Add(yAxisCone);

            Mesh3D zAxisCylinder = new Cylinder3D(radius, radius, m_zAxisLength, 6);
            zAxisCylinder.SetColor(m_axisZColor);
            TransformMatrix.Transform(zAxisCylinder,
                new Point3D(m_xAxisCenter, m_yAxisCenter, m_zAxisCenter + m_zAxisLength/2), 0, 0);
            meshs.Add(zAxisCylinder);

            Mesh3D zAxisCone = new Cone3D(2*radius, 2*radius, radius*5, 6);
            zAxisCone.SetColor(m_axisZColor);
            TransformMatrix.Transform(zAxisCone,
                new Point3D(m_xAxisCenter, m_yAxisCenter, m_zAxisCenter + m_zAxisLength), 0, 0);
            meshs.Add(zAxisCone);
        }

        private void AddAxesLabelsMeshes(ArrayList meshs)
        {
            var radius = (m_xAxisLength + m_yAxisLength + m_zAxisLength)/(3*m_axisLengthWidthRatio);

            Mesh3D xAxisCylinder = new Cylinder3D(radius, radius, m_xAxisLength/10, 6);
            xAxisCylinder.SetColor(m_axisXColor);
            TransformMatrix.Transform(xAxisCylinder,
                new Point3D(m_xAxisCenter/15 + (m_xAxisLength/10)/2, m_yAxisCenter/15, m_zAxisCenter/15), 0, 90);
            meshs.Add(xAxisCylinder);

            Mesh3D xAxisCone = new Cone3D(2*radius, 2*radius, radius*5, 6);
            xAxisCone.SetColor(m_axisXColor);
            TransformMatrix.Transform(xAxisCone,
                new Point3D(m_xAxisCenter/15 + m_xAxisLength/10, m_yAxisCenter/15, m_zAxisCenter/15), 0, 90);
            meshs.Add(xAxisCone);

            Mesh3D yAxisCylinder = new Cylinder3D(radius, radius, m_yAxisLength/10, 6);
            yAxisCylinder.SetColor(m_axisYColor);
            TransformMatrix.Transform(yAxisCylinder,
                new Point3D(m_xAxisCenter/15, m_yAxisCenter/15 + (m_yAxisLength/10)/2, m_zAxisCenter/15), 90, 90);
            meshs.Add(yAxisCylinder);

            Mesh3D yAxisCone = new Cone3D(2*radius, 2*radius, radius*5, 6);
            yAxisCone.SetColor(m_axisYColor);
            TransformMatrix.Transform(yAxisCone,
                new Point3D(m_xAxisCenter/15, m_yAxisCenter/15 + m_yAxisLength/10, m_zAxisCenter/15), 90, 90);
            meshs.Add(yAxisCone);

            Mesh3D zAxisCylinder = new Cylinder3D(radius, radius, m_zAxisLength/10, 6);
            zAxisCylinder.SetColor(m_axisZColor);
            TransformMatrix.Transform(zAxisCylinder,
                new Point3D(m_xAxisCenter/15, m_yAxisCenter/15, m_zAxisCenter/15 + (m_zAxisLength/10)/2), 0, 0);
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