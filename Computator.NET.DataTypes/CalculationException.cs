using System;

namespace Computator.NET.DataTypes
{
    public class CalculationException : Exception
    {
        public CalculationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}