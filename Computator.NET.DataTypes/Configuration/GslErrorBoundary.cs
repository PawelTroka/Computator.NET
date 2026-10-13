using System;
using System.Threading;

namespace Computator.NET.DataTypes.Configuration
{
    /// <summary>Translate GSL callback errors only after control returns to managed code.</summary>
    public sealed class GslErrorBoundary : IDisposable
    {
        [ThreadStatic] private static GslErrorBoundary _current;
        private static bool _throwOnError;
        public static bool ThrowOnError
        {
            get => Volatile.Read(ref _throwOnError);
            set => Volatile.Write(ref _throwOnError, value);
        }
        private readonly GslErrorBoundary _previous;
        private readonly bool _throw;
        private int _status;
        private string _reason;
        private GslErrorBoundary()
        {
            _previous = _current;
            _throw = ThrowOnError;
            _current = this;
        }
        public static GslErrorBoundary Enter() => new GslErrorBoundary();
        // This callback path must never throw across unmanaged frames or perform
        // logging, user callbacks, formatting, allocation or settings IO.
        public static void Record(string reason, int status)
        {
            var scope = _current;
            if (scope == null || scope._status != 0) return;
            scope._reason = reason;
            scope._status = status;
        }
        public void ThrowIfError()
        {
            if (_throw && _status != 0) throw new ArithmeticException("GSL error " + _status + ": " + _reason);
        }
        public void Dispose() { _current = _previous; }
    }
}
