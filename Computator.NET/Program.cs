using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Logging;
using Computator.NET.Natives;
using Computator.NET.Properties;
using Computator.NET.UI;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Dialogs;
using Computator.NET.UI.ErrorHandling;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Presenters;
using Computator.NET.UI.Views;
using Microsoft.Practices.Unity;

namespace Computator.NET
{
    internal static class Program
    {
        private static readonly SimpleLogger.SimpleLogger logger = new SimpleLogger.SimpleLogger(GlobalConfig.AppName) {ClassName = "Program"};



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

            var mainForm = BootStrapper().Resolve<MainForm>();

            LoadingScreen.CloseForm();
            Application.Run(mainForm);
        }




        private static UnityContainer BootStrapper()
        {
            var container = new UnityContainer();

            container.RegisterType<IMainForm, MainForm>(new ContainerControlledLifetimeManager());
            container.RegisterType<IExpressionView, ExpressionView>(new ContainerControlledLifetimeManager());

            container.RegisterType<IExpressionTextBox, ExpressionTextBox>(new ContainerControlledLifetimeManager());//check

            container.RegisterType<ITextProvider, IExpressionTextBox>(new ContainerControlledLifetimeManager());//check

            container.RegisterType<IMenuStripView, MenuStripView>(new ContainerControlledLifetimeManager());
            container.RegisterType<IToolbarView, ToolBarView>(new ContainerControlledLifetimeManager());

            container.RegisterType<IChartingView, ChartingView>(new ContainerControlledLifetimeManager());
            container.RegisterType<IChartAreaValuesView, ChartAreaValuesView>(new ContainerControlledLifetimeManager());

            container.RegisterType<ICalculationsView, CalculationsView>(new ContainerControlledLifetimeManager());

            container.RegisterType<INumericalCalculationsView, NumericalCalculationsView>(new ContainerControlledLifetimeManager());

            container.RegisterType<IScriptingView, ScriptingView>(new ContainerControlledLifetimeManager());
            container.RegisterType<ISolutionExplorerView, SolutionExplorerView>(new ContainerControlledLifetimeManager());

            container.RegisterType<ICustomFunctionsView, CustomFunctionsView>(new ContainerControlledLifetimeManager());

            container.RegisterType<ISharedViewState, SharedViewState>(new ContainerControlledLifetimeManager());
            container.RegisterType<IFunctionsDetails, FunctionsDetails>(new ContainerControlledLifetimeManager());

            container.RegisterType<IErrorHandler, IErrorHandler>(new ContainerControlledLifetimeManager());
            container.RegisterType<IExceptionsHandler, ExceptionsHandler>(new ContainerControlledLifetimeManager());
            

            var mainFormPresenter = container.Resolve<MainFormPresenter>();
            var chartingViewPresenter = container.Resolve<ChartingViewPresenter>();
            var calculationsViewPresenter = container.Resolve<CalculationsPresenter>();
            var numericalCalculationsPresenter = container.Resolve<NumericalCalculationsPresenter>();
            var scriptingViewPresenter = container.Resolve<ScriptingViewPresenter>();
            var customFunctionsViewPresenter = container.Resolve<CustomFunctionsPresenter>();

            return container;
        }


        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, Strings.Program_Application_ThreadException_Unhandled_Thread_Exception);

            logger.MethodName = MethodBase.GetCurrentMethod().Name;
            logger.Log(Strings.Program_Application_ThreadException_Unhandled_Thread_Exception, ErrorType.General,
                e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message,
                Strings.Program_CurrentDomain_UnhandledException_Unhandled_UI_Exception);

            if (ex.Message.Contains("Font") || ex.Message.Contains("font"))
            {
                //e.IsTerminating = false;

                MessageBox.Show(Strings.Program_CurrentDomain_UnhandledException_Try_installing_font_);
                Process.Start(GlobalConfig.FullPath("UI", "fonts", "CAMBRIA.TTC"));
            }


            logger.MethodName = MethodBase.GetCurrentMethod().Name;
            logger.Log(Strings.Program_CurrentDomain_UnhandledException_Unhandled_UI_Exception, ErrorType.General, ex);
        }


        // DO NOT DELETE THIS CODE UNLESS WE NO LONGER REQUIRE ASSEMBLY A!!!
        private static void DummyFunctionToMakeSureReferencesGetCopiedProperly_DO_NOT_DELETE_THIS_CODE()
        {
            // Assembly A is used by this file, and that assembly depends on assembly B,
            // but this project does not have any code that explicitly references assembly B. Therefore, when another project references
            // this project, this project's assembly and the assembly A get copied to the project's bin directory, but not
            // assembly B. So in order to get the required assembly B copied over, we add some dummy code here (that never
            // gets called) that references assembly B; this will flag VS/MSBuild to copy the required assembly B over as well.
            var dummyType = typeof(System.Runtime.CompilerServices.AsyncTaskMethodBuilder);//System.Threading.Tasks.dll
            Console.WriteLine(dummyType.FullName);

            dummyType = typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute);//System.Runtime.dll

            Console.WriteLine(dummyType.FullName);
        }
    }
}