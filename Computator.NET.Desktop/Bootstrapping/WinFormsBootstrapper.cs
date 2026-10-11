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
using Unity.Lifetime;

namespace Computator.NET.Desktop.Bootstrapping
{
    public class WinFormsBootstrapper : CoreBootstrapper
    {
        private FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper> _resolver;

        public WinFormsBootstrapper() : this(new UnityContainer()) { }

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
            if (typeof(T) == typeof(MainView)) CreatePresenters();
            return base.Create<T>();
        }

        private void CreatePresenters()
        {
            Container.Resolve<MainFormPresenter>(_resolver.Overrides);
            Container.Resolve<ExpressionViewPresenter>(_resolver.Overrides);
            Container.Resolve<ChartingViewPresenter>(_resolver.Overrides);
            Container.Resolve<ChartAreaValuesPresenter>(_resolver.Overrides);
            Container.Resolve<CalculationsPresenter>(_resolver.Overrides);
            Container.Resolve<NumericalCalculationsPresenter>(_resolver.Overrides);
            Container.Resolve<ScriptingViewPresenter>(_resolver.Overrides);
            Container.Resolve<CustomFunctionsPresenter>(_resolver.Overrides);
        }

        private void RegisterViews()
        {
            Container.RegisterTypeLegacy<IMainView, MainView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IExpressionView, ExpressionView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IChartingView, ChartingView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IChartAreaValuesView, ChartAreaValuesView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<ICalculationsView, CalculationsView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<INumericalCalculationsView, NumericalCalculationsView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IScriptingView, ScriptingView>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<ISolutionExplorerView, SolutionExplorerView>();
            Container.RegisterTypeLegacy<ICustomFunctionsView, CustomFunctionsView>(new ContainerControlledLifetimeManager());
        }

        private void RegisterControls()
        {
            Container.RegisterTypeLegacy<IChart2D, Chart2D>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IComplexChart, ComplexChart>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IChart3D, Chart3DControl>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IExpressionTextBox, ExpressionTextBox>(new ContainerControlledLifetimeManager());
            Container.RegisterFactory<ITextProvider>(c => c.Resolve<ExpressionTextBox>());
            Container.RegisterTypeLegacy<IOpenFileDialog, OpenFileDialogWrapper>();
            Container.RegisterTypeLegacy<IClickedMouseButtonsProvider, MouseButtonsProvider>();
            Container.RegisterType<CodeEditorControlWrapper>("scripting");
            Container.RegisterType<CodeEditorControlWrapper>("customFunctions");
            _resolver = new FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper>(
                new Dictionary<string, CodeEditorControlWrapper>
                {
                    { "script", Container.Resolve<CodeEditorControlWrapper>("scripting") },
                    { "customFunction", Container.Resolve<CodeEditorControlWrapper>("customFunctions") }
                });
            Container.RegisterFactory<ICodeEditorView>(c => c.Resolve<CodeEditorControlWrapper>(_resolver.Overrides));
            Container.RegisterFactory<ICodeDocumentsEditor>(c => c.Resolve<CodeEditorControlWrapper>(_resolver.Overrides));
            Container.RegisterFactory<IDocumentsEditor>(c => c.Resolve<CodeEditorControlWrapper>(_resolver.Overrides));
            Container.RegisterFactory<ICanFileEdit>(c => c.Resolve<CodeEditorControlWrapper>(_resolver.Overrides));
            Container.RegisterFactory<ICanOpenFiles>(c => c.Resolve<CodeEditorControlWrapper>(_resolver.Overrides));
            Container.RegisterFactory<IScriptProvider>(c => c.Resolve<CodeEditorControlWrapper>(_resolver.Overrides));
            Container.RegisterFactory<ISupportsExceptionHighliting>(c => c.Resolve<CodeEditorControlWrapper>(_resolver.Overrides));
        }
    }
}
