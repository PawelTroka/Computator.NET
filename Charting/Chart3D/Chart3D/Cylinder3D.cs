// class for a 3d cylinder model.
// version 0.1

using System;

namespace Computator.NET.Charting.Chart3D
{
    public class Cylinder3D : Mesh3D
    {
        //  first 3 parameter is the cylinder size, last parameter is the cylinder smoothness
        private int m_nRes;

        public Cylinder3D(double a, double b, double h, int nRes)
        {
            SetMesh(nRes);
            SetData(a, b, h);
        }

        // set mesh structure, (triangle connection)
        private void SetMesh(int nRes)
        {
            int nVertNo = 2*nRes + 2;
            int nTriNo = 4*nRes;
            SetSize(nVertNo, nTriNo);
            int n1, n2;
            for (int i = 0; i < nRes; i++)
            {
                n1 = i;
                if (i == (nRes - 1)) n2 = 0;
                else n2 = i + 1;
                SetTriangle(i*4 + 0, n1, n2, nRes + n1); // side
                SetTriangle(i*4 + 1, nRes + n1, n2, nRes + n2); // side
                SetTriangle(i*4 + 2, n2, n1, 2*nRes); // bottom
                SetTriangle(i*4 + 3, nRes + n1, nRes + n2, 2*nRes + 1); // top
            }

            m_nRes = nRes;
        }

        // set mesh vertex location
        private void SetData(double a, double b, double h)
        {
            double aXYStep = 2.0f*3.1415926f/((double) m_nRes);
            for (int i = 0; i < m_nRes; i++)
            {
                double aXY = i*aXYStep;
                SetPoint(i, a*Math.Cos(aXY), b*Math.Sin(aXY), -h/2);
            }

            for (int i = 0; i < m_nRes; i++)
            {
                double aXY = i*aXYStep;
                SetPoint(m_nRes + i, a*Math.Cos(aXY), b*Math.Sin(aXY), h/2);
            }

            SetPoint(2*m_nRes, 0, 0, -h/2);
            SetPoint(2*m_nRes + 1, 0, 0, h/2);

            m_xMin = -a;
            m_xMax = a;
            m_yMin = -b;
            m_yMax = b;
            m_zMin = - h/2;
            m_zMax = h/2;
        }
    }
}