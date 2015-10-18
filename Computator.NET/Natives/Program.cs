using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.Logging;
using Computator.NET.UI.Forms;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET
{
    internal static class Program
    {
        private static readonly SimpleLogger logger = new SimpleLogger {ClassName = "Program"};

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
            //MultiplatformDllLoader.Enable = true;
            // var dllDir = GlobalConfig.fullPath("Special", (Environment.Is64BitProcess) ? "x64" : "x86");
            // MessageBox.Show(dllDir);
            //SetDllDirectory(dllDir);

           // LoadLibrary(GlobalConfig.FullPath("Special", (Environment.Is64BitProcess) ? "x64" : "x86",
           //     GlobalConfig.gslCblasDllName));

          //  LoadLibrary(GlobalConfig.FullPath("Special", (Environment.Is64BitProcess) ? "x64" : "x86",
            //    GlobalConfig.gslDllName));

            EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.gslDllName, (Environment.Is64BitProcess) ? Properties.Resources.gsl_x64 : Properties.Resources.gsl_x86);
            EmbeddedDllClass.ExtractEmbeddedDlls(GlobalConfig.gslCblasDllName, (Environment.Is64BitProcess) ? Properties.Resources.cblas_x64 : Properties.Resources.cblas_x86);

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

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception");

            logger.MethodName = MethodBase.GetCurrentMethod().Name;
            logger.Log("Unhandled Thread Exception", ErrorType.General, e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception).Message, "Unhandled UI Exception");

            logger.MethodName = MethodBase.GetCurrentMethod().Name;
            logger.Log("Unhandled UI Exception", ErrorType.General, (e.ExceptionObject as Exception));
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);
    }
}