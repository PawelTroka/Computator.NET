using System;
using System.Numerics;
using System.Reflection;
using Computator.NET.Config;

namespace Computator.NET.Functions
{
    class Function : BaseFunction
    {

        public Function(Delegate function, string tslCode, string csCode, FunctionType? functionType = null)
            : base(function,tslCode, csCode)
        {

            if (functionType.HasValue)
                FunctionType = functionType.Value;
            else
                guessType();

            logger = new SimpleLogger() { ClassName = this.GetType().FullName };
        }




        private void guessType()
        {
            if (_function.Method.ReturnType == typeof(Complex))
            {
                switch (_function.Method.GetParameters().Length)
                {
                    case 1:
                        this.FunctionType = FunctionType.Complex;
                        break;
                    case 2:
                        this.FunctionType = FunctionType.ComplexImplicit;
                        break;
                }
            }
            else if (_function.Method.ReturnType == typeof(double))
            {

                switch (_function.Method.GetParameters().Length)
                {
                    case 1:
                        this.FunctionType = FunctionType.Real2D;
                        break;
                    case 2:
                        this.FunctionType = FunctionType.Real3D;//or Real2DImplicit, we really cannot tell
                        break;
                    case 3:
                        this.FunctionType = FunctionType.Real3DImplicit;
                        break;
                }

            }
        }



        public virtual T Evaluate<T>(params T[] arguments)
        {
            var value = default(T);

            try
            {
                switch (FunctionType)
                {
                    case FunctionType.Real2D:
                    case FunctionType.Complex:
                        value = (T)_function.DynamicInvoke(arguments[0]);
                        break;
                    case FunctionType.Real2DImplicit:
                    case FunctionType.Real3D:
                    case FunctionType.ComplexImplicit:
                        value = (T)_function.DynamicInvoke(arguments[0], arguments[1]);
                        break;
                    case FunctionType.Real3DImplicit:
                        value = (T)_function.DynamicInvoke(arguments[0], arguments[1], arguments[2]);
                        break;
                }
            }
            catch (Exception exception)
            {
                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                logger.Parameters["TSLCode"] = tslCode;
                logger.Parameters["CSCode"] = csCode;
                logger.Parameters["FunctionType"] = FunctionType;
                logger.Parameters["Name"] = Name;
                logger.Parameters["arg0"] = arguments[0];
                if (arguments.Length >= 2)
                    logger.Parameters["arg1"] = arguments[1];
                if (arguments.Length >= 3)
                    logger.Parameters["arg2"] = arguments[2];
                logger.Log(exception.Message, ErrorType.Calculation, exception);
            }
            return value;
        }
        // public virtual Complex Evaluate(params Complex[] arguments) { return new Complex(double.NaN,double.NaN);}
        public string Name { get { return tslCode; } }
    }
}