using System;
using System.Reflection;

namespace Computator.NET.DataTypes
{
    public class ScriptFunction : BaseFunction
    {
        public ScriptFunction(Delegate function) : base(function)

        {
            FunctionType = FunctionType.Scripting;
        }

        public void Evaluate(Action<string> consoleCallback)
        {
            try
            {
                ((Action<Action<string>>) _function)(consoleCallback);
            }
            catch (Exception exception)
            {
                var message = "Calculation Error, details:" + Environment.NewLine + exception.Message;
                Logger.Error(exception, $"{message}.{Environment.NewLine}{nameof(TslCode)} = '{TslCode}'{Environment.NewLine}{nameof(CsCode)} = '{CsCode}'{Environment.NewLine}{nameof(FunctionType)} = '{FunctionType}'");
                throw new CalculationException(message, exception);
            }
        }
    }
}