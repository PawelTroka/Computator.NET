#if !__MonoCS__
// class for a 3d rotation, drag and zoom.
// version 0.1

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    public class TransformMatrix
    {
        private bool m_mouseLdown; // mouse down
        private bool m_mouseRdown; // mouse down
        private Point m_movePoint; // previous mouse location
        private Matrix3D m_projMatrix = new Matrix3D();
        public double m_scaleFactor = 1.3; // sensativity for zoom
        public Matrix3D m_totalMatrix;
        private Matrix3D m_viewMatrix = new Matrix3D();

        public void ResetView()
        {
            m_viewMatrix.SetIdentity();
        }

        public void OnMBtnDown()
        {
            m_viewMatrix.SetIdentity();
            m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
        }

        public void OnLBtnDown(Point pt)
        {
            m_mouseLdown = true;
            m_movePoint = pt;
        }

        public void OnRBtnDown(Point pt)
        {
            m_mouseRdown = true;
            m_movePoint = pt;
        }

        public void OnMouseMove(Point pt, Viewport3D viewPort)
        {
            if (!m_mouseLdown && !m_mouseRdown) return;

            var width = viewPort.ActualWidth;
            var height = viewPort.ActualHeight;

            //OrthographicCamera camera = viewPort.Camera as System.Windows.Media.Media3D.OrthographicCamera;
            //Matrix3D cameraMatrix = camera.Transform.Value;

            //if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            //{
            //}
            if (m_mouseRdown) //Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var shiftX = 2*(pt.X - m_movePoint.X)/width;
                var shiftY = -2*(pt.Y - m_movePoint.Y)/width;
                m_viewMatrix.Translate(new Vector3D(shiftX, shiftY, 0));
                m_movePoint = pt;
            }
            else if (m_mouseLdown)
            {
                var aY = 180*(pt.X - m_movePoint.X)/width;
                var aX = 180*(pt.Y - m_movePoint.Y)/height;

                m_viewMatrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), aX));
                m_viewMatrix.Rotate(new Quaternion(new Vector3D(0, 1, 0), aY));
                m_movePoint = pt;
            }
            m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
        }

        public void OnLBtnUp()
        {
            m_mouseLdown = false;
        }

        public void OnRBtnUp()
        {
            m_mouseRdown = false;
        }

        public void OnKeyDown(KeyEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Home:
                    m_viewMatrix.SetIdentity();
                    break;
                case Key.OemPlus:
                    m_viewMatrix.Scale(new Vector3D(m_scaleFactor, m_scaleFactor, m_scaleFactor));
                    break;
                case Key.OemMinus:
                    m_viewMatrix.Scale(new Vector3D(1/m_scaleFactor, 1/m_scaleFactor, 1/m_scaleFactor));
                    break;
                default:
                    return;
            }
            m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
        }

        public void OnMouseWheel(MouseWheelEventArgs e)
        {
            var delta = e.Delta;

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                m_viewMatrix.SetIdentity();
                m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
                return;
            }

            if (delta > 0)
                // for (int i = 0; i < delta; i++)
            {
                m_viewMatrix.Scale(new Vector3D(m_scaleFactor, m_scaleFactor, m_scaleFactor));
                m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
            }
            else if (delta < 0)
                //for (int i = 0; i < -delta; i++)
            {
                m_viewMatrix.Scale(new Vector3D(1/m_scaleFactor, 1/m_scaleFactor, 1/m_scaleFactor));
                m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
            }
        }

        // transform input point pt1, (rotate "aX, aZ" and move to "center")
        public static Point3D Transform(Point3D pt1, Point3D center, double aX, double aZ)
        {
            var angleX = Math.PI*aX/180;
            var angleZ = Math.PI*aZ/180;

            // rotate from z-axis
            var x2 = pt1.X*Math.Cos(angleZ) + pt1.Z*Math.Sin(angleZ);
            var y2 = pt1.Y;
            var z2 = -pt1.X*Math.Sin(angleZ) + pt1.Z*Math.Cos(angleZ);

            var x3 = center.X + x2*Math.Cos(angleX) - y2*Math.Sin(angleX);
            var y3 = center.Y + x2*Math.Sin(angleX) + y2*Math.Cos(angleX);
            var z3 = center.Z + z2;

            return new Point3D(x3, y3, z3);
        }

        // transform input point pt1, (rotate "aX, aZ" and move to "center")
        public static void Transform(Mesh3D model, Point3D center, double aX, double aZ)
        {
            var angleX = Math.PI*aX/180;
            var angleZ = Math.PI*aZ/180;

            var nVertNo = model.GetVertexNo();
            for (var i = 0; i < nVertNo; i++)
            {
                var pt1 = model.GetPoint(i);
                // rotate from z-axis
                var x2 = pt1.X*Math.Cos(angleZ) + pt1.Z*Math.Sin(angleZ);
                var y2 = pt1.Y;
                var z2 = -pt1.X*Math.Sin(angleZ) + pt1.Z*Math.Cos(angleZ);

                var x3 = center.X + x2*Math.Cos(angleX) - y2*Math.Sin(angleX);
                var y3 = center.Y + x2*Math.Sin(angleX) + y2*Math.Cos(angleX);
                var z3 = center.Z + z2;

                model.SetPoint(i, x3, y3, z3);
            }
        }

        // set the projection matrix
        public void CalculateProjectionMatrix(Mesh3D mesh, double scaleFactor)
        {
            CalculateProjectionMatrix(mesh.m_xMin, mesh.m_xMax, mesh.m_yMin, mesh.m_yMax, mesh.m_zMin, mesh.m_zMax,
                scaleFactor);
        }

        public void CalculateProjectionMatrix(double min, double max, double scaleFactor)
        {
            CalculateProjectionMatrix(min, max, min, max, min, max, scaleFactor);
        }

        public void CalculateProjectionMatrix(double xMin, double xMax, double yMin, double yMax, double zMin,
            double zMax, double scaleFactor)
        {
            var xC = (xMin + xMax)/2;
            var yC = (yMin + yMax)/2;
            var zC = (zMin + zMax)/2;

            var xRange = (xMax - xMin)/2;
            var yRange = (yMax - yMin)/2;
            var zRange = (zMax - zMin)/2;

            m_projMatrix.SetIdentity();
            m_projMatrix.Translate(new Vector3D(-xC, -yC, -zC));

            if (xRange < 1e-10) return;

            var sX = scaleFactor/xRange;
            var sY = scaleFactor/yRange;
            var sZ = scaleFactor/zRange;
            m_projMatrix.Scale(new Vector3D(sX, sY, sZ));

            m_totalMatrix = Matrix3D.Multiply(m_projMatrix, m_viewMatrix);
        }

        // get the screen position from original vertex
        public Point VertexToScreenPt(Point3D point, Viewport3D viewPort)
        {
            var pt2 = m_totalMatrix.Transform(point);

            var width = viewPort.ActualWidth;
            var height = viewPort.ActualHeight;

            var x3 = width/2 + pt2.X*width/2;
            var y3 = height/2 - pt2.Y*width/2;

            return new Point(x3, y3);
        }

        public static Point ScreenPtToViewportPt(Point point, Viewport3D viewPort)
        {
            var width = viewPort.ActualWidth;
            var height = viewPort.ActualHeight;

            var x3 = point.X;
            var y3 = point.Y;
            var x2 = (x3 - width/2)*2/width;
            var y2 = (height/2 - y3)*2/width;

            return new Point(x2, y2);
        }

        public Point VertexToViewportPt(Point3D point, Viewport3D viewPort)
        {
            var pt2 = m_totalMatrix.Transform(point);
            return new Point(pt2.X, pt2.Y);
        }
    }
}
#endif