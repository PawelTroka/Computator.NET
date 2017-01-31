#if !__MonoCS__
// class for a 3d bar model.
// version 0.1

//			0______________1
//		   /|	          /|
//        3 ____________2  |
//        | 4  ---------|- 5
//        |/            | /
//        7 ___________ 6 


namespace Computator.NET.Charting.Chart3D.Chart3D
{
    public class Bar3D : Mesh3D
    {
        public Bar3D(double x0, double y0, double z0, double W, double L, double H)
        {
            SetMesh();
            SetData(x0, y0, z0, W, L, H);
        }

        // set the mesh structure (triangle connection)
        public void SetMesh()
        {
            SetSize(8, 12);

            SetTriangle(0, 0, 2, 1);
            SetTriangle(1, 0, 3, 2);
            SetTriangle(2, 1, 2, 5);
            SetTriangle(3, 2, 6, 5);
            SetTriangle(4, 3, 6, 2);
            SetTriangle(5, 3, 7, 6);
            SetTriangle(6, 0, 4, 3);
            SetTriangle(7, 3, 4, 7);
            SetTriangle(8, 4, 6, 7);
            SetTriangle(9, 4, 5, 6);
            SetTriangle(10, 0, 5, 4);
            SetTriangle(11, 0, 1, 5);
        }

        // set the spatial location of 8 vertices
        // first 3 parameters are the bar center, the last 3 parameters are the bar size at each axis
        public void SetData(double x0, double y0, double z0, double W, double L, double H)
        {
            SetPoint(0, x0 - W/2, y0 + L/2, z0 + H/2);
            SetPoint(1, x0 + W/2, y0 + L/2, z0 + H/2);
            SetPoint(2, x0 + W/2, y0 - L/2, z0 + H/2);
            SetPoint(3, x0 - W/2, y0 - L/2, z0 + H/2);

            SetPoint(4, x0 - W/2, y0 + L/2, z0 - H/2);
            SetPoint(5, x0 + W/2, y0 + L/2, z0 - H/2);
            SetPoint(6, x0 + W/2, y0 - L/2, z0 - H/2);
            SetPoint(7, x0 - W/2, y0 - L/2, z0 - H/2);

            m_xMin = x0 - W/2;
            m_xMax = x0 + W/2;
            m_yMin = y0 - L/2;
            m_yMax = y0 + L/2;
            m_zMin = z0 - H/2;
            m_zMax = z0 + H/2;
        }
    }
}
#endif