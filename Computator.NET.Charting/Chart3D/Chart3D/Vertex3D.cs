#if !__MonoCS__
// A dot in 3D space
// version 0.1

using System.Windows.Media;

namespace Computator.NET.Charting.Chart3D.Chart3D
{
    internal class Vertex3D
    {
        public Color color; // color of the dot
        public int nMaxI; // link to the viewport positions array index
        public int nMinI; // link to the viewport positions array index
        public bool selected = false; // is this dot selected by user
        public float x, y, z; // location of the dot
    }
}
#endif