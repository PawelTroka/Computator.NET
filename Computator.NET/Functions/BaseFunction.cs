using System;

namespace Computator.NET.Functions
{
    internal class Function<T>
    {
        private readonly Func<T, T> f;

        public Function(Func<T, T> function, string name)
        {
            f = function;
            Name = name;
        }

        public string Name { get; set; }

        public Type getType()
        {
            return typeof (T);
        }

        public T eval(T x)
        {
            return f(x);
        }
    }

    internal static class ObjectExtension
    {
        public static bool IsNumericType(this object o)
        {
            if (o == null)
                return false;
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}