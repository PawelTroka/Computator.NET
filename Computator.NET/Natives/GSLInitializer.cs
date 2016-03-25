using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Logging;
using Computator.NET.Properties;

namespace Computator.NET
{
    class GSLInitializer
    {
        private static gsl_error_handler_t UnmanagedHandler;

        private static readonly SimpleLogger logger = new SimpleLogger { ClassName = nameof(GSLInitializer) };

        public static void Initialize()
        {
            UnmanagedHandler = HandleUnmanagedException;

            byte[] gsl = null;
            byte[] cblas = null;

            if (Environment.Is64BitProcess && IntPtr.Size == 8)
            {
                gsl = Resources.gsl_x64;
                cblas = Resources.cblas_x64;
            }
            else if (!Environment.Is64BitProcess && IntPtr.Size == 4)
            {
                gsl = Resources.gsl_x86;
                cblas = Resources.cblas_x86;
            }
            else
                throw new Exception("Inconsistent system - IntPtr.Size and Environment.Is64BitProcess dont match");

            try
            {
                EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.gslDllName, gsl);
                EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.gslCblasDllName, cblas);

                NativeMethods.gsl_set_error_handler(UnmanagedHandler);
            }
            catch (Exception exception)
            {
                logger.Log("ExtractEmbeddedDlls failed", ErrorType.General, exception);
                try
                {
                    string gslTempPath = Path.Combine(Path.GetTempPath(), GlobalConfig.gslDllName);
                    string cblasTempPath = Path.Combine(Path.GetTempPath(), GlobalConfig.gslCblasDllName);

                    File.WriteAllBytes(cblasTempPath, cblas);
                    File.WriteAllBytes(gslTempPath, gsl);

                    var h1 = NativeMethods.LoadLibrary(cblasTempPath);
                    var h2 = NativeMethods.LoadLibrary(gslTempPath);

                    if (h1 == IntPtr.Zero || h2 == IntPtr.Zero)
                    {
                            throw new Win32Exception($"Could not load the Computator.NET modules at the paths '{gslTempPath}'{Environment.NewLine}'{cblasTempPath}'.", new Win32Exception()); // Calls GetLastError                       
                    }

                    NativeMethods.gsl_set_error_handler(UnmanagedHandler);
                }
                catch (Exception exception2)
                {
                    logger.Log("LoadLibrary failed", ErrorType.General, exception2);
                    MessageBox.Show($"{Strings.Program_Main_Exception_during_startup}.{Environment.NewLine}ExtractEmbeddedDlls exception:{Environment.NewLine}{exception}{Environment.NewLine}LoadLibrary exception:{Environment.NewLine}{exception2}", Strings.Error);
                }
            }
        }

        private static void HandleUnmanagedException(string reason,
            string file, int line, int gsl_errno)
        {
            throw new Exception(
                $"Exception occcured in {file}\n at line {line}\nReason: {reason}\nError code: {gsl_errno}");
        }
    }
}