#if !__MonoCS__
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    // The numeric coordinate follows the existing mesh axis. The other two
    // coordinates remain on its expanded data-bound corner, not at invented zeroes.
    internal sealed class Chart3DAxis
    {
        private readonly double _firstRadius;
        private readonly double _secondRadius;
        private readonly Cone3D _arrow;

        public Chart3DAxis(int index, double minimum, double maximum, Point3D start,
            Point3D end, Vector3D radii)
        {
            Index = index;
            Minimum = minimum;
            Maximum = maximum;
            Start = start;
            End = end;
            // Cylinder3D/Cone3D are built on their local z-axis. A radial
            // coordinate must use the corresponding world coordinate's scale,
            // including after the principal-axis permutation below.
            _firstRadius = index == 2 ? radii.X : radii.Z;
            _secondRadius = index == 0 ? radii.Y : index == 1 ? radii.X : radii.Y;
            var axialRadius = index == 0 ? radii.X : index == 1 ? radii.Y : radii.Z;
            _arrow = new Cone3D(2 * _firstRadius, 2 * _secondRadius, axialRadius * 5, 6);
            Orient(_arrow, End);
            LabelAnchor = _arrow.GetLastPoint();
            var vertices = new List<Point3D>();
            for (var vertex = 0; vertex < 6; vertex++) vertices.Add(_arrow.GetPoint(vertex));
            vertices.Add(LabelAnchor);
            ArrowVertices = vertices;
        }

        public int Index { get; }
        public double Minimum { get; }
        public double Maximum { get; }
        public Point3D Start { get; }
        public Point3D End { get; }
        public Point3D LabelAnchor { get; }
        public IReadOnlyList<Point3D> ArrowVertices { get; }

        internal Cylinder3D CreateCylinder(Color color)
        {
            var cylinder = new Cylinder3D(_firstRadius, _secondRadius, Maximum - Minimum, 6);
            Orient(cylinder, At(Minimum + (Maximum - Minimum) / 2));
            cylinder.SetColor(color);
            return cylinder;
        }

        internal Cone3D CreateArrow(Color color)
        {
            _arrow.SetColor(color);
            return _arrow;
        }

        private void Orient(Mesh3D mesh, Point3D center)
        {
            for (var vertex = 0; vertex < mesh.GetVertexNo(); vertex++)
            {
                var point = mesh.GetPoint(vertex);
                // Exact 90-degree permutations avoid leaking cos(pi/2) times a
                // very large axial coordinate into a very small radial one.
                var world = Index == 0 ? new Point3D(center.X + point.Z, center.Y + point.Y, center.Z - point.X) :
                    Index == 1 ? new Point3D(center.X - point.Y, center.Y + point.Z, center.Z - point.X) :
                    new Point3D(center.X + point.X, center.Y + point.Y, center.Z + point.Z);
                mesh.SetPoint(vertex, world);
            }
        }

        public Point3D At(double value)
        {
            return Index == 0 ? new Point3D(value, Start.Y, Start.Z) :
                Index == 1 ? new Point3D(Start.X, value, Start.Z) :
                new Point3D(Start.X, Start.Y, value);
        }
    }
}
#endif
