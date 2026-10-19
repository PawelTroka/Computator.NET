#if NETFRAMEWORK
using System;

namespace Computator.NET.Core.Compatibility
{
    /// <summary>
    /// Framework replacement for Random.Shared/NextInt64, also called by compiled
    /// TSL code. Preserves concurrent access and inclusive Int32 endpoint support.
    /// </summary>
    public sealed class FrameworkRandom
    {
        public static readonly FrameworkRandom Shared = new FrameworkRandom();
        private readonly object _gate = new object();
        private readonly Random _random = new Random();
        private readonly byte[] _bytes = new byte[8];

        private FrameworkRandom() { }

        public double NextDouble()
        {
            lock (_gate) return _random.NextDouble();
        }

        public long NextInt64(long maximum) => NextInt64(0, maximum);

        public long NextInt64(long minimum, long maximum)
        {
            if (minimum > maximum) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (minimum == maximum) return minimum;
            var range = unchecked((ulong)(maximum - minimum));
            // Rejection removes modulo bias, including ranges crossing zero and
            // the full inclusive Int32 interval used by random(int, int).
            var threshold = unchecked(0UL - range) % range;
            lock (_gate)
            {
                ulong sample;
                do
                {
                    _random.NextBytes(_bytes);
                    sample = BitConverter.ToUInt64(_bytes, 0);
                } while (sample < threshold);
                return unchecked((long)(sample % range) + minimum);
            }
        }
    }
}
#endif
