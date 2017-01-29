#if !__MonoCS__
// class translate mesh3d object to ModelVisual3D object.
// version 0.1

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    public class Model3D : ModelVisual3D
    {
        private readonly TextureMapping m_mapping = new TextureMapping();

        public void SetRGBColor()
        {
            m_mapping.SetRGBMaping();
        }

        public void SetPsedoColor()
        {
            m_mapping.SetPseudoMaping();
        }

        // set this ModelVisual3D object from a array of mesh3D objects
        private void SetModel(ArrayList meshs, Material backMaterial)
        {
            if (meshs == null)
                return;
            var nMeshNo = meshs.Count;
            if (nMeshNo == 0) return;

            var triangleMesh = new MeshGeometry3D();
            var nTotalVertNo = 0;
            for (var j = 0; j < nMeshNo; j++)
            {
                var mesh = (Mesh3D) meshs[j];
                var nVertNo = mesh.GetVertexNo();
                var nTriNo = mesh.GetTriangleNo();
                if ((nVertNo <= 0) || (nTriNo <= 0)) continue;

                var vx = new double[nVertNo];
                var vy = new double[nVertNo];
                var vz = new double[nVertNo];
                for (var i = 0; i < nVertNo; i++)
                {
                    vx[i] = vy[i] = vz[i] = 0;
                }

                // get normal of each vertex
                for (var i = 0; i < nTriNo; i++)
                {
                    var tri = mesh.GetTriangle(i);
                    var vN = mesh.GetTriangleNormal(i);
                    var n0 = tri.n0;
                    var n1 = tri.n1;
                    var n2 = tri.n2;

                    vx[n0] += vN.X;
                    vy[n0] += vN.Y;
                    vz[n0] += vN.Z;
                    vx[n1] += vN.X;
                    vy[n1] += vN.Y;
                    vz[n1] += vN.Z;
                    vx[n2] += vN.X;
                    vy[n2] += vN.Y;
                    vz[n2] += vN.Z;
                }
                for (var i = 0; i < nVertNo; i++)
                {
                    var length = Math.Sqrt(vx[i]*vx[i] + vy[i]*vy[i] + vz[i]*vz[i]);
                    if (length > 1e-20)
                    {
                        vx[i] /= length;
                        vy[i] /= length;
                        vz[i] /= length;
                    }
                    triangleMesh.Positions.Add(mesh.GetPoint(i));
                    var color = mesh.GetColor(i);
                    var mapPt = m_mapping.GetMappingPosition(color);

                    if (color == Colors.Transparent)
                        mapPt = TextureMapping.GetMappingPosition(color, false);

                    //Point mapPt = TextureMapping.GetMappingPosition(color,true);
                    triangleMesh.TextureCoordinates.Add(new Point(mapPt.X, mapPt.Y));
                    triangleMesh.Normals.Add(new Vector3D(vx[i], vy[i], vz[i]));
                }

                for (var i = 0; i < nTriNo; i++)
                {
                    var tri = mesh.GetTriangle(i);
                    var n0 = tri.n0;
                    var n1 = tri.n1;
                    var n2 = tri.n2;

                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n0);
                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n1);
                    triangleMesh.TriangleIndices.Add(nTotalVertNo + n2);
                }
                nTotalVertNo += nVertNo;
            }
            //Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            Material material = m_mapping.m_material;

            var triangleModel = new GeometryModel3D(triangleMesh, material);
            triangleModel.Transform = new Transform3DGroup();
            if (backMaterial != null) triangleModel.BackMaterial = backMaterial;

            Content = triangleModel;
        }

        // get MeshGeometry3D object from Viewport3D
        public static MeshGeometry3D GetGeometry(Viewport3D viewport3d, int nModelIndex)
        {
            if (nModelIndex == -1) return null;
            var visual3d = (ModelVisual3D) viewport3d.Children[nModelIndex];
            if (visual3d.Content == null) return null;
            var triangleModel = (GeometryModel3D) visual3d.Content;
            return (MeshGeometry3D) triangleModel.Geometry;
        }

        // update the ModelVisual3D object in "viewport3d" using Mesh3D array "meshs"

        public int UpdateModel(ArrayList meshs, Material backMaterial, int nModelIndex, Viewport3D viewport3d)
        {
            if (nModelIndex >= 0)
            {
                var m = (ModelVisual3D) viewport3d.Children[nModelIndex];
                viewport3d.Children.Remove(m);
            }

            if (backMaterial == null)
                SetRGBColor();
            else
                SetPsedoColor();

            SetModel(meshs, backMaterial);

            var nModelNo = viewport3d.Children.Count;
            viewport3d.Children.Add(this);
            //viewport3d.Children.Add(labelsModelVisual3D);

            return nModelNo;
        }
    }
}
#endif