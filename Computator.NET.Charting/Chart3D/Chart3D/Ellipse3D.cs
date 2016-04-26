// class for a 3d ellipse.
// version 0.1

using System;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    public class Ellipse3D : Mesh3D
    {
        // the first 3 parameters are the ellipse size, last parameter is the smoothness of the ellipse
        private int m_nRes;

        public Ellipse3D(double a, double b, double h, int nRes)
        {
            SetMesh(nRes);
            SetData(a, b, h);
        }

        // set the mesh structure (triangle connection)
        private void SetMesh(int nRes)
        {
            // one vertex at top and bottom, other ring has nRes vertex
            var nVertNo = (nRes - 2)*nRes + 2;
            // the top and bottom band has nRes triangle
            // middle band has 2*nRes*(nRes - 2 - 1)
            var nTriNo = 2*nRes*(nRes - 3) + 2*nRes;
            SetSize(nVertNo, nTriNo);

            // set triangle
            int n00, n01, n10, n11;
            var nTriIndex = 0;
            int nI2;
            // first set top band
            int i;
            var j = 1;
            for (i = 0; i < nRes; i++)
            {
                if (i == nRes - 1) nI2 = 0;
                else nI2 = i + 1;

                n00 = 1 + (j - 1)*nRes + i;
                n10 = 1 + (j - 1)*nRes + nI2;
                n01 = 0;

                SetTriangle(nTriIndex, n00, n10, n01);
                nTriIndex++;
            }
            // set middle bands
            for (j = 1; j < nRes - 2; j++)
            {
                for (i = 0; i < nRes; i++)
                {
                    if (i == nRes - 1) nI2 = 0;
                    else nI2 = i + 1;
                    n00 = 1 + (j - 1)*nRes + i;
                    n10 = 1 + (j - 1)*nRes + nI2;
                    n01 = 1 + j*nRes + i;
                    n11 = 1 + j*nRes + nI2;

                    SetTriangle(nTriIndex, n00, n01, n10);
                    SetTriangle(nTriIndex + 1, n01, n11, n10);
                    nTriIndex += 2;
                }
            }

            j = nRes - 2;
            for (i = 0; i < nRes; i++)
            {
                if (i == nRes - 1) nI2 = 0;
                else nI2 = i + 1;

                n00 = 1 + (j - 1)*nRes + i;
                n10 = 1 + (j - 1)*nRes + nI2;
                n01 = nVertNo - 1;

                SetTriangle(nTriIndex, n00, n01, n10);
                nTriIndex++;
            }
            m_nRes = nRes;
        }

        // the vertex location according the ellipse size
        private void SetData(double a, double b, double h)
        {
            double aXYStep = 2.0f*3.1415926f/m_nRes;
            double aZStep = 3.1415926f/((float) m_nRes - 1);

            SetPoint(0, 0, 0, h); // first vertex is at top

            int i, j;
            double x1, y1, z1;
            for (j = 1; j < m_nRes - 1; j++)
            {
                for (i = 0; i < m_nRes; i++)
                {
                    var aXY = i*aXYStep;
                    var aZAngle = j*aZStep;

                    x1 = a*Math.Sin(aZAngle)*Math.Cos(aXY);
                    y1 = b*Math.Sin(aZAngle)*Math.Sin(aXY);
                    z1 = h*Math.Cos(aZAngle);
                    SetPoint((j - 1)*m_nRes + i + 1, x1, y1, z1);
                }
            }
            SetPoint((m_nRes - 2)*m_nRes + 1, 0, 0, -h);

            m_xMin = -a;
            m_xMax = a;
            m_yMin = -b;
            m_yMax = b;
            m_zMin = -h;
            m_zMax = h;
        }
    }
}