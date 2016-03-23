using System;
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
            for (var i = 0; i < points.Count; i++)
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
            var meshs = ((ScatterChart3D) m_3dChart).GetMeshes();

            // if (_useSpline)
            //  drawSpline(points, color, meshs);

            // 5. display vertex no and triangle no.
            UpdateChartLabels(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix
            rescaleProjectionMatrix();
            TransformChart();
        }

        public void AddPoints(List<Point3D> points, Color color)
        {
            var oldSize = m_3dChart.GetDataNo();
            m_3dChart.IncreaseDataSize(points.Count);

            // 2. set the properties of each dot
            for (var i = 0; i < points.Count; i++)
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
            var meshs = (((ScatterChart3D) m_3dChart).GetMeshes());

            // if (_useSpline)
            //  drawSpline(points, color, meshs);

            // 5. display vertex no and triangle no.
            UpdateChartLabels(meshs);

            // 6. show 3D scatter plot in Viewport3d
            var model3d = new Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, mainViewport);

            // 7. set projection matrix
            rescaleProjectionMatrix();
            TransformChart();
        }
        
    }
}