using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computator.NET.Core.Validation
{
    class ChartAreaValuesValidator
    {
        public static bool IsValid(double xmin, double xmax, double ymin, double ymax)
        {
            return xmin < xmax && ymax > ymin;
        }
    }
}
