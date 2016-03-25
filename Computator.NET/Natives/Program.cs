using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Localization;
using Computator.NET.Logging;
using Computator.NET.UI.Forms;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET
{
    internal static class Program
    {
        private static readonly SimpleLogger logger = new SimpleLogger { ClassName = "Program" };
        private static gsl_error_handler_t UnmanagedHandler;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Thread.CurrentThread.CurrentCulture = Settings.Default.Language;
            Thread.CurrentThread.CurrentUICulture = Settings.Default.Language;
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            LoadingScreen.ShowSplashScreen();

            GSLInitializer.Initialize();

            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new MyMessageFilter());

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
            var ex = (e.ExceptionObject as Exception);
            MessageBox.Show(ex.Message,
                Strings.Program_CurrentDomain_UnhandledException_Unhandled_UI_Exception);

            if (ex.Message.Contains("Font")|| ex.Message.Contains("font"))
            {
                //e.IsTerminating = false;
                
                MessageBox.Show("Try installing font!");
                Process.Start(GlobalConfig.FullPath("UI", "fonts", "CAMBRIA.TTC"));
            }
            

            logger.MethodName = MethodBase.GetCurrentMethod().Name;
            logger.Log(Strings.Program_CurrentDomain_UnhandledException_Unhandled_UI_Exception, ErrorType.General, ex);
        }
    }
}