using Computator.NET.Desktop.Tests;

internal static partial class Program
{
    private static void ChartExportFormats()
    {
        foreach (var kind in ChartExportContract.ChartKinds)
        {
            foreach (var extension in ChartExportContract.RasterExtensions)
                Run("native-chart-export-" + kind + "-" + extension,
                    () => ChartExportContract.RoundTrip(kind, extension));
            Run("native-chart-export-" + kind + "-unsupported-preserves-files",
                () => ChartExportContract.UnsupportedFormatsPreserveFiles(kind));
        }
        Run("native-chart-export-localized-choices-and-mode-switches", ChartExportContract.LocalizedChoicesAndModeSwitches);
    }
}
