using System;
using NLog;

namespace Computator.NET.DataTypes
{
    public abstract class BaseFunction
    {
        protected readonly Delegate _function;
        protected readonly ILogger Logger;

        protected BaseFunction(Delegate function)
        {
            _function = function;
            Logger = LogManager.GetLogger(GetType().FullName);
        }

        public string CsCode { get; set; }
        public string TslCode { get; set; }

        public FunctionType FunctionType { get; protected set; }

        public bool IsImplicit
            => FunctionType == FunctionType.ComplexImplicit || FunctionType == FunctionType.Real2DImplicit ||
               FunctionType == FunctionType.Real3DImplicit;
    }
}