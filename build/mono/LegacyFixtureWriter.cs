// Test-only entry point for the existing trusted historical-format writer.
// This standalone process writes known data and never deserializes input.
internal static class LegacyFixtureWriter
{
    private static int Main(string[] args)
    {
        if (args.Length != 1) return 2;
        Computator.NET.Core.Properties.Settings.Write(args[0]);
        return System.IO.File.Exists(args[0]) ? 0 : 1;
    }
}
