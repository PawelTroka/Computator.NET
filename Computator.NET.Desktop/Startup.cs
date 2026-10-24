using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Config;
using Computator.NET.Desktop.Dialogs;
using Computator.NET.Desktop.Views;
using Computator.NET.Localization;
using NLog;

namespace Computator.NET.Desktop
{
    public class Startup
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, Strings.Program_Application_ThreadException_Unhandled_Thread_Exception);
            Logger.Error(e.Exception, Strings.Program_Application_ThreadException_Unhandled_Thread_Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            MessageBox.Show(exception?.Message ?? "Unhandled application error", Strings.Unhandled_UI_Exception);
            Logger.Error(exception, Strings.Unhandled_UI_Exception + $"; terminating: {e.IsTerminating}");
        }

        public static void Start()
        {
#if __MonoCS__
            // Set this before creating any WinForms controls. Mono's X11 input
            // method path can fail during startup on systems without an XIM server.
            if (RuntimeInformation.IsUnix)
                Environment.SetEnvironmentVariable("MONO_WINFORMS_XIM_STYLE", "disabled");
#endif
            if (RuntimeInformation.IsWindows10S)
            {
                MessageBox.Show("Windows 10S is not supported. Computator.NET will exit now.", Strings.Error);
                return;
            }
            // The former manifest requested PerMonitorV2. Preserve that mode via the
            // modern API, and configure text rendering before the splash creates HWNDs.
#if !__MonoCS__
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            LogsConfigurator.Configure();
            EnvironmentLogger.LogEnvironmentInformation();
            Thread.CurrentThread.CurrentCulture = Settings.Default.Language ?? new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = Settings.Default.Language ?? new CultureInfo("en");
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            LoadingScreen.ShowSplashScreen();
            GSLInitializer.Initialize();
            Settings.Default.RestoreScriptingExamples();
            if (RuntimeInformation.IsWindows)
                Application.AddMessageFilter(new MyMessageFilter());
            var bootstrapper = new WinFormsBootstrapper();
            using (bootstrapper.Container)
            using (var mainForm = bootstrapper.Create<MainView>())
            {
                LoadingScreen.CloseForm();
                Application.Run(mainForm);
            }
        }
    }
}
