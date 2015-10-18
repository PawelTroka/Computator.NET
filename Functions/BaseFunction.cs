using System;
using Computator.NET.Config;
using MathNet.Numerics;

namespace Computator.NET.Functions
{
    abstract class BaseFunction
    {
        protected BaseFunction(Delegate function,string tslCode, string csCode)
        {
            _function = function;
            this.tslCode = tslCode;
            this.csCode = csCode;
        }
        protected SimpleLogger logger;
        protected readonly Delegate _function;
        protected string tslCode;
        protected string csCode;
        public FunctionType FunctionType { get; protected set; }
        public bool IsImplicit
        {
            get
            {
                return FunctionType == FunctionType.ComplexImplicit || FunctionType == FunctionType.Real2DImplicit ||
                       FunctionType == FunctionType.Real3DImplicit;
            }
        }
    }

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
            return typeof(T);
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