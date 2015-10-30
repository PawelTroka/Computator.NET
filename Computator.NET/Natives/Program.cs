namespace Computator.NET
{
    internal static class Program
    {
        private static readonly Logging.SimpleLogger logger = new Logging.SimpleLogger {ClassName = "Program"};
        private static gsl_error_handler_t UnmanagedHandler;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [System.STAThread]
        private static void Main()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = Properties.Settings.Default.Language;
            System.Threading.Thread.CurrentThread.CurrentUICulture = Properties.Settings.Default.Language;

            System.Windows.Forms.Application.EnableVisualStyles();
            UI.Forms.LoadingScreen.ShowSplashScreen();
            //MultiplatformDllLoader.Enable = true;
            // var dllDir = GlobalConfig.fullPath("Special", (Environment.Is64BitProcess) ? "x64" : "x86");
            // MessageBox.Show(dllDir);
            //SetDllDirectory(dllDir);

            // LoadLibrary(GlobalConfig.FullPath("Special", (Environment.Is64BitProcess) ? "x64" : "x86",
            //     GlobalConfig.gslCblasDllName));

            //  LoadLibrary(GlobalConfig.FullPath("Special", (Environment.Is64BitProcess) ? "x64" : "x86",
            //    GlobalConfig.gslDllName));

            Config.EmbeddedDllClass.ExtractEmbeddedDlls(Config.GlobalConfig.gslDllName,
                (System.Environment.Is64BitProcess) ? Properties.Resources.gsl_x64 : Properties.Resources.gsl_x86);
            Config.EmbeddedDllClass.ExtractEmbeddedDlls(Config.GlobalConfig.gslCblasDllName,
                (System.Environment.Is64BitProcess) ? Properties.Resources.cblas_x64 : Properties.Resources.cblas_x86);

            // NativeMethods.gsl_set_error_handler_off();
            UnmanagedHandler = HandleUnmanagedException;
            NativeMethods.gsl_set_error_handler(UnmanagedHandler);

            System.Threading.Thread.CurrentThread.CurrentCulture = Properties.Settings.Default.Language;
            System.Threading.Thread.CurrentThread.CurrentUICulture = Properties.Settings.Default.Language;

            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.AddMessageFilter(new Config.MyMessageFilter());

            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var mainForm = new GUI();

            UI.Forms.LoadingScreen.CloseForm();
            System.Windows.Forms.Application.Run(mainForm);
        }

        private static void HandleUnmanagedException(string reason,
            string file, int line, int gsl_errno)
        {
            throw new System.Exception("Exception occcured in " + file + "\n at line " + line + "\nReason: " + reason +
                                       "\nError code: " + gsl_errno);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception");

            logger.MethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            logger.Log("Unhandled Thread Exception", Config.ErrorType.General, e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show((e.ExceptionObject as System.Exception).Message,
                "Unhandled UI Exception");

            logger.MethodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            logger.Log("Unhandled UI Exception", Config.ErrorType.General, (e.ExceptionObject as System.Exception));
        }
    }
}