using System;
using System.Numerics;

namespace Computator.NET.Core.Helpers
{
    public static class TypeExtenstion
    {
        public static bool IsInteger(this Type type)
        {
            return type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(ulong) ||
                   type == typeof(long) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(char) ||
                   type == typeof(byte);
        }

        public static bool IsDouble(this Type type)
        {
            return type == typeof(float) ||
                   type == typeof(double);
        }

        public static bool IsComplex(this Type type)
        {
            return type == typeof(Complex);
        }
    }
}