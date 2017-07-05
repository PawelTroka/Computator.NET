namespace Computator.NET.Core.Validation
{
    public class ChartAreaValuesValidator
    {
        public static bool IsValid(double xmin, double xmax, double ymin, double ymax)
        {
            return xmin < xmax && ymax > ymin;
        }
    }
}
