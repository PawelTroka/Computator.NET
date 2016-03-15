using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.Localization;
using Computator.NET.Logging;
using Computator.NET.Properties;
using Computator.NET.UI.Forms;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET
{
    internal static class Program
    {
        private static readonly SimpleLogger logger = new SimpleLogger {ClassName = "Program"};
        private static gsl_error_handler_t UnmanagedHandler;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Thread.CurrentThread.CurrentCulture = Settings.Default.Language;
            Thread.CurrentThread.CurrentUICulture = Settings.Default.Language;

            Application.EnableVisualStyles();
            LoadingScreen.ShowSplashScreen();


            EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.gslDllName,
                (Environment.Is64BitProcess) ? Resources.gsl_x64 : Resources.gsl_x86);
            EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.gslCblasDllName,
                (Environment.Is64BitProcess) ? Resources.cblas_x64 : Resources.cblas_x86);

            // NativeMethods.gsl_set_error_handler_off();
            UnmanagedHandler = HandleUnmanagedException;
            NativeMethods.gsl_set_error_handler(UnmanagedHandler);

            Thread.CurrentThread.CurrentCulture = Settings.Default.Language;
            Thread.CurrentThread.CurrentUICulture = Settings.Default.Language;

            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new MyMessageFilter());

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var mainForm = new GUI();

            LoadingScreen.CloseForm();
            Application.Run(mainForm);
        }

        private static void HandleUnmanagedException(string reason,
            string file, int line, int gsl_errno)
        {
            throw new Exception(
                $"Exception occcured in {file}\n at line {line}\nReason: {reason}\nError code: {gsl_errno}");
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, Strings.Program_Application_ThreadException_Unhandled_Thread_Exception);

            logger.MethodName = MethodBase.GetCurrentMethod().Name;
            logger.Log(Strings.Program_Application_ThreadException_Unhandled_Thread_Exception, ErrorType.General, e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception).Message,
                Strings.Program_CurrentDomain_UnhandledException_Unhandled_UI_Exception);

            logger.MethodName = MethodBase.GetCurrentMethod().Name;
            logger.Log(Strings.Program_CurrentDomain_UnhandledException_Unhandled_UI_Exception, ErrorType.General, (e.ExceptionObject as Exception));
        }
    }
}