using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Menu.Commands.TopMenuCommands;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Views;
using Settings = Computator.NET.Core.Properties.Settings;

internal static partial class Program
{
    private static void SymbolicDesktopWorkflow()
    {
        GSLInitializer.Initialize();
        var previousEditor = Settings.Default.CodeEditor;
        var previousScripts = Settings.Default.ScriptingDirectory;
        var previousFunctions = Settings.Default.CustomFunctionsDirectory;
        var directory = Path.Combine(outputDirectory, "symbolic-documents");
        Directory.CreateDirectory(directory);
        Settings.Default.CodeEditor = CodeEditorType.TextEditor;
        Settings.Default.ScriptingDirectory = directory;
        Settings.Default.CustomFunctionsDirectory = directory;
        var errors = new RecordingErrors();
        var bootstrapper = new WinFormsBootstrapper();
        try
        {
            using (bootstrapper.Container)
            {
                bootstrapper.RegisterInstance<IErrorHandler>(errors);
                using (var window = bootstrapper.Create<MainView>())
                {
                    window.Show();
                    var state = bootstrapper.Create<ISharedViewState>();
                    state.CurrentView = ViewName.SymbolicCalculations;
                    Application.DoEvents();
                    var view = bootstrapper.Create<SymbolicCalculationsView>();
                    var expression = bootstrapper.Create<IExpressionTextBox>();
                    Same(view.FindForm(), window, "Symbolic presenter must use the displayed tab.");
                    Same(bootstrapper.Create<ISymbolicCalculationsView>(), view, "Symbolic interface must resolve the displayed control.");
                    Check(view.Enabled && view.Visible, "Symbolic tab is disabled or hidden.");
                    Equal(window.ModeText, "Mode[Symbolic]");

                    view.SelectOperation("simplify");
                    expression.Text = "sin(x)^2 + cos(x)^2";
                    // A real native pointer click proves the button's event wiring,
                    // rather than invoking the presenter's method directly.
                    var button = (Button)Find(view, "symbolicCalculate");
                    var clicks = 0;
                    EventHandler observe = (sender, args) => clicks++;
                    button.Click += observe;
                    try { NativeX11Input.ClickOnce(window.Handle, button.Handle, () => clicks, Application.DoEvents); }
                    finally { button.Click -= observe; }
                    WaitForSymbolicCompletion(view);
                    Equal(Find(view, "symbolicResult").Text, "1");

                    view.SelectOperation("definite-integrate");
                    expression.Text = "x²";
                    Find(view, "symbolicLowerBound").Text = "0";
                    Find(view, "symbolicUpperBound").Text = "3";
                    bootstrapper.Create<RunCommand>().Execute();
                    WaitForSymbolicCompletion(view);
                    Equal(Find(view, "symbolicResult").Text, "9");

                    foreach (var operation in new[] { "expand", "factor", "differentiate", "integrate", "limit",
                        "solve", "substitute", "evaluate", "taylor", "solve-system", "determinant", "inverse", "transpose", "trace" })
                    {
                        view.SelectOperation(operation);
                        ((Button)Find(view, "symbolicExample")).PerformClick();
                        bootstrapper.Create<RunCommand>().Execute();
                        WaitForSymbolicCompletion(view);
                        Check(Find(view, "symbolicResult").Text.Length > 0, operation + " did not produce an actual symbolic result.");
                        Check(Find(view, "symbolicLatex").Text.Length > 0, operation + " did not produce LaTeX.");
                        Check(!Find(view, "symbolicStatus").Text.Contains("Could not calculate"), operation + " failed.");
                    }

                    view.SelectOperation("simplify");
                    expression.Text = "sin(";
                    bootstrapper.Create<RunCommand>().Execute();
                    WaitForSymbolicCompletion(view);
                    Check(Find(view, "symbolicStatus").Text.Contains("Could not calculate"), "Malformed input must show a useful diagnostic.");
                    Equal(Find(view, "symbolicResult").Text, "");
                    expression.Text = "6 * 7";
                    bootstrapper.Create<RunCommand>().Execute();
                    WaitForSymbolicCompletion(view);
                    Equal(Find(view, "symbolicResult").Text, "42");
                    errors.AssertEmpty();
                    Capture(window, "symbolic-calculations.png");
                    window.Close();
                    Application.DoEvents();
                    Check(window.IsDisposed, "Symbolic application workflow must shut down normally.");
                    Check(!state.DefaultActions.ContainsKey(ViewName.SymbolicCalculations), "Disposed symbolic view retained a toolbar action.");
                }
            }
        }
        finally
        {
            Settings.Default.CodeEditor = previousEditor;
            Settings.Default.ScriptingDirectory = previousScripts;
            Settings.Default.CustomFunctionsDirectory = previousFunctions;
        }
    }

    private static void WaitForSymbolicCompletion(SymbolicCalculationsView view)
    {
        var watch = Stopwatch.StartNew();
        while (Find(view, "symbolicCancel").Enabled && watch.ElapsedMilliseconds < 20000)
        { Application.DoEvents(); Thread.Sleep(5); }
        Application.DoEvents();
        Check(!Find(view, "symbolicCancel").Enabled && Find(view, "symbolicCalculate").Enabled,
            "The asynchronous symbolic workflow did not complete through the native dispatcher.");
    }
}
