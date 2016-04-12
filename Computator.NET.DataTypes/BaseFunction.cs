using System;
using Computator.NET.Logging;

namespace Computator.NET.DataTypes
{
    public abstract class BaseFunction
    {
        protected readonly Delegate _function;
        protected SimpleLogger logger;

        protected BaseFunction(Delegate function)
        {
            _function = function;
        }

        public string CsCode { get; set; }
        public string TslCode { get; set; }

        public FunctionType FunctionType { get; protected set; }

        public bool IsImplicit
            => FunctionType == FunctionType.ComplexImplicit || FunctionType == FunctionType.Real2DImplicit ||
               FunctionType == FunctionType.Real3DImplicit;
    }
}