using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Computator.NET.Charting.Chart3D.Spline
{
    public abstract class BasicSpline
    {
        public static object[] EMPTYOBJLIST = {};

        public void calcNaturalCubic(List<Point3D> valueCollection, Func<Point3D, double> getVal,
            List<Cubic> cubicCollection)
        {
            var num = valueCollection.Count - 1;

            var gamma = new double[num + 1];
            var delta = new double[num + 1];
            var D = new double[num + 1];

            int i;
            /*
                 We solve the equation
                [2 1       ] [D[0]]   [3(x[1] - x[0])  ]
                |1 4 1     | |D[1]|   |3(x[2] - x[0])  |
                |  1 4 1   | | .  | = |      .         |
                |    ..... | | .  |   |      .         |
                |     1 4 1| | .  |   |3(x[n] - x[n-2])|
                [       1 2] [D[n]]   [3(x[n] - x[n-1])]
          
                by using row operations to convert the matrix to upper triangular
                and then back sustitution.  The D[i] are the derivatives at the knots.
            */
            gamma[0] = 1.0f/2.0f;
            for (i = 1; i < num; i++)
            {
                gamma[i] = 1.0f/(4.0f - gamma[i - 1]);
            }
            gamma[num] = 1.0f/(2.0f - gamma[num - 1]);

            var p0 = getVal(valueCollection[0]);
            var p1 = getVal(valueCollection[1]);

            delta[0] = 3.0f*(p1 - p0)*gamma[0];
            for (i = 1; i < num; i++)
            {
                p0 = getVal(valueCollection[i - 1]);
                p1 = getVal(valueCollection[i + 1]);
                delta[i] = (3.0f*(p1 - p0) - delta[i - 1])*gamma[i];
            }
            p0 = getVal(valueCollection[num - 1]);
            p1 = getVal(valueCollection[num]);

            delta[num] = (3.0f*(p1 - p0) - delta[num - 1])*gamma[num];

            D[num] = delta[num];
            for (i = num - 1; i >= 0; i--)
            {
                D[i] = delta[i] - gamma[i]*D[i + 1];
            }

            /*
                 now compute the coefficients of the cubics 
            */
            cubicCollection.Clear();

            for (i = 0; i < num; i++)
            {
                p0 = getVal(valueCollection[i]);
                p1 = getVal(valueCollection[i + 1]);

                cubicCollection.Add(new Cubic(
                    p0,
                    D[i],
                    3*(p1 - p0) - 2*D[i] - D[i + 1],
                    2*(p0 - p1) + D[i] + D[i + 1]
                    )
                    );
            }
        }
    }
}