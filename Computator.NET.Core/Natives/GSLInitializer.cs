using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Logging;
using Computator.NET.Properties;
using Computator.NET.UI.ErrorHandling;

namespace Computator.NET.Natives
{
    public class GSLInitializer
    {
        private static gsl_error_handler_t UnmanagedHandler;

        private static readonly SimpleLogger.SimpleLogger logger = new SimpleLogger.SimpleLogger(GlobalConfig.AppName) { ClassName = nameof(GSLInitializer)};

        public static void Initialize(IMessagingService messagingService)
        {
            UnmanagedHandler = HandleUnmanagedException;

            //  var msvcr = Resources.msvcr120_x86;
            var gsl = Resources.gsl_x86;
            //  var cblas = Resources.cblas_x86;

            if (Environment.Is64BitProcess && IntPtr.Size == 8)
            {
                //msvcr = Resources.msvcr120_x64;
                gsl = Resources.gsl_x64;
                //    cblas = Resources.cblas_x64;
            }
            //else if (!Environment.Is64BitProcess && IntPtr.Size == 4)

            //else
            //  throw new Exception("Inconsistent system - IntPtr.Size and Environment.Is64BitProcess dont match");

            try
            {
                //       EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.msvcrDllName, msvcr);
                EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.GslDllName, gsl);
                //    EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.gslCblasDllName, cblas);
                //   System.Threading.Thread.Sleep(1000);
                NativeMethods.gsl_set_error_handler(UnmanagedHandler);
            }
            catch (Exception exception)
            {
                logger.Log("ExtractEmbeddedDlls failed", ErrorType.General, exception);
                try
                {
                    //  string msvcrTempPath = Path.Combine(Path.GetTempPath(), GlobalConfig.msvcrDllName);
                    var gslTempPath = Path.Combine(Path.GetTempPath(), GlobalConfig.GslDllName);
                    //  string cblasTempPath = Path.Combine(Path.GetTempPath(), GlobalConfig.gslCblasDllName);

                    //     File.WriteAllBytes(msvcrTempPath, msvcr);
                    // File.WriteAllBytes(cblasTempPath, cblas);
                    File.WriteAllBytes(gslTempPath, gsl);

                    //  var h0 = NativeMethods.LoadLibrary(msvcrTempPath);
                    //    var h1 = NativeMethods.LoadLibrary(cblasTempPath);
                    var h2 = NativeMethods.LoadLibrary(gslTempPath);

                    if ( /*h0 == IntPtr.Zero || h1 == IntPtr.Zero ||*/ h2 == IntPtr.Zero)
                    {
                        throw new Win32Exception(
                            $"{Strings.GSLInitializer_Initialize_Could_not_load_the_Computator_NET_modules_at_the_paths} '{gslTempPath}'{Environment.NewLine}.",
                            new Win32Exception()); // Calls GetLastError                       
                    }

                    NativeMethods.gsl_set_error_handler(UnmanagedHandler);
                }
                catch (Exception exception2)
                {
                    logger.Log("LoadLibrary failed", ErrorType.General, exception2);
                    messagingService.Show(
                        $"{Strings.Program_Main_Exception_during_startup}.{Environment.NewLine}ExtractEmbeddedDlls {Strings.Exception}:{Environment.NewLine}{exception}{Environment.NewLine}LoadLibrary {Strings.Exception}:{Environment.NewLine}{exception2}",
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