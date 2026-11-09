using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Computator.NET.Core.Model;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.DataTypes.Text;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using CompletionMenu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;
using CompletionList = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteListView;
using Settings = Computator.NET.Core.Properties.Settings;

internal static partial class Program
{
    // Resolve the displayed application views and their production completion
    // providers. A standalone TextBox or a fabricated item list cannot detect
    // an editor that has never been connected to scripting autocomplete.
    private static void ScriptingAutocompleteRealX11()
    {
        using (var fixture = new ScriptingCompletionFixture("insertion"))
        {
            const string prefix = "// α\n  ";
            const string suffix = ";\nwrite(7);";
            fixture.State.IsExponent = true;
            var path = fixture.Open("keyboard.tsl", prefix + suffix, prefix.Length);
            var document = fixture.Editor.Document;
            TypeX11("write");
            Equal(fixture.Scripts.Text, prefix + "write" + suffix);
            var menu = InstalledCompletion(fixture.Editor);
            WaitForCompletion(menu, "write(o)");
            Same(menu.TargetControlWrapper.TargetControl, fixture.TextArea,
                "Scripting completion must target the displayed ICSharpCode TextArea.");
            Check(menu.VisibleItems.All(item => item.IsScripting), "Scripting is using the expression completion catalog.");
            Check(menu.VisibleItems.Any(item => item.Text == "write(file,o)"), "Scripting overloads are missing.");
            Check(fixture.TextArea.ContainsFocus, "Opening scripting completion stole keyboard focus.");

            var selected = menu.SelectedItemIndex;
            var offset = fixture.Editor.ActiveTextAreaControl.Caret.Offset;
            KeyX11("Down");
            Check(menu.SelectedItemIndex == selected + 1, "Down did not select the next scripting suggestion.");
            KeyX11("Up");
            Check(menu.SelectedItemIndex == selected, "Up did not restore the scripting selection.");
            Equal(fixture.Scripts.Text, prefix + "write" + suffix);
            Check(fixture.Editor.ActiveTextAreaControl.Caret.Offset == offset,
                "Completion navigation also moved the document caret.");
            SelectCompletionX11(menu, "write(o)");
            KeyX11("Return");
            fixture.AssertInserted(prefix + "write(o)" + suffix, prefix.Length + "write(o)".Length);
            Same(fixture.Editor.Document, document, "Completion replaced the backing document.");
            Check(fixture.Tabs.SelectedTab.ImageIndex == 1, "Completion did not mark the script dirty.");
            AssertCompletionClosed(menu, "Enter left the scripting popup or reference visible.");
            // Baseline Mono does not dispatch Ctrl+Z to ICSharpCode's edit
            // actions. The real document command must undo completion once.
            fixture.Scripts.Undo();
            Equal(fixture.Scripts.Text, prefix + "write" + suffix);
            Same(fixture.Editor.Document, document, "Undo changed the document identity.");
            Equal(File.ReadAllText(path), prefix + suffix);

            fixture.Open("special.tsl", prefix + suffix, prefix.Length);
            TypeX11("Be");
            WaitForCompletion(menu, "BesselJ0(x)");
            var broadCount = menu.VisibleItems.Count;
            var broadList = (CompletionList)menu.ListView;
            Check(broadList.VerticalScroll.Visible, "The production special-function list does not expose scrolling.");
            var broadSelection = menu.SelectedItemIndex;
            KeyX11("Next");
            Check(menu.SelectedItemIndex > broadSelection && broadList.VerticalScroll.Value > 0,
                "PageDown did not advance and scroll the scripting suggestions.");
            KeyX11("Prior");
            Check(menu.SelectedItemIndex == broadSelection, "PageUp did not return to the original scripting suggestion.");
            Equal(fixture.Scripts.Text, prefix + "Be" + suffix);
            TypeX11("sselJ0");
            WaitForCompletion(menu, "BesselJ0(x)");
            Check(menu.VisibleItems.Count < broadCount, "Typing a longer special-function prefix did not filter suggestions.");
            SelectCompletionX11(menu, "BesselJ0(x)");
            var host = CompletionHost(menu);
            var help = CompletionHelp(menu);
            WaitForX11(() => help.Visible, "The actual scripting special-function reference did not open.");
            var reference = help.Items.OfType<ToolStripControlHost>().Single().Control;
            Check(reference.Text.Contains("BesselJ0"), "Scripting displayed another function's reference.");
            Check(Screen.FromControl(fixture.TextArea).WorkingArea.Contains(host.Bounds),
                "Scripting suggestions are clipped outside the screen.");
            CaptureAutocompleteX11(fixture.Window, host, help, "scripting-autocomplete-special.png");
            KeyX11("Tab");
            fixture.AssertInserted(prefix + "BesselJ0(x)" + suffix, prefix.Length + "BesselJ0(x)".Length);
            AssertCompletionClosed(menu, "Tab left the scripting popup or reference visible.");

            fixture.Open("mouse.tsl", prefix + suffix, prefix.Length);
            TypeX11("write");
            WaitForCompletion(menu, "write(o)");
            var index = menu.VisibleItems.ToList().FindIndex(item => item.Text == "write(o)");
            var list = (Control)menu.ListView;
            var row = menu.GetItemRectangle(index);
            Check(row.Top >= 0 && row.Bottom <= list.ClientSize.Height, "The mouse acceptance row is not visible.");
            var point = list.PointToScreen(new Point(row.Left + Math.Min(50, row.Width / 2), row.Top + row.Height / 2));
            MoveAndClickX11(point.X, point.Y);
            fixture.AssertInserted(prefix + "write(o)" + suffix, prefix.Length + "write(o)".Length);
            AssertCompletionClosed(menu, "Mouse acceptance left a scripting popup visible.");
            TypeX11("x");
            Equal(fixture.Scripts.Text, prefix + "write(o)x" + suffix);

            // Explicit completion must replace the selected word at its real
            // offset. A single-line fixture avoids the legacy selection
            // manager's separate stale-document multiline painting problem.
            const string selectedPrefix = "var result = ";
            const string selectedSuffix = "; // suffix";
            fixture.Open("selection.tsl", selectedPrefix + "write" + selectedSuffix,
                selectedPrefix.Length + "write".Length);
            // Establish the selection with its current document; native edit
            // shortcuts are a separate pre-existing Mono dispatch limitation.
            SelectSearchText(fixture.Editor, selectedPrefix.Length, "write".Length);
            AssertSearchSelection(fixture.Editor, selectedPrefix.Length, "write");
            KeyX11("ctrl+space");
            WaitForCompletion(menu, "write(o)");
            SelectCompletionX11(menu, "write(o)");
            KeyX11("Return");
            fixture.AssertInserted(selectedPrefix + "write(o)" + selectedSuffix,
                selectedPrefix.Length + "write(o)".Length);
            fixture.Errors.AssertEmpty();
        }
    }

    private static void ScriptingAutocompleteLifetimeX11()
    {
        using (var fixture = new ScriptingCompletionFixture("lifetime"))
        {
            var first = fixture.Open("first.tsl", string.Empty, 0);
            TypeX11("write");
            var menu = InstalledCompletion(fixture.Editor);
            WaitForCompletion(menu, "write(o)");
            var firstDocument = fixture.Editor.Document;
            var second = Path.Combine(fixture.Directory, "second.tsl");
            File.WriteAllText(second, "BesselJ");
            fixture.Scripts.NewDocument(second);
            AssertCompletionClosed(menu, "Opening another script retained completion for the previous buffer.");
            fixture.FocusScript("BesselJ".Length);
            KeyX11("ctrl+space");
            WaitForCompletion(menu, "BesselJ0(x)");
            SelectCompletionX11(menu, "BesselJ0(x)");
            KeyX11("Return");
            Equal(fixture.Scripts.Text, "BesselJ0(x)");
            fixture.Scripts.SwitchTab(first);
            Same(fixture.Editor.Document, firstDocument, "Switching tabs did not recover the original script document.");
            Equal(fixture.Scripts.Text, "write");
            fixture.FocusScript("write".Length);
            KeyX11("ctrl+space");
            WaitForCompletion(menu, "write(o)");
            KeyX11("Escape");
            AssertCompletionClosed(menu, "Escape did not dismiss scripting completion and help.");
            Equal(fixture.Scripts.Text, "write");

            // Give the real input event a pending display, then switch buffers
            // before its timer runs. It must not reopen over the next document.
            var originalInterval = menu.AppearInterval;
            menu.AppearInterval = 1000;
            fixture.Scripts.Text = string.Empty;
            fixture.FocusScript(0);
            TypeX11("wr");
            Check(!menu.Visible, "The pending-display scenario opened before the document switch.");
            KeyX11("Escape");
            PumpCompletionFor(1200);
            AssertCompletionClosed(menu, "Escape before the popup opened failed to cancel its pending display.");
            Equal(fixture.Scripts.Text, "wr");
            TypeX11("i");
            Check(!menu.Visible, "The second pending display opened before the document switch.");
            fixture.Scripts.SwitchTab(second);
            PumpCompletionFor(1200);
            AssertCompletionClosed(menu, "A pending completion reopened after switching documents.");
            Equal(fixture.Scripts.Text, "BesselJ0(x)");
            fixture.Scripts.SwitchTab(first);
            Equal(fixture.Scripts.Text, "wri");
            menu.AppearInterval = originalInterval;
            fixture.FocusScript(3);
            TypeX11("te");
            WaitForCompletion(menu, "write(o)");

            var console = Find(fixture.Window, "consoleOutputTextBox");
            var consolePoint = console.PointToScreen(new Point(10, 10));
            MoveAndClickX11(consolePoint.X, consolePoint.Y);
            Check(console.ContainsFocus, "The native click did not reach the actual scripting console.");
            WaitForX11(() => !menu.Visible && !CompletionHelp(menu).Visible,
                "Leaving the script for its console left completion floating over another control.");
            NativeX11Input.FocusOwnedWindow(fixture.Window.Handle, PumpX11);
            var editorPoint = fixture.TextArea.PointToScreen(new Point(70, 12));
            MoveAndClickX11(editorPoint.X, editorPoint.Y);
            Check(fixture.TextArea.ContainsFocus, "A real click did not return focus to the scripting TextArea.");
            fixture.Editor.ActiveTextAreaControl.Caret.Position =
                fixture.Editor.Document.OffsetToPosition(fixture.Scripts.Text.Length);
            TypeX11("ln");
            Equal(fixture.Scripts.Text, "writeln");
            WaitForCompletion(menu, "writeln(o)");
            KeyX11("Escape");

            fixture.Editor.Document.ReadOnly = true;
            try
            {
                KeyX11("ctrl+space");
                TypeX11("x");
                AssertCompletionClosed(menu, "A read-only script opened a completion popup.");
                Equal(fixture.Scripts.Text, "writeln");
            }
            finally { fixture.Editor.Document.ReadOnly = false; }
            KeyX11("ctrl+space");
            WaitForCompletion(menu, "writeln(o)");
            fixture.State.CurrentView = ViewName.Calculations;
            PumpX11();
            AssertCompletionClosed(menu, "Changing workspace left scripting completion visible.");
            fixture.State.CurrentView = ViewName.Scripting;
            fixture.FocusScript("writeln".Length);
            KeyX11("ctrl+space");
            WaitForCompletion(menu, "writeln(o)");
            KeyX11("Escape");

            var scrollText = "wr\n" + string.Concat(Enumerable.Repeat("write(0);\n", 70));
            fixture.Open("scroll.tsl", scrollText, 2);
            // The legacy control recalculates a swapped document's scrollbar
            // range on layout. Resize the real window before sending the wheel.
            fixture.Window.Size = new Size(fixture.Window.Width + 1, fixture.Window.Height + 1);
            PumpX11();
            var scrollbar = fixture.Editor.ActiveTextAreaControl.VScrollBar;
            Check(scrollbar.Maximum - scrollbar.LargeChange + 1 > 0,
                "The long scripting document has no scrollable viewport.");
            TypeX11("ite");
            WaitForCompletion(menu, "write(o)");
            var wheelMenu = CompletionHost(menu).Bounds;
            var wheelReference = CompletionHelp(menu).Bounds;
            var wheelPoints = new[]
            {
                new Point(6, fixture.TextArea.Height - 12),
                new Point(6, fixture.TextArea.Height / 2),
                new Point(fixture.TextArea.Width - 12, fixture.TextArea.Height - 12)
            }.Select(fixture.TextArea.PointToScreen)
                .Where(point => !wheelMenu.Contains(point) && !wheelReference.Contains(point)).ToArray();
            Check(wheelPoints.Length > 0, "No visible scripting-editor point is clear of the completion/reference windows.");
            var wheelPoint = wheelPoints[0];
            Xdotool("mousemove " + wheelPoint.X + " " + wheelPoint.Y + " click --repeat 4 --delay 30 5");
            PumpX11();
            Check(fixture.Editor.ActiveTextAreaControl.TextArea.VirtualTop.Y > 0,
                "The real mouse wheel did not scroll the scripting viewport at " + wheelPoint +
                "; editor=" + fixture.TextArea.RectangleToScreen(fixture.TextArea.ClientRectangle) +
                "; completion=" + wheelMenu + "; reference=" + wheelReference + ".");
            AssertCompletionClosed(menu, "Scrolling the scripting viewport left a popup anchored to the old caret position.");

            // Dispose while a native keystroke still has a pending display.
            fixture.Scripts.SwitchTab(first);
            fixture.FocusScript(fixture.Scripts.Text.Length);
            menu.AppearInterval = 1000;
            TypeX11("x");
            var host = CompletionHost(menu);
            var hostedList = (Control)menu.ListView;
            var help = CompletionHelp(menu);
            var reference = help.Items.OfType<ToolStripControlHost>().Single().Control;
            var textArea = fixture.TextArea;
            var custom = fixture.Bootstrapper.Create<CustomFunctionsView>();
            var hiddenEditor = SearchDescendants((Control)custom.CustomFunctionsEditor).OfType<TextEditorCodeEditor>().Single();
            var hiddenMenu = InstalledCompletion(hiddenEditor);
            var hiddenHost = CompletionHost(hiddenMenu);
            var hiddenList = (Control)hiddenMenu.ListView;
            var hiddenHelp = CompletionHelp(hiddenMenu);
            var hiddenReference = hiddenHelp.Items.OfType<ToolStripControlHost>().Single().Control;
            var hiddenArea = hiddenEditor.ActiveTextAreaControl.TextArea;
            Check(!hiddenEditor.Visible && !hiddenMenu.Visible,
                "The second completion owner must be the application's hidden custom-functions editor.");
            fixture.Window.Dispose();
            PumpCompletionFor(1200);
            Check(host.IsDisposed && hostedList.IsDisposed && help.IsDisposed && reference.IsDisposed,
                "Disposing scripting did not dispose its shown completion controls.");
            Check(hiddenHost.IsDisposed && hiddenList.IsDisposed && hiddenHelp.IsDisposed && hiddenReference.IsDisposed,
                "Disposing the hidden editor leaked its never-shown completion controls.");
            Check(menu.GetAutocompleteMenu(textArea) == null, "Disposing scripting retained a static editor/menu binding.");
            Check(hiddenMenu.GetAutocompleteMenu(hiddenArea) == null, "Disposing the hidden editor retained a static completion binding.");
            Equal(File.ReadAllText(first), string.Empty);
            Equal(File.ReadAllText(second), "BesselJ");
            fixture.Errors.AssertEmpty();
        }
    }

    private static void ExpressionAutocompleteIntegrationX11()
    {
        using (var fixture = new ScriptingCompletionFixture("expression"))
        {
            fixture.State.CurrentView = ViewName.Calculations;
            var expression = fixture.Bootstrapper.Create<ExpressionTextBox>();
            Same(expression.FindForm(), fixture.Window, "Expression regression must use the displayed expression editor.");
            var menu = InstalledCompletion(expression);
            fixture.State.IsExponent = false;
            expression.Text = string.Empty;
            NativeX11Input.FocusOwnedWindow(fixture.Window.Handle, PumpX11);
            expression.Focus();
            PumpX11();
            TypeX11("sin");
            WaitForCompletion(menu, "sin(x)");
            Check(menu.VisibleItems.All(item => !item.IsScripting), "Expression completion is using the scripting catalog.");
            SelectCompletionX11(menu, "sin(x)");
            KeyX11("Return");
            Equal(expression.Text, "sin(x)");
            Check(expression.ContainsFocus, "Expression acceptance lost keyboard focus.");
            AssertCompletionClosed(menu, "Expression acceptance left autocomplete open.");

            expression.Text = "x";
            expression.SelectionStart = 1;
            fixture.State.IsExponent = true;
            TypeX11("si");
            WaitForCompletion(menu, "sin(x)");
            SelectCompletionX11(menu, "sin(x)");
            KeyX11("Return");
            Equal(expression.Text, "x" + SpecialSymbols.AsciiToSuperscript("sin(x)"));
            AssertCompletionClosed(menu, "Exponent completion left autocomplete open.");
            fixture.Errors.AssertEmpty();
        }
    }

    private static CompletionMenu InstalledCompletion(Control editor)
    {
        var field = editor.GetType().GetField("_autocompleteMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        var menu = field == null ? null : field.GetValue(editor) as CompletionMenu;
        Check(menu != null, "The actual " + editor.GetType().Name +
            " has no autocomplete integration; native typing cannot show scripting suggestions (issue #324).");
        return menu;
    }

    private static ToolStripDropDown CompletionHost(CompletionMenu menu) => (ToolStripDropDown)typeof(CompletionMenu)
        .GetProperty("Host", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menu, null);

    private static ToolStripDropDown CompletionHelp(CompletionMenu menu) => (ToolStripDropDown)typeof(CompletionList)
        .GetField("toolTip", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menu.ListView);

    private static void WaitForCompletion(CompletionMenu menu, string signature)
    {
        WaitForX11(() => menu.Visible && menu.VisibleItems.Any(item => item.Text == signature),
            "Native typing did not display the production completion for " + signature + ".");
    }

    private static void SelectCompletionX11(CompletionMenu menu, string signature)
    {
        var target = menu.VisibleItems.ToList().FindIndex(item => item.Text == signature);
        Check(target >= 0, "Missing completion: " + signature);
        while (menu.SelectedItemIndex != target)
        {
            var previous = menu.SelectedItemIndex;
            KeyX11(previous < target ? "Down" : "Up");
            Check(menu.Visible && menu.SelectedItemIndex != previous,
                "Native keyboard navigation stopped before reaching " + signature + ".");
        }
    }

    private static void AssertCompletionClosed(CompletionMenu menu, string message)
        => Check(!menu.Visible && !CompletionHelp(menu).Visible, message);

    private static void PumpCompletionFor(int milliseconds)
    {
        var watch = Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < milliseconds) PumpX11();
    }

    private sealed class ScriptingCompletionFixture : IDisposable
    {
        private readonly Dictionary<Type, object[]> previousSubscriptions = GlobalSubscriptions()
            .ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
        private readonly CodeEditorType originalEditor = Settings.Default.CodeEditor;
        private readonly string originalScripts = Settings.Default.ScriptingDirectory;
        private readonly string originalFunctions = Settings.Default.CustomFunctionsDirectory;
        private readonly TooltipType originalTooltip = Settings.Default.TooltipType;
        private bool disposed;
        private ExpressionTextBox expression;
        private CompletionMenu expressionMenu;
        internal readonly string Directory;
        internal readonly RecordingErrors Errors = new RecordingErrors();
        internal WinFormsBootstrapper Bootstrapper;
        internal MainView Window;
        internal ISharedViewState State;
        internal CodeEditorControlWrapper Scripts;
        internal TextEditorCodeEditor Editor;
        internal DocumentsTabControl Tabs;
        internal Control TextArea => Editor.ActiveTextAreaControl.TextArea;

        internal ScriptingCompletionFixture(string scenario)
        {
            Directory = Path.Combine(outputDirectory, "scripting-completion-" + scenario);
            System.IO.Directory.CreateDirectory(Directory);
            Settings.Default.CodeEditor = CodeEditorType.TextEditor;
            Settings.Default.ScriptingDirectory = Settings.Default.CustomFunctionsDirectory = Directory;
            Settings.Default.TooltipType = TooltipType.Default;
            try
            {
                Bootstrapper = new WinFormsBootstrapper();
                Bootstrapper.RegisterInstance<IErrorHandler>(Errors);
                Window = Bootstrapper.Create<MainView>();
                expression = Bootstrapper.Create<ExpressionTextBox>();
                expressionMenu = InstalledCompletion(expression);
                Window.StartPosition = FormStartPosition.Manual;
                Window.Location = new Point(5, 5);
                Window.Size = new Size(1100, 760);
                Window.Show();
                State = Bootstrapper.Create<ISharedViewState>();
                State.CurrentView = ViewName.Scripting;
                var view = Bootstrapper.Create<ScriptingView>();
                Scripts = (CodeEditorControlWrapper)view.CodeEditorView;
                Editor = SearchDescendants(Scripts).OfType<TextEditorCodeEditor>().Single();
                Tabs = SearchDescendants(Scripts).OfType<DocumentsTabControl>().Single();
                Same(view.FindForm(), Window, "Acceptance must exercise MainView's real ScriptingView.");
                Check(Editor.Visible, "MainView did not display the Mono scripting editor.");
                PumpX11();
                FocusScript(0);
            }
            catch { Dispose(); throw; }
        }

        internal string Open(string name, string text, int caret)
        {
            var path = Path.Combine(Directory, name);
            File.WriteAllText(path, text);
            Scripts.NewDocument(path);
            FocusScript(caret);
            return path;
        }

        internal void FocusScript(int offset)
        {
            NativeX11Input.FocusOwnedWindow(Window.Handle, PumpX11);
            Editor.ActiveTextAreaControl.SelectionManager.ClearSelection();
            Editor.ActiveTextAreaControl.Caret.Position = Editor.Document.OffsetToPosition(offset);
            TextArea.Focus();
            PumpX11();
            Check(TextArea.ContainsFocus, "The actual scripting TextArea did not receive focus.");
        }

        internal void AssertInserted(string text, int caret)
        {
            Equal(Scripts.Text, text);
            Check(Editor.ActiveTextAreaControl.Caret.Offset == caret, "Completion left the caret at the wrong offset.");
            Check(!Editor.ActiveTextAreaControl.SelectionManager.HasSomethingSelected,
                "Completion left an unintended document selection.");
            Check(TextArea.ContainsFocus, "Completion did not return focus to the actual scripting TextArea.");
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            // Intentional test drafts must not enter a save-prompt modal loop.
            try
            {
                try
                {
                    // ExpressionTextBox's popup ownership is a separate
                    // baseline issue. Release the resource this fixture creates.
                    if (expressionMenu != null)
                    {
                        expressionMenu.Close();
                        expressionMenu.SetAutocompleteMenu(expression, null);
                        expressionMenu.Dispose();
                    }
                }
                finally
                {
                    try { if (Window != null) Window.Dispose(); }
                    finally { if (Bootstrapper != null) Bootstrapper.Container.Dispose(); }
                }
            }
            finally
            {
                // MainView presenters subscribe to the process-wide event bus.
                // Remove only this fixture's subscriptions, preserving earlier
                // acceptance state and preventing callbacks into disposed charts.
                foreach (var entry in GlobalSubscriptions().ToArray())
                {
                    object[] previous;
                    if (!previousSubscriptions.TryGetValue(entry.Key, out previous)) previous = new object[0];
                    var unsubscribe = typeof(IEventAggregator).GetMethod("Unsubscribe").MakeGenericMethod(entry.Key);
                    foreach (var subscriber in entry.Value.ToArray())
                        if (!previous.Any(item => ReferenceEquals(item, subscriber)))
                            unsubscribe.Invoke(EventAggregator.Instance, new[] { subscriber });
                }
                Settings.Default.CodeEditor = originalEditor;
                Settings.Default.ScriptingDirectory = originalScripts;
                Settings.Default.CustomFunctionsDirectory = originalFunctions;
                Settings.Default.TooltipType = originalTooltip;
            }
        }

        private static IDictionary<Type, List<object>> GlobalSubscriptions()
            => (IDictionary<Type, List<object>>)typeof(EventAggregator)
                .GetField("subscriptions", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(EventAggregator.Instance);
    }
}
