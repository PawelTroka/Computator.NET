using System;
using System.Reflection;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.Logging;

namespace Computator.NET
{
    public class SimpleErrorHandler : IErrorHandler
    {
        private static SimpleErrorHandler _instance;
        private readonly SimpleLogger logger;

        private SimpleErrorHandler()
        {
            logger = new SimpleLogger(this);
        }

        public static SimpleErrorHandler Instance => _instance ?? (_instance = new SimpleErrorHandler());

        public void DispalyError(string message, string title)
        {
            MessageBox.Show(message, title);
        }

        public void LogError(string message, ErrorType errorType, Exception ex)
        {
            logger.MethodName = MethodBase.GetCurrentMethod().Name;
            logger.Log(message, ErrorType.General, ex);
        }
    }
}