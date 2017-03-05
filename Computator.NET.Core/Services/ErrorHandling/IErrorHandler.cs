using System;
using Computator.NET.DataTypes;

namespace Computator.NET.Core.Services.ErrorHandling
{
    public interface IErrorHandler
    {
        void DisplayError(string message, string title);
        void LogError(string message, ErrorType errorType, Exception ex);
    }
}