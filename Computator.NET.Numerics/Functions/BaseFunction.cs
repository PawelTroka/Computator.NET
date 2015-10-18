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

}