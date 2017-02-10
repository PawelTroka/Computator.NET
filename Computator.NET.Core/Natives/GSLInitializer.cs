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

        private static readonly SimpleLogger.SimpleLogger logger = new SimpleLogger.SimpleLogger(GlobalConfig.AppName) { ClassName = nameof(GSLInitializer)};

        public static void Initialize(IMessagingService messagingService)
        {
            UnmanagedHandler = HandleUnmanagedException;


            byte[] gsl;

            if (Environment.Is64BitProcess && IntPtr.Size == 8)
                gsl = GlobalConfig.IsUnix ? Resources.libgsl_amd64 : Resources.gsl_x64;
            else if (!Environment.Is64BitProcess && IntPtr.Size == 4)
                gsl = GlobalConfig.IsUnix ? Resources.libgsl_i686 : Resources.gsl_x86;
            else
                throw new PlatformNotSupportedException("Inconsistent operating system. Handles only 32 and 64 bit OS.");

            var cblas = Environment.Is64BitProcess ? Resources.libgslcblas_amd64 : Resources.libgslcblas_i686;

            try
            {

                EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.GslDllName, gsl);

                
                if (GlobalConfig.IsUnix)
                {
                    EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.GslCblasDllName, cblas);
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
            catch (Exception exception)
            {
                logger.Log("ExtractEmbeddedDlls failed", ErrorType.General, exception);
                try
                {

                    var gslTempPath = Path.Combine(Path.GetTempPath(), GlobalConfig.GslDllName);

                    File.WriteAllBytes(gslTempPath, gsl);

                    if (GlobalConfig.IsUnix)
                    {
                        var cblasTempPath = Path.Combine(Path.GetTempPath(), GlobalConfig.GslCblasDllName);
                        File.WriteAllBytes(cblasTempPath, cblas);

                        var h1 = NativeMethods.dlopen(cblasTempPath, NativeMethods.RTLD.RTLD_GLOBAL);
                        if (h1 == IntPtr.Zero)
                        {
                            throw new Win32Exception(
                                $"{Strings.GSLInitializer_Initialize_Could_not_load_the_Computator_NET_modules_at_the_paths} '{cblasTempPath}'{Environment.NewLine}.",
                                new Win32Exception()); // Calls GetLastError   
                        }
                    }
                    
                    var h2 = GlobalConfig.IsUnix ? NativeMethods.dlopen(gslTempPath, NativeMethods.RTLD.RTLD_GLOBAL) : NativeMethods.LoadLibrary(gslTempPath);

                    if (h2 == IntPtr.Zero)
                    {
                        throw new Win32Exception(
                            $"{Strings.GSLInitializer_Initialize_Could_not_load_the_Computator_NET_modules_at_the_paths} '{gslTempPath}'{Environment.NewLine}.",
                            new Win32Exception()); // Calls GetLastError                       
                    }

                    NativeMethods.gsl_set_error_handler(UnmanagedHandler);
                }
                catch (Exception exception2)
                {
                    var funcName = GlobalConfig.IsUnix ? "dlopen" : "LoadLibrary";
                    logger.Log($"{funcName} failed", ErrorType.General, exception2);
                    messagingService.Show(
                        $"{Strings.Program_Main_Exception_during_startup}.{Environment.NewLine}ExtractEmbeddedDlls {Strings.Exception}:{Environment.NewLine}{exception}{Environment.NewLine}{funcName} {Strings.Exception}:{Environment.NewLine}{exception2}",
                        Strings.Error);
                }
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