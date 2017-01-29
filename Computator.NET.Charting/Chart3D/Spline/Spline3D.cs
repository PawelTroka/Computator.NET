using System;
using System.Collections.Generic;
using Computator.NET.DataTypes;

namespace Computator.NET.Charting.Chart3D.Spline
{
    public class Spline3D : BasicSpline
    {
        private readonly Func<Point3D, double> _point3DgetXMethod;
        private readonly Func<Point3D, double> _point3DgetYMethod;
        private readonly Func<Point3D, double> _point3DgetZMethod;
        private readonly List<Point3D> _points;
        private readonly List<Cubic> _xCubics;
        private readonly List<Cubic> _yCubics;
        private readonly List<Cubic> _zCubics;

        public Spline3D()
        {
            _points = new List<Point3D>();

            _xCubics = new List<Cubic>();
            _yCubics = new List<Cubic>();
            _zCubics = new List<Cubic>();
            _point3DgetXMethod = point3D => point3D.x;
            _point3DgetYMethod = point3D => point3D.y;
            _point3DgetZMethod = point3D => point3D.z;
        }

        public void AddPoint(Point3D point)
        {
            _points.Add(point);
        }

        public List<Point3D> GetPoints()
        {
            return _points;
        }

        public void CalcSpline()
        {
            CalcNaturalCubic(_points, _point3DgetXMethod, _xCubics);
            CalcNaturalCubic(_points, _point3DgetYMethod, _yCubics);
            CalcNaturalCubic(_points, _point3DgetZMethod, _zCubics);
        }

        public Point3D GetPoint(double position)
        {
            position = position*(_xCubics.Count - 1);
            var cubicNum = (int) position;
            var cubicPos = position - cubicNum;

            return new Point3D(_xCubics[cubicNum].eval(cubicPos),
                _yCubics[cubicNum].eval(cubicPos),
                _zCubics[cubicNum].eval(cubicPos));
        }
    }
}