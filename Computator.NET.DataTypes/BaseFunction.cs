using System;
using Computator.NET.Logging;

namespace Computator.NET.DataTypes
{
    public abstract class BaseFunction
    {
        protected readonly Delegate _function;
        public string CsCode { get; set; }
        protected SimpleLogger logger;
        public string TslCode { get; set; }

        protected BaseFunction(Delegate function)
        {
            _function = function;

        }

        public FunctionType FunctionType { get; protected set; }

        public bool IsImplicit => FunctionType == FunctionType.ComplexImplicit || FunctionType == FunctionType.Real2DImplicit ||
                                  FunctionType == FunctionType.Real3DImplicit;
    }
}