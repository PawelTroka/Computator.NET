using System;
using System.IO;
using System.Reflection;

namespace Computator.NET.DataTypes
{
    public static class AppInformation
    {
        private static readonly AssemblyName An = Assembly.GetExecutingAssembly().GetName();
        public static readonly string Version = $"v{An.Version}ß";

        public const string Name = "Computator.NET";

        public static readonly string Directory = Path.GetDirectoryName(new System.Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        public static readonly string DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppInformation.Name);
        public static readonly string LogsDirectory = Path.Combine(DataDirectory, "Logs");
        public static readonly string SettingsPath = Path.Combine(DataDirectory, "settings.dat");

        public static readonly string TempDirectory = Path.Combine(Path.GetTempPath(),
            $"{An.Name}.{An.ProcessorArchitecture}.{An.Version}");
    }
}