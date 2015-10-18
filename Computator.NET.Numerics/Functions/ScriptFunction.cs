using System;
using System.Reflection;
using System.Windows.Forms;
using Computator.NET.Config;

namespace Computator.NET.Functions
{
    class ScriptFunction : BaseFunction
    {
        public ScriptFunction(Delegate function, string tslCode, string csCode)
            : base(function,tslCode, csCode)
        {
            FunctionType=FunctionType.Scripting;

            logger = new SimpleLogger() { ClassName = this.GetType().FullName };
        }

        public void Evaluate(RichTextBox consoleOutput)
        {
            try
            {
                _function.DynamicInvoke(consoleOutput);
            }
            catch (Exception exception)
            {
                logger.MethodName = MethodBase.GetCurrentMethod().Name;
                logger.Parameters["TSLCode"] = tslCode;
                logger.Parameters["CSCode"] = csCode;
                logger.Parameters["FunctionType"] = FunctionType;
                logger.Log(exception.Message, ErrorType.Calculation, exception);
            }
        }
    }
}