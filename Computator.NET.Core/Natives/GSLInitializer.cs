using System;
using System.IO;
using System.Linq;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Localization;
using NLog;

namespace Computator.NET.Core.Natives
{
    public class GSLInitializer
    {
        private static gsl_error_handler_t UnmanagedHandler;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public static void Initialize()
        {
            Environment.CurrentDirectory = AppInformation.Directory;

            UnmanagedHandler = HandleUnmanagedException;

            AddGslLocationToEnvironmentalVariableForLibraries();

            byte[] gsl;

            if (RuntimeInformation.Is64Bit)
                gsl = RuntimeInformation.IsUnix
                    ? (RuntimeInformation.IsMacOS ? Resources.libgsl_osx_amd64 : Resources.libgsl_amd64)
                    : Resources.gsl_x64;
            else if (RuntimeInformation.Is32Bit)
                gsl = RuntimeInformation.IsUnix
                    ? (RuntimeInformation.IsMacOS ? Resources.libgsl_osx_i686 : Resources.libgsl_i686)
                    : Resources.gsl_x86;
            else
                throw new PlatformNotSupportedException(
                    "Inconsistent operating system. Handles only 32 and 64 bit OS.");


            ExtractEmbeddedDlls(GslConfig.GslLibraryName, gsl);
            
            switch (Settings.Default.CalculationsErrors)
            {
                case CalculationsErrors.ReturnNAN:
                    NativeMethods.gsl_set_error_handler_off();
                    break;
                case CalculationsErrors.ShowError:
                    NativeMethods.gsl_set_error_handler(UnmanagedHandler);
                    break;
            }
        }

        private static void ExtractEmbeddedDlls(string dllName, byte[] resourceBytes)
        {
            if (!Directory.Exists(GslConfig.Location))
                Directory.CreateDirectory(GslConfig.Location);

            // See if the file exists, avoid rewriting it if not necessary
            var dllPath = Path.Combine(GslConfig.Location, dllName);
            var rewrite = true;
            if (File.Exists(dllPath))
            {
                var existing = File.ReadAllBytes(dllPath);
                if (resourceBytes.SequenceEqual(existing))
                    rewrite = false;
            }
            if (rewrite)
                File.WriteAllBytes(dllPath, resourceBytes);
            if (!File.Exists(dllPath))
                throw new FileNotFoundException($"Couldn't write to file {dllPath}.", dllPath);
        }

        private static void AddGslLocationToEnvironmentalVariableForLibraries()
        {
            string environmentPathForLibraries;

            if (RuntimeInformation.IsMacOS)
                environmentPathForLibraries = "DYLD_LIBRARY_PATH";
            else if (RuntimeInformation.IsLinux)
                environmentPathForLibraries = "LD_LIBRARY_PATH";
            else if (RuntimeInformation.IsWindows)
                environmentPathForLibraries = "PATH";
            else
                throw new PlatformNotSupportedException(
                    "This platform does not support sharing native libraries across assemblies");

            var environmentValuesSeparator = RuntimeInformation.IsUnix ? ':' : ';';

            // Add the temporary dirName to the PATH environment variable (at the head!)
            var path = Environment.GetEnvironmentVariable(environmentPathForLibraries) ?? "";
            //Environment variable names are not case-sensitive.

            var pathPieces = path.Split(environmentValuesSeparator);
            var found = false;
            foreach (var pathPiece in pathPieces)
                if (pathPiece == GslConfig.Location)
                {
                    found = true;
                    break;
                }
            if (!found)
                Environment.SetEnvironmentVariable(environmentPathForLibraries,
                    GslConfig.Location + environmentValuesSeparator + path);

            path = Environment.GetEnvironmentVariable(environmentPathForLibraries) ?? "";

            if (!path.Contains(GslConfig.Location))
                throw new Exception("Couldn't add gsl to PATH Environmet Variable\npath = \n" + path);
        }

        private static void HandleUnmanagedException(string reason,
            string file, int line, int gsl_errno)
        {
            throw new Exception(
                $"{Strings.Exception_occcured_in} {file}\n {Strings.at_line} {line}\n{Strings.Reason}: {reason}\n{Strings.Error_code}: {gsl_errno}");
        }
    }
}