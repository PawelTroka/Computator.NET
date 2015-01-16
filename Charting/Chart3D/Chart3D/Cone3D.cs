// class for a 3d cone model.
// version 0.1

using System;

namespace Computator.NET.Charting.Chart3D
{
    public class Cone3D : Mesh3D
    {
        // first 3 parameter are cone size, last parameter is cone resolution (smoothness)
        private int m_nRes; // resolution of the cone

        public Cone3D(double a, double b, double h, int nRes)
        {
            SetMesh(nRes);
            SetData(a, b, h);
        }

        // set mesh stracture, (triangle connection)
        private void SetMesh(int nRes)
        {
            int nVertNo = nRes + 2;
            int nTriNo = 2*nRes;
            SetSize(nVertNo, nTriNo);
            for (int i = 0; i < nRes - 1; i++)
            {
                SetTriangle(i, i, i + 1, nRes + 1);
                SetTriangle(nRes + i, i + 1, i, nRes);
            }
            SetTriangle(nRes - 1, nRes - 1, 0, nRes + 1);
            SetTriangle(2*nRes - 1, 0, nRes - 1, nRes);

            m_nRes = nRes;
        }

        // set cone vertices  
        // a: cone bottom ellipse long axis
        // b: cone bottom ellipse short axis
        // h: cone height
        private void SetData(double a, double b, double h)
        {
            double aXYStep = 2.0f*3.1415926f/((double) m_nRes);
            for (int i = 0; i < m_nRes; i++)
            {
                double aXY = i*aXYStep;
                SetPoint(i, a*Math.Cos(aXY), b*Math.Sin(aXY), 0);
            }
            SetPoint(m_nRes, 0, 0, 0);
            SetPoint(m_nRes + 1, 0, 0, h);

            m_xMin = -a;
            m_xMax = a;
            m_yMin = -b;
            m_yMax = b;
            m_zMin = 0;
            m_zMax = h;
        }
    }
}