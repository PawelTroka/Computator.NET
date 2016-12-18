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
    public class Bootstrapper
    {
        private readonly bool _registerUserInterfaceElements;
        public UnityContainer Container { get; } = new UnityContainer();

        private FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper> _resolver;
        public Bootstrapper(bool registerUserInterfaceElements=true)
        {
            _registerUserInterfaceElements = registerUserInterfaceElements;
            if(_registerUserInterfaceElements)
                RegisterViews();
            RegisterSharedObjects();
            RegisterHandlers();
            if(_registerUserInterfaceElements)
                RegisterControls();
            RegisterModel();
            if(_registerUserInterfaceElements)
                CreatePresenters();
        }
        public MainForm CreateMainForm()
        {
            return Container.Resolve<MainForm>(_resolver);
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

        private void RegisterModel()
        {
            //models and business objects
            Container.RegisterType<IModeDeterminer, ModeDeterminer>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITslCompiler, TslCompiler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScriptEvaluator, ScriptEvaluator>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IExpressionsEvaluator, ExpressionsEvaluator>(new ContainerControlledLifetimeManager());


            Container.RegisterType<IFunctionsDetails, FunctionsDetails>(new ContainerControlledLifetimeManager());

            if (!_registerUserInterfaceElements) return;
            Container.RegisterInstance(
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
            Container.RegisterType<IExpressionTextBox, ExpressionTextBox>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITextProvider>(new InjectionFactory(c => Container.Resolve<ExpressionTextBox>()));
            Container.RegisterType<IOpenFileDialog,OpenFileDialogWrapper>();
            Container.RegisterType<IClickedMouseButtonsProvider, MouseButtonsProvider>();

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
                new InjectionFactory(c => Container.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<ICodeDocumentsEditor>(
                new InjectionFactory(c => Container.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<IDocumentsEditor>(
                new InjectionFactory(c => Container.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<ICanFileEdit>(
                new InjectionFactory(c => Container.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<ICanOpenFiles>(
                new InjectionFactory(c => Container.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<IScriptProvider>(
                new InjectionFactory(c => Container.Resolve<CodeEditorControlWrapper>(_resolver)));
            Container.RegisterType<ISupportsExceptionHighliting>(
                new InjectionFactory(c => Container.Resolve<CodeEditorControlWrapper>(_resolver)));
        }

        private void RegisterHandlers()
        {
            //singleton handlers
            Container.RegisterType<IErrorHandler, SimpleErrorHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IExceptionsHandler, ExceptionsHandler>(new ContainerControlledLifetimeManager());
        }

        private void RegisterSharedObjects()
        {
            //shared singletons
            Container.RegisterType<ISharedViewState, SharedViewState>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFunctionsDetails, FunctionsDetails>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IApplicationManager, ApplicationManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICommandLineHandler, CommandLineHandler>(new ContainerControlledLifetimeManager());
        }

        private void RegisterViews()
        {
            //views
            Container.RegisterType<IMainForm, MainForm>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IExpressionView, ExpressionView>(new ContainerControlledLifetimeManager());

            // container.RegisterType<IMenuStripView, MenuStripView>(new ContainerControlledLifetimeManager());
            //  container.RegisterType<IToolbarView, ToolBarView>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IChartingView, ChartingView>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IChartAreaValuesView, ChartAreaValuesView>(new ContainerControlledLifetimeManager());

            Container.RegisterType<ICalculationsView, CalculationsView>(new ContainerControlledLifetimeManager());

            Container.RegisterType<INumericalCalculationsView, NumericalCalculationsView>(
                new ContainerControlledLifetimeManager());

            Container.RegisterType<IScriptingView, ScriptingView>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISolutionExplorerView, SolutionExplorerView>();

            Container.RegisterType<ICustomFunctionsView, CustomFunctionsView>(new ContainerControlledLifetimeManager());
        }
    }
}