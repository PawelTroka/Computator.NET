using System;
using Computator.NET.Config;

namespace Computator.NET
{
    public interface IErrorHandler
    {
        void DispalyError(string message, string title);
        void LogError(string message, ErrorType errorType, Exception ex);
    }
}