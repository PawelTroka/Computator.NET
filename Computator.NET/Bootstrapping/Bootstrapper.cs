using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Compilation;
using Computator.NET.Data;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.ErrorHandling;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.FileCommands;
using Computator.NET.UI.Models;
using Computator.NET.UI.Presenters;
using Computator.NET.UI.Views;
using Microsoft.Practices.Unity;

namespace Computator.NET
{
    class Bootstrapper
    {
        private readonly UnityContainer _container = new UnityContainer();

        private FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper> _resolver;
        public Bootstrapper()
        {
            RegisterViews();
            RegisterSharedObjects();
            RegisterHandlers();
            RegisterControls();
            RegisterModel();
            CreatePresenters();
        }
        public MainForm CreateMainForm()
        {
            return _container.Resolve<MainForm>(_resolver);
        }

        private void CreatePresenters()
        {
            //presenters
            var mainFormPresenter = _container.Resolve<MainFormPresenter>(_resolver);
            var expressionViewPresenter = _container.Resolve<ExpressionViewPresenter>(_resolver);
            var chartingViewPresenter = _container.Resolve<ChartingViewPresenter>(_resolver);
            var chartAreaValuesViewPresenter = _container.Resolve<ChartAreaValuesPresenter>(_resolver);
            var calculationsViewPresenter = _container.Resolve<CalculationsPresenter>(_resolver);
            var numericalCalculationsPresenter = _container.Resolve<NumericalCalculationsPresenter>(_resolver);
            var scriptingViewPresenter = _container.Resolve<ScriptingViewPresenter>(_resolver);
            var customFunctionsViewPresenter = _container.Resolve<CustomFunctionsPresenter>(_resolver);
        }

        private void RegisterModel()
        {
            //models and business objects
            _container.RegisterType<IModeDeterminer, ModeDeterminer>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ITslCompiler, TslCompiler>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IScriptEvaluator, ScriptEvaluator>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IExpressionsEvaluator, ExpressionsEvaluator>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IFunctionsDetails, FunctionsDetails>(new ContainerControlledLifetimeManager());
            _container.RegisterInstance(
                new ReadOnlyDictionary<CalculationsMode, IChart>(new Dictionary<CalculationsMode, IChart>()
                {
                    {CalculationsMode.Real, new Chart2D()},
                    {CalculationsMode.Complex, new ComplexChart()},
                    {CalculationsMode.Fxy, new Chart3DControl()},
                }));
        }

        private void RegisterControls()
        {
            //components and controls
            //ExpressionTextBox
            _container.RegisterType<IExpressionTextBox, ExpressionTextBox>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ITextProvider>(new InjectionFactory(c => _container.Resolve<ExpressionTextBox>()));
            _container.RegisterType<IOpenFileDialog,OpenFileDialogWrapper>();
            _container.RegisterType<IClickedMouseButtonsProvider, MouseButtonsProvider>();

            //Scripting and CustomFunctions
            _container.RegisterType<CodeEditorControlWrapper>("scripting");
            _container.RegisterType<CodeEditorControlWrapper>("customFunctions");

            _resolver = new FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper>(new Dictionary<string, CodeEditorControlWrapper>()
            {
                {"script", _container.Resolve<CodeEditorControlWrapper>("scripting")},
                {"customFunction", _container.Resolve<CodeEditorControlWrapper>("customFunctions")},
            });

            //container.RegisterType<ICodeEditorView, CodeEditorControlWrapper>();//check
            _container.RegisterType<ICodeEditorView>(
                new InjectionFactory(c => _container.Resolve<CodeEditorControlWrapper>(_resolver)));
            _container.RegisterType<ICodeDocumentsEditor>(
                new InjectionFactory(c => _container.Resolve<CodeEditorControlWrapper>(_resolver)));
            _container.RegisterType<IDocumentsEditor>(
                new InjectionFactory(c => _container.Resolve<CodeEditorControlWrapper>(_resolver)));
            _container.RegisterType<ICanFileEdit>(
                new InjectionFactory(c => _container.Resolve<CodeEditorControlWrapper>(_resolver)));
            _container.RegisterType<ICanOpenFiles>(
                new InjectionFactory(c => _container.Resolve<CodeEditorControlWrapper>(_resolver)));
            _container.RegisterType<IScriptProvider>(
                new InjectionFactory(c => _container.Resolve<CodeEditorControlWrapper>(_resolver)));
            _container.RegisterType<ISupportsExceptionHighliting>(
                new InjectionFactory(c => _container.Resolve<CodeEditorControlWrapper>(_resolver)));
        }

        private void RegisterHandlers()
        {
            //singleton handlers
            _container.RegisterType<IErrorHandler, SimpleErrorHandler>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IExceptionsHandler, ExceptionsHandler>(new ContainerControlledLifetimeManager());
        }

        private void RegisterSharedObjects()
        {
            //shared singletons
            _container.RegisterType<ISharedViewState, SharedViewState>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IFunctionsDetails, FunctionsDetails>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IApplicationManager, ApplicationManager>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ICommandLineHandler, CommandLineHandler>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            //views
            _container.RegisterType<IMainForm, MainForm>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IExpressionView, ExpressionView>(new ContainerControlledLifetimeManager());

            // container.RegisterType<IMenuStripView, MenuStripView>(new ContainerControlledLifetimeManager());
            //  container.RegisterType<IToolbarView, ToolBarView>(new ContainerControlledLifetimeManager());

            _container.RegisterType<IChartingView, ChartingView>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IChartAreaValuesView, ChartAreaValuesView>(new ContainerControlledLifetimeManager());

            _container.RegisterType<ICalculationsView, CalculationsView>(new ContainerControlledLifetimeManager());

            _container.RegisterType<INumericalCalculationsView, NumericalCalculationsView>(
                new ContainerControlledLifetimeManager());

            _container.RegisterType<IScriptingView, ScriptingView>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISolutionExplorerView, SolutionExplorerView>();

            _container.RegisterType<ICustomFunctionsView, CustomFunctionsView>(new ContainerControlledLifetimeManager());
        }
    }
}