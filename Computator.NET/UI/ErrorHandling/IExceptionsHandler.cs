using System;

namespace Computator.NET.UI.ErrorHandling
{
    public interface IExceptionsHandler
    {
        void HandleException(Exception ex);
    }
}