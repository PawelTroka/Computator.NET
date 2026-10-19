using System;
using System.Reflection;

// The original Worker.cs workloads, boundaries, counts and oracles are unchanged.
// This same entry point is compiled for both historical and modern workers.
internal static class DirectExpressionEntryPoint
{
    [STAThread]
    private static int Main(string[] args)
    {
        AppContext.SetSwitch("Computator.Tsl.DisableDirectExpressions", Array.IndexOf(args, "--roslyn-only") >= 0);
        // Invoke before the worker's first recorded clock; do not evaluate,
        // translate, compile or prewarm any product operation here.
        return (int)typeof(Worker).GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { args });
    }
}
