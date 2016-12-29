using System;
using System.Reflection;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Services.ErrorHandling
{
    public class SimpleErrorHandler : IErrorHandler
    {
        private readonly SimpleLogger.SimpleLogger _logger;
        private readonly IMessagingService _messagingService;

        public SimpleErrorHandler(IMessagingService messagingService)
        {
            _messagingService = messagingService;
            _logger = new SimpleLogger.SimpleLogger((GlobalConfig.AppName));
        }

  
        public void DispalyError(string message, string title)
        {
            _messagingService.Show(message, title);
        }

        public void LogError(string message, ErrorType errorType, Exception ex)
        {
            _logger.MethodName = MethodBase.GetCurrentMethod().Name;
            _logger.Log(message, ErrorType.General, ex);
        }
    }
}