using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Bootstrapping;
using Computator.NET.Config;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Dialogs;
using Computator.NET.Services;
using Computator.NET.Views;
using NLog;
using NLog.Fluent;

namespace Computator.NET
{
    internal static class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            LogsConfigurator.Configure();
            EnvironmentLogger.LogEnvironmentInformation();

            if (RuntimeInformation.IsUnix)
            {
                Environment.SetEnvironmentVariable("MONO_WINFORMS_XIM_STYLE", "disabled", EnvironmentVariableTarget.Process);
                //this is because of Bug 436000 - XIM: winforms program crash randomly while starting XIM on Mono
                //see https://bugzilla.novell.com/show_bug.cgi?id=436000
                //https://bugzilla.xamarin.com/show_bug.cgi?id=28047
            }

            Thread.CurrentThread.CurrentCulture = Settings.Default.Language ?? new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = Settings.Default.Language ?? new CultureInfo("en");
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            LoadingScreen.ShowSplashScreen();

            GSLInitializer.Initialize();

            Application.SetCompatibleTextRenderingDefault(false);

            if (!RuntimeInformation.IsUnix)
                Application.AddMessageFilter(new MyMessageFilter());//TODO: find a way to redirect messages to underlying controls in Unix

            var mainForm = (new WinFormsBootstrapper()).Create<MainView>();

            LoadingScreen.CloseForm();
            Application.Run(mainForm);
        }


        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, Strings.Program_Application_ThreadException_Unhandled_Thread_Exception);
            
            Logger.Error(e.Exception, Strings.Program_Application_ThreadException_Unhandled_Thread_Exception+$" {e.Exception}");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message,
                Strings.Unhandled_UI_Exception);

            if (ex.Message.Contains("Font") || ex.Message.Contains("font"))
            {
                //e.IsTerminating = false;

                MessageBox.Show(Strings.Program_CurrentDomain_UnhandledException_Try_installing_font_);
                Process.Start(PathUtility.GetFullPath("Static", "fonts", "CAMBRIA.TTC"));
            }

            Logger.Error(ex, Strings.Unhandled_UI_Exception+ $"is terminting: {e.IsTerminating}, {e?.ExceptionObject}");
        }


        // DO NOT DELETE THIS CODE UNLESS WE NO LONGER REQUIRE ASSEMBLY A!!!
        private static void DummyFunctionToMakeSureReferencesGetCopiedProperly_DO_NOT_DELETE_THIS_CODE()
        {
            // Assembly A is used by this file, and that assembly depends on assembly B,
            // but this project does not have any code that explicitly references assembly B. Therefore, when another project references
            // this project, this project's assembly and the assembly A get copied to the project's bin directory, but not
            // assembly B. So in order to get the required assembly B copied over, we add some dummy code here (that never
            // gets called) that references assembly B; this will flag VS/MSBuild to copy the required assembly B over as well.
            var dummyType = typeof(System.Runtime.CompilerServices.AsyncTaskMethodBuilder); //System.Threading.Tasks.dll
            Console.WriteLine(dummyType.FullName);

            dummyType = typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute); //System.Runtime.dll
            Console.WriteLine(dummyType.FullName);
        }
    }
}