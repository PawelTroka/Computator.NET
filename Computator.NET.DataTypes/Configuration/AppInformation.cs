using System;
using System.IO;
using System.Reflection;

namespace Computator.NET.DataTypes.Configuration
{
    public static class AppInformation
    {
        private static readonly AssemblyName An = typeof(AppInformation).Assembly.GetName();
        public static readonly string Version = $"v{An.Version}";
        public const string Name = "Computator.NET";
        public static readonly string Directory = AppContext.BaseDirectory;
        // An explicit override allows isolated CI and side-by-side migration trials.
        // The ordinary location remains unchanged so existing documents are found.
        public static readonly string DataDirectory = Path.GetFullPath(
            Environment.GetEnvironmentVariable("COMPUTATOR_DATA_DIRECTORY") ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Name));
        public static readonly string LogsDirectory = Path.Combine(DataDirectory, "Logs");
        public static readonly string SettingsPath = Path.Combine(DataDirectory, "settings.dat");
        public static readonly string TempDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Name, "Native",
            System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(), Version);
    }
}
