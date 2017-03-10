using System;
using System.Reflection;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.DataTypes;
using NLog;

namespace Computator.NET.Core.Services.ErrorHandling
{
    public class SimpleErrorHandler : IErrorHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IMessagingService _messagingService;

        public SimpleErrorHandler(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

  
        public void DisplayError(string message, string title)
        {
            _messagingService.Show(message, title);
        }

        public void LogError(string message, ErrorType errorType, Exception ex)
        {
            var logEvent = new LogEventInfo(LogLevel.Error, Logger.Name,
                message + $" {nameof(errorType)} = '{errorType}'") {Exception = ex};

            Logger.Log(typeof(SimpleErrorHandler),logEvent);
        }
    }
}