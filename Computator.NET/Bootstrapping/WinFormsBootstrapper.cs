using System.Collections.Generic;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.DataTypes;
using Computator.NET.Services;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Controls.AutocompleteMenu;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Dialogs;
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
    public class WinFormsBootstrapper : CoreBootstrapper
    {
        private FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper> _resolver;

        public WinFormsBootstrapper() : this(new UnityContainer())
        {
        }

        public WinFormsBootstrapper(IUnityContainer coreContainer) : base(coreContainer)
        {
            Container.RegisterType<IApplicationManager, ApplicationManager>(new ContainerControlledLifetimeManager());
            RegisterWinFormsServices();
            RegisterViews();
            RegisterControls();
        }

        private void RegisterWinFormsServices()
        {
            Container.RegisterType<IMessagingService, MessagingService>();
            Container.RegisterType<IShowFuncrionDetails, WebBrowserForm>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDialogFactory, WinFormsDialogFactory>(new ContainerControlledLifetimeManager());
        }

        public override T Create<T>()
        {
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

        private void RegisterControls()
        {
            //components and controls
            Container.RegisterInstance(
                new ReadOnlyDictionary<CalculationsMode, IChart>(new Dictionary<CalculationsMode, IChart>()
                {
                    { CalculationsMode.Real, new Chart2D()},
                    { CalculationsMode.Complex, new ComplexChart()},
                    { CalculationsMode.Fxy, new Chart3DControl()},
                }));


            //ExpressionTextBox
            Container.RegisterType<IExpressionTextBox, ExpressionTextBox>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITextProvider>(new InjectionFactory(c => Container.Resolve<ExpressionTextBox>()));
            Container.RegisterType<IOpenFileDialog, OpenFileDialogWrapper>();
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
    }
}