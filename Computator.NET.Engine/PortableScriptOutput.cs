using System.Globalization;

namespace Computator.NET.Engine;

/// <summary>Bounded result output for portable scripts and their custom functions.</summary>
public static class PortableScriptOutput
{
    private static readonly AsyncLocal<Sink?> Current = new();

    public static void write(object? value) => CurrentSink().Write(Format(value));

    public static void writeln(object? value) => CurrentSink().WriteLine(Format(value));

    private static string Format(object? value) => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

    private static Sink CurrentSink() => Current.Value
        ?? throw new InvalidOperationException("Script output is only available while a script is executing.");

    internal static OutputScope Capture(Action<string> append) => new(append);

    internal sealed class OutputScope : IDisposable
    {
        private readonly Sink? _previous;
        private readonly Sink _sink;
        private bool _disposed;

        internal OutputScope(Action<string> append)
        {
            _previous = Current.Value;
            _sink = new Sink(append);
            Append = _sink.Write;
            Current.Value = _sink;
        }

        internal Action<string> Append { get; }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _sink.Close();
            Current.Value = _previous;
        }
    }

    private sealed class Sink(Action<string> append)
    {
        private readonly object _gate = new();
        private Action<string>? _append = append;

        internal void Write(string text)
        {
            lock (_gate) _append?.Invoke(text);
        }

        internal void WriteLine(string text)
        {
            lock (_gate)
            {
                if (_append == null) return;
                // Check before allocating the joined line; the existing append
                // callback checks its full length against the remaining budget.
                if (text.Length > PortableEngine.MaximumSourceCharacters - Environment.NewLine.Length)
                    throw new InvalidOperationException("Script output exceeds the configured limit.");
                _append(text + Environment.NewLine);
            }
        }

        internal void Close()
        {
            // Child execution contexts can outlive their script. They retain a
            // closed sink, never the result buffer or a later script's output.
            lock (_gate) _append = null;
        }
    }
}
