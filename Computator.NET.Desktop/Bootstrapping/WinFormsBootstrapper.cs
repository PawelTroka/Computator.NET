using System.Collections.Generic;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Menu.Commands;
using Computator.NET.Core.Menu.Commands.FileCommands;
using Computator.NET.Core.Presenters;
using Computator.NET.DataTypes.Charts;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.AutocompleteMenu;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Dialogs;
using Computator.NET.Desktop.Services;
using Computator.NET.Desktop.Views;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace Computator.NET.Desktop.Bootstrapping
{
    public class WinFormsBootstrapper : CoreBootstrapper
    {
        private FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper> _resolver;

        public WinFormsBootstrapper() : this(new UnityContainer())
        {
        }

        public WinFormsBootstrapper(IUnityContainer coreContainer) : base(coreContainer)
        {
            Container.RegisterTypeLegacy<IApplicationManager, ApplicationManager>(new ContainerControlledLifetimeManager());
            RegisterWinFormsServices();
            RegisterViews();
            RegisterControls();
        }

        private void RegisterWinFormsServices()
        {
            Container.RegisterTypeLegacy<IMessagingService, MessagingService>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IShowFunctionDetails, WebBrowserForm>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IDialogFactory, WinFormsDialogFactory>(new ContainerControlledLifetimeManager());
        }

        public override T Create<T>()
        {
            if (typeof(T) == typeof(MainView))
                CreatePresenters();
            return base.Create<T>();
        }

        private void CreatePresenters()
        {
            //presenters
            var mainFormPresenter = Container.Resolve<MainFormPresenter>(_resolver);
            var expressionViewPresenter = Container.Resolve<ExpressionViewPresenter>(_resolver);
            var chartingViewPresenter = Container.Resolve<ChartingViewPresenter>(_resolver);
            var chartAreaValuesViewPresenter = Container.Resolve<ChartAreaValuesPresenter>(_resolver);
            var calculationsViewPresenter = Container.Resolve<CalculationsPresenter>(_resolver);
            var numericalCalculationsPresenter = Container.Resolve<NumericalCalculationsPresenter>(_resolver);
            var scriptingViewPresenter = Container.Resolve<ScriptingViewPresenter>(_resolver);
            var customFunctionsViewPresenter = Container.Resolve<CustomFunctionsPresenter>(_resolver);
        }


        private void RegisterViews()
        {
            //views
            Container.RegisterTypeLegacy<IMainView, MainView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IExpressionView, ExpressionView>(new ContainerControlledLifetimeManager());

            // container.RegisterTypeLegacy<IMenuStripView, MenuStripView>(new ContainerControlledLifetimeManager());
            // container.RegisterTypeLegacy<IToolbarView, ToolBarView>(new ContainerControlledLifetimeManager());

            Container.RegisterTypeLegacy<IChartingView, ChartingView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IChartAreaValuesView, ChartAreaValuesView>(new ContainerControlledLifetimeManager());

            Container.RegisterTypeLegacy<ICalculationsView, CalculationsView>(new ContainerControlledLifetimeManager());

            Container.RegisterTypeLegacy<INumericalCalculationsView, NumericalCalculationsView>(
                new ContainerControlledLifetimeManager());

            Container.RegisterTypeLegacy<IScriptingView, ScriptingView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<ISolutionExplorerView, SolutionExplorerView>();

            Container.RegisterTypeLegacy<ICustomFunctionsView, CustomFunctionsView>(new ContainerControlledLifetimeManager());
        }

        private void RegisterControls()
        {
            //components and controls
            //charts
            Container.RegisterTypeLegacy<IChart2D, Chart2D>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IComplexChart, ComplexChart>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IChart3D, Chart3DControl>(new ContainerControlledLifetimeManager());


            //ExpressionTextBox
            Container.RegisterTypeLegacy<IExpressionTextBox, ExpressionTextBox>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITextProvider>(new InjectionFactory(c => Container.Resolve<ExpressionTextBox>()));
            Container.RegisterTypeLegacy<IOpenFileDialog, OpenFileDialogWrapper>();
            Container.RegisterTypeLegacy<IClickedMouseButtonsProvider, MouseButtonsProvider>();

            //Scripting and CustomFunctions
            Container.RegisterType<CodeEditorControlWrapper>("scripting");
            Container.RegisterType<CodeEditorControlWrapper>("customFunctions");

            _resolver = new FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper>(new Dictionary<string, CodeEditorControlWrapper>()
            {
                {"script", Container.Resolve<CodeEditorControlWrapper>("scripting")},
                {"customFunction", Container.Resolve<CodeEditorControlWrapper>("customFunctions")},
            });

            //container.RegisterType<ICodeEditorView, CodeEditorControlWrapper>();//check
            Container.RegisterType<ICodeEditorView>(
                new InjectionFactory(c => c.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<ICodeDocumentsEditor>(
                new InjectionFactory(c => c.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<IDocumentsEditor>(
                new InjectionFactory(c => c.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<ICanFileEdit>(
                new InjectionFactory(c => c.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<ICanOpenFiles>(
                new InjectionFactory(c => c.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<IScriptProvider>(
                new InjectionFactory(c => c.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<ISupportsExceptionHighliting>(
                new InjectionFactory(c => c.Resolve<CodeEditorControlWrapper>(_resolver)));
        }
    }
}