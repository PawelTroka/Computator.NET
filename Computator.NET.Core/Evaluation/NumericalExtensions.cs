namespace Computator.NET.Core.Evaluation
{
    public static class NumericalExtensions
    {
        public static bool IsNumericType(this object o)
        {
            if (o == null)
                return false;
            switch (System.Type.GetTypeCode(o.GetType()))
            {
                case System.TypeCode.Byte:
                case System.TypeCode.SByte:
                case System.TypeCode.UInt16:
                case System.TypeCode.UInt32:
                case System.TypeCode.UInt64:
                case System.TypeCode.Int16:
                case System.TypeCode.Int32:
                case System.TypeCode.Int64:
                case System.TypeCode.Decimal:
                case System.TypeCode.Double:
                case System.TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public const string ToCode = @"
public static class NumericalExtensions
    {
        public static bool IsNumericType(this object o)
        {
            if (o == null)
                return false;
            switch (System.Type.GetTypeCode(o.GetType()))
            {
                case System.TypeCode.Byte:
                case System.TypeCode.SByte:
                case System.TypeCode.UInt16:
                case System.TypeCode.UInt32:
                case System.TypeCode.UInt64:
                case System.TypeCode.Int16:
                case System.TypeCode.Int32:
                case System.TypeCode.Int64:
                case System.TypeCode.Decimal:
                case System.TypeCode.Double:
                case System.TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
";

    }
}