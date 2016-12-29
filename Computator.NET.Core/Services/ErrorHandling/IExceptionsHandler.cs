using System;

namespace Computator.NET.Core.Services.ErrorHandling
{
    public interface IExceptionsHandler
    {
        void HandleException(Exception ex);
    }
}