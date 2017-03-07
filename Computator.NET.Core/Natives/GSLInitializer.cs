using System;
using System.IO;
using System.Linq;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.DataTypes.SettingsTypes;

namespace Computator.NET.Core.Natives
{
    public class GSLInitializer
    {
        private static gsl_error_handler_t UnmanagedHandler;

        private static readonly SimpleLogger.SimpleLogger logger = new SimpleLogger.SimpleLogger(AppInformation.Name)
        {
            ClassName = nameof(GSLInitializer)
        };

        public static void Initialize()
        {
            UnmanagedHandler = HandleUnmanagedException;

            AddGslLocationToEnvironmentalVariableForLibraries();

            if (!RuntimeInformation.IsUnix)
            {
                //for now we have decided to only copy gsl libs from resources to PATH/LD_LIBRARY_PATH/DYLD_LIBRARY_PATH in case of Windows
                //this is because on Mono/Unix we can use DllMap functionality which allows us to specify different names for each system and architecture
                //so on Mono/Unix we are just copying each gsl lib for platform dependent name to output dir so it can be easily called thanks to dllmap being platform specific
                //see issues #30 and #25 for more information

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


                if (RuntimeInformation.IsUnix)
                    ExtractEmbeddedDlls(GslConfig.CblasLibraryName,
                        RuntimeInformation.Is64Bit
                            ? (RuntimeInformation.IsMacOS
                                ? Resources.libgslcblas_osx_amd64
                                : Resources.libgslcblas_amd64)
                            : (RuntimeInformation.IsMacOS ? Resources.libgslcblas_osx_i686 : Resources.libgslcblas_i686));
            }

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
                $"{Strings.GSLInitializer_HandleUnmanagedException_Exception_occcured_in} {file}\n {Strings.GSLInitializer_HandleUnmanagedException_at_line} {line}\n{Strings.GSLInitializer_HandleUnmanagedException_Reason}: {reason}\n{Strings.GSLInitializer_HandleUnmanagedException_Error_code}: {gsl_errno}");
        }
    }
}