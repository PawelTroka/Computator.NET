#define PREFER_NATIVE_METHODS_OVER_SENDKING_SHORTCUT_KEYS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Accord.Collections;
using Computator.NET.Charting;
using Computator.NET.Data;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Menus;
using Computator.NET.UI.Menus.Commands;
using Computator.NET.UI.Menus.Commands.DummyCommands;
using Computator.NET.UI.Menus.Commands.EditCommands;
using Computator.NET.UI.Menus.Commands.FileCommands;
using Computator.NET.UI.Menus.Commands.HelpCommands;
using HelpCommand = Computator.NET.UI.Menus.HelpCommand;

namespace Computator.NET.UI.Presenters
{
    public class MainFormPresenter
    {
        private readonly IMainForm _view;
        private ISharedViewState _sharedViewState;

        private CalculationsMode _calculationsMode;
        private bool _applicationNeedRestart;

        public MainFormPresenter(IMainForm view, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails)
        {
            _view = view;
            _sharedViewState = sharedViewState;
            _view.Load += (sender, args) => HandleCommandLine();
            _view.ToolbarView.SetCommands(new List<IToolbarCommand>
            {
                new NewCommand(_view.ScriptingView.CodeEditorView, _view.CustomFunctionsView.CustomFunctionsEditor,sharedViewState),
                new OpenCommand(_view.ScriptingView.CodeEditorView, _view.CustomFunctionsView.CustomFunctionsEditor,sharedViewState),
                new SaveCommand(_view.ScriptingView.CodeEditorView, _view.CustomFunctionsView.CustomFunctionsEditor,sharedViewState),
                new PrintCommand(_view.ScriptingView.CodeEditorView, _view.CustomFunctionsView.CustomFunctionsEditor,
                    _view,sharedViewState),
                null,
                new CutCommand(_view.ScriptingView.CodeEditorView, _view.CustomFunctionsView.CustomFunctionsEditor,
                    _view,sharedViewState),
                new CopyCommand(_view.ScriptingView.CodeEditorView, _view.CustomFunctionsView.CustomFunctionsEditor,
                    _view,sharedViewState),
                new PasteCommand(_view.ScriptingView.CodeEditorView, _view.CustomFunctionsView.CustomFunctionsEditor,
                    _view,sharedViewState),
                null,
                new HelpCommand(),
                null,
                new ExponentCommand(sharedViewState),
                null,
                new RunCommand(sharedViewState)
            });


            ICanFileEdit codeEditorView = _view.ScriptingView.CodeEditorView;
            ITextProvider codeEditorView1 = _view.ScriptingView.CodeEditorView;
            ReadOnlyDictionary<CalculationsMode, IChart> charts = _view.ChartingView.Charts;
            ITextProvider expressionTextBox = _view.ExpressionView.ExpressionTextBox;
            ITextProvider customFunctionsEditor1 = _view.CustomFunctionsView.CustomFunctionsEditor;
            _view.MenuStripView.SetCommands(new List<IToolbarCommand>
            {
                new FileCommand(_view, codeEditorView, codeEditorView,sharedViewState ),
                new EditCommand(_view, codeEditorView, _view.CustomFunctionsView.CustomFunctionsEditor, sharedViewState),
                new FunctionsCommand(expressionTextBox, codeEditorView1,
                    customFunctionsEditor1, sharedViewState, functionsDetails),
                new ConstantsCommand(expressionTextBox, codeEditorView1,
                    customFunctionsEditor1, sharedViewState, functionsDetails),
                new ChartCommand(charts,sharedViewState),
                new TransformCommand(sharedViewState, charts),
                new ToolsCommand(_view),
                new HelpCommand(),
            });

            //  _view.EnterClicked += (o, e) => _sharedViewState.CurrentAction?.Invoke(o, e);

            var expressionViewPresenter = new ExpressionViewPresenter(_view.ExpressionView,sharedViewState);


            _view.ModeForcedToReal += (sender, args) =>
            {
                //   SetMode(CalculationsMode.Real);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
            };
            _view.ModeForcedToComplex += (sender, args) =>
            {
                //  SetMode(CalculationsMode.Complex);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Complex));
            };
            _view.ModeForcedToFxy += (sender, args) =>
            {
                //  SetMode(CalculationsMode.Fxy);
                EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Fxy));
            };

            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(mode => SetMode(mode.CalculationsMode));


            Settings.Default.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Settings.Default.Language):
                        _applicationNeedRestart = !Equals(CultureInfo.CurrentCulture, Settings.Default.Language);
                        break;
                }
            };

            Settings.Default.SettingsSaving += (o, e) =>
            {
                if (_applicationNeedRestart)
                {
                    _applicationNeedRestart = false;
                    Task.Factory.StartNew(() => { Thread.Sleep(400); _view.Restart(); });
                }
            };

            ///////EventAggregator.Instance.Subscribe<ChangeViewEvent>(cv => { _view.SelectedViewIndex = (int) cv.View; });

            _sharedViewState.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_sharedViewState.CurrentView):
                        _view.SelectedViewIndex = (int)_sharedViewState.CurrentView;
                        break;
                }
            };

            _view.SelectedViewChanged += _view_SelectedViewChanged;

            _view.StatusText = GlobalConfig.version;
        }

        private void _view_SelectedViewChanged(object sender, EventArgs e)
        {
            _sharedViewState.CurrentView = (ViewName)_view.SelectedViewIndex;
        }


        private void HandleCommandLine()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 2) return;
            if (!args[1].Contains(".tsl")) return;

            if (args[1].Contains(".tslf"))
            {
                _view.CustomFunctionsView.CustomFunctionsEditor.NewDocument(args[1]);
                _view.CustomFunctionsView.CustomFunctionsEditor.CurrentFileName = args[1];
                _sharedViewState.CurrentView = ViewName.CustomFunctions;
            }
            else
            {
                _view.ScriptingView.CodeEditorView.NewDocument(args[1]);
                _view.ScriptingView.CodeEditorView.CurrentFileName = args[1];
                _sharedViewState.CurrentView = ViewName.Scripting;
            }
        }

        private void SetMode(CalculationsMode mode)
        {
            if (mode == _calculationsMode)
                return;


            switch (mode)
            {
                case CalculationsMode.Complex:
                    _view.ModeText = "Mode[Complex : f(z)]";
                    break;
                case CalculationsMode.Fxy:
                    _view.ModeText = "Mode[3D : f(x,y)]";
                    break;
                case CalculationsMode.Real:
                    _view.ModeText = "Mode[Real : f(x)]";
                    break;
            }

            _calculationsMode = mode;
            //  _view.EditChartMenus.SetMode(_calculationsMode);
        }
    }
}