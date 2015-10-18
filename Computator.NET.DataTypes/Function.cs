using System;
using System.Numerics;
using System.Reflection;
using Computator.NET.Config;
using Computator.NET.Logging;

namespace Computator.NET.DataTypes
{
    public class Function : BaseFunction
    {
        public Function(Delegate function, string tslCode, string csCode)
            : base(function, tslCode, csCode)
        {
            guessType();

            logger = new SimpleLogger {ClassName = GetType().FullName};
            //    setInvoker();
        }

        public Function(Delegate function, string tslCode, string csCode, FunctionType functionType)
            : base(function, tslCode, csCode)
        {
            FunctionType = functionType;
            logger = new SimpleLogger {ClassName = GetType().FullName};
            //    setInvoker();
        }

        // public virtual Complex Evaluate(params Complex[] arguments) { return new Complex(double.NaN,double.NaN);}
        public string Name
        {
            get { return tslCode; }
        }

        private void guessType()
        {
            if (_function.Method.ReturnType == typeof (Complex))
            {
                switch (_function.Method.GetParameters().Length)
                {
                    case 1:
                        FunctionType = FunctionType.Complex;
                        break;
                    case 2:
                        FunctionType = FunctionType.ComplexImplicit;
                        break;
                }
            }
            else if (_function.Method.ReturnType == typeof (double))
            {
                switch (_function.Method.GetParameters().Length)
                {
                    case 1:
                        FunctionType = FunctionType.Real2D;
                        break;
                    case 2:
                        FunctionType = FunctionType.Real3D; //or Real2DImplicit, we really cannot tell
                        break;
                    case 3:
                        FunctionType = FunctionType.Real3DImplicit;
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
                        value = ((Func<T, T>) _function)(arguments[0]);
                        break;
                    case FunctionType.Real2DImplicit:
                    case FunctionType.Real3D:
                    case FunctionType.ComplexImplicit:
                        value = ((Func<T, T, T>) _function)(arguments[0], arguments[1]);
                        break;
                    case FunctionType.Real3DImplicit:
                        value = ((Func<T, T, T, T>) _function)(arguments[0], arguments[1], arguments[2]);
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

                var message = "Calculation Error, details:" + Environment.NewLine + exception.Message;

                throw new CalculationException(message, exception);
            }
            return value;
        }
    }
}