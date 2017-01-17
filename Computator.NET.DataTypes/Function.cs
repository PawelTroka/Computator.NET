using System;
using System.Numerics;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

namespace Computator.NET.DataTypes
{
    public class Function : BaseFunction
    {
        private string name;

        private Function(Delegate function) : base(function)
        {
            logger = new SimpleLogger.SimpleLogger(GlobalConfig.AppName) { ClassName = GetType().FullName};
        }

        public Function(Delegate function, bool isImplicit) : this(function)
        {
            DeduceType(isImplicit);
        }

        public Function(Delegate function, FunctionType functionType) : this(function)
        {
            FunctionType = functionType;
        }

        // public virtual Complex Evaluate(params Complex[] arguments) { return new Complex(double.NaN,double.NaN);}
        public string Name
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
                if (!string.IsNullOrWhiteSpace(TslCode))
                    return TslCode;
                if (!string.IsNullOrWhiteSpace(CsCode))
                    return CsCode;
                return "Function" + GetHashCode();
            }
            set { name = value; }
        }

        public static implicit operator Func<double>(Function function)
        {
            return function._function as Func<double>;
        }

        public static implicit operator Func<double, double>(Function function)
        {
            return function._function as Func<double, double>;
        }

        public static implicit operator Func<double, double, double>(Function function)
        {
            return function._function as Func<double, double, double>;
        }

        public static implicit operator Func<double, double, double, double>(Function function)
        {
            return function._function as Func<double, double, double, double>;
        }


        public static implicit operator Func<Complex>(Function function)
        {
            return function._function as Func<Complex>;
        }

        public static implicit operator Func<Complex, Complex>(Function function)
        {
            return function._function as Func<Complex, Complex>;
        }

        public static implicit operator Func<Complex, Complex, Complex>(Function function)
        {
            return function._function as Func<Complex, Complex, Complex>;
        }

        public static implicit operator Func<Complex, Complex, Complex, Complex>(Function function)
        {
            return function._function as Func<Complex, Complex, Complex, Complex>;
        }

        private void DeduceType(bool isImplicit)
        {
            if (_function.Method.ReturnType == typeof(Complex))
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
            else if (_function.Method.ReturnType == typeof(double))
            {
                switch (_function.Method.GetParameters().Length)
                {
                    case 1:
                        FunctionType = FunctionType.Real2D;
                        break;
                    case 2:
                        FunctionType = isImplicit ? FunctionType.Real2DImplicit : FunctionType.Real3D;
                        break;
                    case 3:
                        FunctionType = FunctionType.Real3DImplicit;
                        break;
                }
            }
        }


        public dynamic EvaluateDynamic(params double[] arguments)
        {
            try
            {
                switch (FunctionType)
                {
                    case FunctionType.Real2D:
                        return ((Func<double, double>) _function)(arguments[0]);
                    case FunctionType.Complex:
                        return ((Func<Complex, Complex>) _function)(new Complex(arguments[0], arguments[1]));
                    case FunctionType.Real2DImplicit:
                    case FunctionType.Real3D:
                        return ((Func<double, double, double>) _function)(arguments[0], arguments[1]);

                    case FunctionType.ComplexImplicit:
                        return ((Func<Complex, Complex, Complex>) _function)(new Complex(arguments[0], arguments[1]),
                            new Complex(arguments[2], arguments[3]));

                    case FunctionType.Real3DImplicit:
                        return ((Func<double, double, double, double>) _function)(arguments[0], arguments[1],
                            arguments[2]);
                }
            }
            catch (Exception exception)
            {
                if (exception is RuntimeBinderException)
                    //hack for sqrt(x) for x<0 in chart, calculations etc (not in scripting where it returns complex number)
                    return double.NaN;
                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                logger.Parameters["TSLCode"] = TslCode;
                logger.Parameters["CSCode"] = CsCode;
                logger.Parameters["FunctionType"] = FunctionType;
                logger.Parameters["Name"] = Name;
                logger.Parameters["arg0"] = arguments[0];
                if (arguments.Length >= 2)
                    logger.Parameters["arg1"] = arguments[1];
                if (arguments.Length >= 3)
                    logger.Parameters["arg2"] = arguments[2];
                if (arguments.Length >= 4)
                    logger.Parameters["arg3"] = arguments[3];

                logger.Log(exception.Message, ErrorType.Calculation, exception);

                var message = "Calculation Error, details:" + Environment.NewLine + exception.Message;

                throw new CalculationException(message, exception);
            }
            return double.NaN;
        }

        public virtual T Evaluate<T>(params T[] arguments)
        {
            var value = default(T);

            try
            {
                switch (FunctionType)
                {
                    case FunctionType.Real2D:
                        value = ((Func<T, T>) _function)(arguments[0]);
                        break;
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
                if (exception is RuntimeBinderException)
                    //hack for sqrt(x) for x<0 in chart, calculations etc (not in scripting where it returns complex number)
                    return (T) (object) double.NaN;
                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                logger.Parameters["TSLCode"] = TslCode;
                logger.Parameters["CSCode"] = CsCode;
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