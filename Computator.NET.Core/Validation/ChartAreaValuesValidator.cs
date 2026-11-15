namespace Computator.NET.Core.Validation
{
    public class ChartAreaValuesValidator
    {
        public static bool IsValid(double xmin, double xmax, double ymin, double ymax)
        {
            return IsFinite(xmin) && IsFinite(xmax) && IsFinite(ymin) && IsFinite(ymax)
                   && xmin < xmax && ymin < ymax;
        }
        private static bool IsFinite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
