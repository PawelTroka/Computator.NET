using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D
{
    public partial class Chart3DControl : UserControl
    {
        public void SetPoints(List<Point3D> points, Color color)
        {
            // 1. set the scatter plot size
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(points.Count);

            // 2. set the properties of each dot
            for (int i = 0; i < points.Count; i++)
            {
                var plotItem = new ScatterPlotItem();

                plotItem.w = (float) DotSize; //size of plotItem
                plotItem.h = (float) DotSize; //size of plotItem

                plotItem.x = (float) points[i].X;
                plotItem.y = (float) points[i].Y;
                plotItem.z = (float) points[i].Z;

                plotItem.shape = (int) Chart3D.SHAPE.ELLIPSE;

                plotItem.color = color;
                ((ScatterChart3D) m_3dChart).SetVertex(i, plotItem);
            }
            m_3dChart.UseAxes = VisibilityAxes;
            m_3dChart.SetAxesColor(AxesColor);
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            ArrayList meshs = ((ScatterChart3D) m_3dChart).GetMeshes();

            // if (_useSpline)
            //  drawSpline(points, color, meshs);

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix
            rescaleProjectionMatrix();
            TransformChart();
        }

        public void AddPoints(List<Point3D> points, Color color)
        {
            int oldSize = m_3dChart.GetDataNo();
            m_3dChart.IncreaseDataSize(points.Count);

            // 2. set the properties of each dot
            for (int i = 0; i < points.Count; i++)
            {
                var plotItem = new ScatterPlotItem();

                plotItem.w = (float) DotSize; //size of plotItem
                plotItem.h = (float) DotSize; //size of plotItem

                plotItem.x = (float) points[i].X;
                plotItem.y = (float) points[i].Y;
                plotItem.z = (float) points[i].Z;

                plotItem.shape = (int) Chart3D.SHAPE.ELLIPSE;

                plotItem.color = color;
                ((ScatterChart3D) m_3dChart).SetVertex(oldSize + i, plotItem);
            }
            m_3dChart.UseAxes = VisibilityAxes;
            m_3dChart.SetAxesColor(AxesColor);
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            ArrayList meshs = (((ScatterChart3D) m_3dChart).GetMeshes());

            // if (_useSpline)
            //  drawSpline(points, color, meshs);

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix
            rescaleProjectionMatrix();
            TransformChart();
        }

        // function for testing surface chart
        private void TestSurfacePlot(int nGridNo)
        {
            int nXNo = nGridNo;
            int nYNo = nGridNo;
            // 1. set the surface grid
            m_3dChart = new UniformSurfaceChart3D();
            ((UniformSurfaceChart3D) m_3dChart).SetGrid(nXNo, nYNo, -100, 100, -100, 100);

            // 2. set surface chart z value
            double xC = m_3dChart.XCenter();
            double yC = m_3dChart.YCenter();
            int nVertNo = m_3dChart.GetDataNo();
            double zV;
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];

                double r = 0.15*Math.Sqrt((vert.x - xC)*(vert.x - xC) + (vert.y - yC)*(vert.y - yC));
                if (r < 1e-10) zV = 1;
                else zV = Math.Sin(r)/r;

                m_3dChart[i].z = (float) zV;
            }
            m_3dChart.GetDataRange();

            // 3. set the surface chart color according to z vaule
            double zMin = m_3dChart.ZMin();
            double zMax = m_3dChart.ZMax();
            for (int i = 0; i < nVertNo; i++)
            {
                Vertex3D vert = m_3dChart[i];
                double h = (vert.z - zMin)/(zMax - zMin);

                Color color = TextureMapping.PseudoColor(h);
                m_3dChart[i].color = color;
            }

            // 4. Get the Mesh3D array from surface chart
            ArrayList meshs = ((UniformSurfaceChart3D) m_3dChart).GetMeshes();

            // 5. display vertex no and triangle no of this surface chart
            UpdateModelSizeInfo(meshs);

            // 6. Set the model display of surface chart
            var model3d = new Model3D();
            Material backMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            m_nChartModelIndex = model3d.UpdateModel(meshs, backMaterial, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix, so the data is in the display region
            float xMin = m_3dChart.XMin();
            float xMax = m_3dChart.XMax();
            m_transformMatrix.CalculateProjectionMatrix(xMin, xMax, xMin, xMax, zMin, zMax, 0.5);
            TransformChart();
        }

        // function for testing 3d scatter plot
        private void TestScatterPlot(int nDotNo)
        {
            // 1. set scatter chart data no.
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(nDotNo);

            // 2. set property of each dot (size, position, shape, color)
            var randomObject = new Random();
            int nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                var plotItem = new ScatterPlotItem();

                plotItem.w = 4;
                plotItem.h = 6;

                plotItem.x = randomObject.Next(nDataRange);
                plotItem.y = randomObject.Next(nDataRange);
                plotItem.z = randomObject.Next(nDataRange);

                plotItem.shape = randomObject.Next(4);

                var nR = (Byte) randomObject.Next(256);
                var nG = (Byte) randomObject.Next(256);
                var nB = (Byte) randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D) m_3dChart).SetVertex(i, plotItem);
            }

            // 3. set the axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. get Mesh3D array from the scatter plot
            ArrayList meshs = ((ScatterChart3D) m_3dChart).GetMeshes();

            // 5. display model vertex no and triangle no
            UpdateModelSizeInfo(meshs);

            // 6. display scatter plot in Viewport3D
            var model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix
            float viewRange = nDataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        // function for set a scatter plot, every dot is just a simple pyramid.
        private void TestSimpleScatterPlot(int nDotNo)
        {
            // 1. set the scatter plot size
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(nDotNo);

            // 2. set the properties of each dot
            var randomObject = new Random();
            int nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                var plotItem = new ScatterPlotItem();

                plotItem.w = 2;
                plotItem.h = 2;

                plotItem.x = randomObject.Next(nDataRange);
                plotItem.y = randomObject.Next(nDataRange);
                plotItem.z = randomObject.Next(nDataRange);

                plotItem.shape = (int) Chart3D.SHAPE.PYRAMID;

                var nR = (Byte) randomObject.Next(256);
                var nG = (Byte) randomObject.Next(256);
                var nB = (Byte) randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D) m_3dChart).SetVertex(i, plotItem);
            }
            // 3. set axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. Get Mesh3D array from scatter plot
            ArrayList meshs = ((ScatterChart3D) m_3dChart).GetMeshes();

            // 5. display vertex no and triangle no.
            UpdateModelSizeInfo(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix
            float viewRange = nDataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }
    }
}