using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Splines
{
    public class Spline3D : BasicSpline
    {
        private static object[] EMPTYOBJ = {};
        private readonly Func<Point3D, double> point3DgetXMethod;
        private readonly Func<Point3D, double> point3DgetYMethod;
        private readonly Func<Point3D, double> point3DgetZMethod;
        private readonly List<Point3D> points;
        private readonly List<Cubic> xCubics;
        private readonly List<Cubic> yCubics;
        private readonly List<Cubic> zCubics;

        public Spline3D()
        {
            points = new List<Point3D>();

            xCubics = new List<Cubic>();
            yCubics = new List<Cubic>();
            zCubics = new List<Cubic>();
            point3DgetXMethod = delegate(Point3D point3d) { return point3d.X; };
            point3DgetYMethod = delegate(Point3D point3d) { return point3d.Y; };
            point3DgetZMethod = delegate(Point3D point3d) { return point3d.Z; };
        }

        public void addPoint(Point3D point)
        {
            points.Add(point);
        }

        public List<Point3D> getPoints()
        {
            return points;
        }

        public void calcSpline()
        {
            calcNaturalCubic(points, point3DgetXMethod, xCubics);
            calcNaturalCubic(points, point3DgetYMethod, yCubics);
            calcNaturalCubic(points, point3DgetZMethod, zCubics);
        }

        public Point3D getPoint(double position)
        {
            position = position*(xCubics.Count - 1);
            var cubicNum = (int) position;
            var cubicPos = position - cubicNum;

            return new Point3D(xCubics[cubicNum].eval(cubicPos),
                yCubics[cubicNum].eval(cubicPos),
                zCubics[cubicNum].eval(cubicPos));
        }
    }
}