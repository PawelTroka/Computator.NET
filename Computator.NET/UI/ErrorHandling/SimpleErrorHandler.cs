using System;
using System.Reflection;
using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.Logging;

namespace Computator.NET.UI.ErrorHandling
{
    public class SimpleErrorHandler : IErrorHandler
    {
        private readonly SimpleLogger.SimpleLogger _logger;

        public SimpleErrorHandler()
        {
            _logger = new SimpleLogger.SimpleLogger((GlobalConfig.AppName));
        }

  
        public void DispalyError(string message, string title)
        {
            MessageBox.Show(message, title);
        }

        public void LogError(string message, ErrorType errorType, Exception ex)
        {
            _logger.MethodName = MethodBase.GetCurrentMethod().Name;
            _logger.Log(message, ErrorType.General, ex);
        }
    }
}