using System;
using System.ComponentModel;
using System.IO;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Config;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.DataTypes.SettingsTypes;

namespace Computator.NET.Core.Natives
{
    public class GSLInitializer
    {
        private static gsl_error_handler_t UnmanagedHandler;

        private static readonly SimpleLogger.SimpleLogger logger = new SimpleLogger.SimpleLogger(AppInformation.Name) { ClassName = nameof(GSLInitializer)};

        public static void Initialize()
        {
            UnmanagedHandler = HandleUnmanagedException;

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


                EmbeddedDllClass.ExtractEmbeddedDlls(GslConfig.GslLibraryName, gsl);


                if (RuntimeInformation.IsUnix)
                {
                    EmbeddedDllClass.ExtractEmbeddedDlls(GslConfig.CblasLibraryName,
                        RuntimeInformation.Is64Bit
                            ? (RuntimeInformation.IsMacOS
                                ? Resources.libgslcblas_osx_amd64
                                : Resources.libgslcblas_amd64)
                            : (RuntimeInformation.IsMacOS ? Resources.libgslcblas_osx_i686 : Resources.libgslcblas_i686));
                }
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

        private static void HandleUnmanagedException(string reason,
            string file, int line, int gsl_errno)
        {
            throw new Exception(
                $"{Strings.GSLInitializer_HandleUnmanagedException_Exception_occcured_in} {file}\n {Strings.GSLInitializer_HandleUnmanagedException_at_line} {line}\n{Strings.GSLInitializer_HandleUnmanagedException_Reason}: {reason}\n{Strings.GSLInitializer_HandleUnmanagedException_Error_code}: {gsl_errno}");
        }
    }
}