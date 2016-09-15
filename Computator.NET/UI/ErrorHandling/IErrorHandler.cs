using System;
using Computator.NET.Logging;

namespace Computator.NET.UI.ErrorHandling
{
    public interface IErrorHandler
    {
        void DispalyError(string message, string title);
        void LogError(string message, ErrorType errorType, Exception ex);
    }
}