using System;
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
        private bool _presentersCreated;
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
            if ((typeof(T) == typeof(MainView) || typeof(T) == typeof(IMainView)) && !_presentersCreated)
            {
                CreatePresenters();
                _presentersCreated = true;
            }
            return base.Create<T>();
        }
        private void CreatePresenters()
        {
            Container.ResolveComposed<MainFormPresenter>();
            Container.ResolveComposed<ExpressionViewPresenter>();
            Container.ResolveComposed<ChartingViewPresenter>();
            Container.ResolveComposed<ChartAreaValuesPresenter>();
            Container.ResolveComposed<CalculationsPresenter>();
            Container.ResolveComposed<NumericalCalculationsPresenter>();
            Container.ResolveComposed<ScriptingViewPresenter>();
            Container.ResolveComposed<CustomFunctionsPresenter>();
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
        private static T MissingEditorRole<T>() => throw new InvalidOperationException(
            "An editor consumer must name its constructor parameter scripting... or customFunctions.... " +
            "Refusing to create a disconnected hidden editor for " + typeof(T).Name + ".");

        private void RegisterControls()
        {
            Container.RegisterTypeLegacy<IChart2D, Chart2D>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IComplexChart, ComplexChart>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IChart3D, Chart3DControl>(new ContainerControlledLifetimeManager());
            Container.RegisterTypeLegacy<IExpressionTextBox, ExpressionTextBox>(new ContainerControlledLifetimeManager());
            Container.RegisterFactory<ITextProvider>(c => c.ResolveComposed<ExpressionTextBox>());
            Container.RegisterTypeLegacy<IOpenFileDialog, OpenFileDialogWrapper>();
            Container.RegisterTypeLegacy<IClickedMouseButtonsProvider, MouseButtonsProvider>();
            Container.RegisterType<CodeEditorControlWrapper>("scripting", new ContainerControlledLifetimeManager());
            Container.RegisterType<CodeEditorControlWrapper>("customFunctions", new ContainerControlledLifetimeManager());
            var resolver = new FuzzyMatchingParameterOverrideWithFallback<CodeEditorControlWrapper>(
                new Dictionary<string, CodeEditorControlWrapper>
                {
                    { "script", Container.Resolve<CodeEditorControlWrapper>("scripting") },
                    { "customFunction", Container.Resolve<CodeEditorControlWrapper>("customFunctions") }
                });
            Container.UseCompositionOverrides(resolver.Overrides);
            // Valid role-named parameters are matched before these fallbacks. A new
            // incorrectly wired consumer must fail visibly, not silently get a clone.
            Container.RegisterFactory<ICodeEditorView>(c => MissingEditorRole<ICodeEditorView>());
            Container.RegisterFactory<ICodeDocumentsEditor>(c => MissingEditorRole<ICodeDocumentsEditor>());
            Container.RegisterFactory<IDocumentsEditor>(c => MissingEditorRole<IDocumentsEditor>());
            Container.RegisterFactory<ICanFileEdit>(c => MissingEditorRole<ICanFileEdit>());
            Container.RegisterFactory<ICanOpenFiles>(c => MissingEditorRole<ICanOpenFiles>());
            Container.RegisterFactory<IScriptProvider>(c => MissingEditorRole<IScriptProvider>());
            Container.RegisterFactory<ISupportsExceptionHighliting>(c => MissingEditorRole<ISupportsExceptionHighliting>());
        }
    }
}
