using System;
using Computator.NET.Logging;

namespace Computator.NET.DataTypes
{
    public abstract class BaseFunction
    {
        protected readonly Delegate _function;
        protected string csCode;
        protected SimpleLogger logger;
        protected string tslCode;

        protected BaseFunction(Delegate function, string tslCode, string csCode)
        {
            _function = function;
            this.tslCode = tslCode;
            this.csCode = csCode;
        }

        public FunctionType FunctionType { get; protected set; }

        public bool IsImplicit => FunctionType == FunctionType.ComplexImplicit || FunctionType == FunctionType.Real2DImplicit ||
                                  FunctionType == FunctionType.Real3DImplicit;
    }
}