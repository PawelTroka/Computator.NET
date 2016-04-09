using System;
using System.Reflection;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.Logging;

namespace Computator.NET.DataTypes
{
    public class ScriptFunction : BaseFunction
    {
        public ScriptFunction(Delegate function) : base(function)
            
        {
            FunctionType = FunctionType.Scripting;

            logger = new SimpleLogger {ClassName = GetType().FullName};
        }

        public void Evaluate(Action<string> consoleCallback)
        {
            try
            {
                ((Action<Action<string>>) _function)(consoleCallback);
            }
            catch (Exception exception)
            {
                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                logger.Parameters["TSLCode"] = TslCode;
                logger.Parameters["CSCode"] = CsCode;
                logger.Parameters["FunctionType"] = FunctionType;
                logger.Log(exception.Message, ErrorType.Calculation, exception);

                var message = "Calculation Error, details:" + Environment.NewLine + exception.Message;

                throw new CalculationException(message, exception);
            }
        }
    }
}