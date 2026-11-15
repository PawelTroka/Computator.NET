using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Services;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using Unity;
using Settings = Computator.NET.Core.Properties.Settings;
using CompletionMenu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;

namespace Computator.NET.Validation
{
    // The Windows fixture and Mono acceptance use the same actual desktop composition.
    // Only the synthetic-key boundary is recorded; native editor Undo/Redo still run.
    internal static class EditorFocusContract
    {
        private sealed class RecordingApplicationManager : IApplicationManager
        {
            internal readonly List<string> Keys = new List<string>();
            public void SendStringAsKey(string key) => Keys.Add(key);
            public void Restart() => throw new InvalidOperationException("Unexpected restart.");
        }

        internal static void Run(ViewName view)
        {
            var previousSubscriptions = GlobalSubscriptions().ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
            var previousEditor = Settings.Default.CodeEditor;
            var previousScripts = Settings.Default.ScriptingDirectory;
            var previousFunctions = Settings.Default.CustomFunctionsDirectory;
            var directory = Path.Combine(Path.GetTempPath(), "Computator-EditorFocus-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            Settings.Default.CodeEditor = CodeEditorType.TextEditor;
            Settings.Default.ScriptingDirectory = Settings.Default.CustomFunctionsDirectory = directory;
            var application = new RecordingApplicationManager();
            WinFormsBootstrapper services = null;
            ExpressionTextBox expression = null;
            CompletionMenu expressionMenu = null;
            try
            {
                services = new WinFormsBootstrapper();
                services.Container.RegisterInstance<IApplicationManager>(application);
                using (services.Container)
                using (var window = services.Create<MainView>())
                {
                    expression = services.Create<ExpressionTextBox>();
                    expressionMenu = (CompletionMenu)typeof(ExpressionTextBox)
                        .GetField("_autocompleteMenu", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(expression);
                    window.StartPosition = FormStartPosition.Manual;
                    window.Location = new Point(20, 20);
                    window.Size = new Size(1100, 760);
                    window.Show();
                    window.Activate();
                    var state = services.Create<ISharedViewState>();
                    var scriptsView = services.Create<ScriptingView>();
                    var scripts = (CodeEditorControlWrapper)scriptsView.CodeEditorView;
                    var functions = (CodeEditorControlWrapper)services.Create<CustomFunctionsView>().CustomFunctionsEditor;
                    var editor = view == ViewName.Scripting ? scripts : functions;
                    var other = view == ViewName.Scripting ? functions : scripts;
                    var native = Descendants(editor).OfType<TextEditorCodeEditor>().Single();
                    var menu = window.Controls.OfType<MenuStripView>().Single().Controls.OfType<MenuStrip>().Single();
                    var items = menu.Items.OfType<ToolStripMenuItem>()
                        .SelectMany(item => item.DropDownItems.OfType<ToolStripMenuItem>()).ToArray();
                    var undo = items.Single(item => item.ShortcutKeyDisplayString == "Ctrl+Z");
                    var redo = items.Single(item => item.ShortcutKeyDisplayString == "Ctrl+Y");
                    state.CurrentView = view;
                    Application.DoEvents();

                    const string original = "write(1);";
                    const string first = " // first";
                    const string second = " second";
                    editor.Text = original;
                    other.Text = "write(2); // untouched";
                    native.Document.UndoStack.ClearAll();
                    native.Document.Insert(native.Document.TextLength, first);
                    native.Document.Insert(native.Document.TextLength, second);
                    Focus(native.ActiveTextAreaControl.TextArea);
                    Check(!native.Focused && native.ContainsFocus,
                        "The scenario must focus the real nested TextArea, not its editor container.");

                    undo.PerformClick();
                    Check(editor.Text == original + first && application.Keys.Count == 0,
                        "Edit > Undo did not reach the focused TextArea's document exactly once. Synthetic keys: " +
                        string.Join(", ", application.Keys));
                    Check(editor.Focused && !other.Focused, "Only the editor containing native keyboard focus should report focus.");
                    redo.PerformClick();
                    Check(editor.Text == original + first + second && application.Keys.Count == 0,
                        "Edit > Redo must reach the focused document without sending a second keyboard action.");

                    // A read-only document still owns keyboard focus, but must not be edited.
                    native.Document.ReadOnly = true;
                    undo.PerformClick();
                    Check(editor.Text == original + first + second && application.Keys.Count == 0,
                        "Read-only Undo must remain with the native editor and preserve its text.");
                    native.Document.ReadOnly = false;

                    // The wrapper also contains tabs. Their focus must not count as editor focus.
                    Focus(Descendants(editor).OfType<DocumentsTabControl>().Single());
                    Check(editor.ContainsFocus && !editor.Focused, "Document-tab focus must not count as text-editor focus.");
                    undo.PerformClick();
                    Check(application.Keys.SequenceEqual(new[] { "^Z" }) && editor.Text == original + first + second,
                        "Undo outside the text editor must retain the existing keyboard fallback.");

                    Focus(native.ActiveTextAreaControl.TextArea);
                    undo.PerformClick();
                    Check(editor.Text == original + first && application.Keys.Count == 1,
                        "Returning focus to the TextArea must restore direct command routing.");

                    // The real output control is a sibling of the scripting editor.
                    state.CurrentView = ViewName.Scripting;
                    Application.DoEvents();
                    Focus(Descendants(scriptsView).OfType<RichTextBox>().Single());
                    Check(!scripts.Focused && !functions.Focused, "Console focus must not be attributed to either editor.");
                    undo.PerformClick();
                    Check(application.Keys.SequenceEqual(new[] { "^Z", "^Z" }),
                        "Console commands must keep the existing keyboard fallback.");
                    Check(editor.Text == original + first && other.Text == "write(2); // untouched",
                        "Focus loss must preserve both editor buffers.");

                    state.CurrentView = view == ViewName.Scripting ? ViewName.CustomFunctions : ViewName.Scripting;
                    Application.DoEvents();
                    var otherNative = Descendants(other).OfType<TextEditorCodeEditor>().Single();
                    otherNative.Document.UndoStack.ClearAll();
                    otherNative.Document.Insert(otherNative.Document.TextLength, " changed");
                    Focus(otherNative.ActiveTextAreaControl.TextArea);
                    Check(other.Focused && !editor.Focused, "The newly focused workspace must own the edit command.");
                    undo.PerformClick();
                    Check(other.Text == "write(2); // untouched" && editor.Text == original + first && application.Keys.Count == 2,
                        "Switching workspaces must route Undo only to the newly focused document.");
                }
            }
            finally
            {
                try
                {
                    // The baseline expression popup is not owned by the control's
                    // Dispose path. Release only the popup created by this fixture.
                    if (expressionMenu != null)
                    {
                        expressionMenu.Close();
                        expressionMenu.SetAutocompleteMenu(expression, null);
                        expressionMenu.Dispose();
                    }
                }
                finally
                {
                    // MainView's presenters retain process-wide subscriptions.
                    // Preserve earlier acceptance state and remove only ours.
                    foreach (var entry in GlobalSubscriptions().ToArray())
                    {
                        object[] previous;
                        if (!previousSubscriptions.TryGetValue(entry.Key, out previous)) previous = new object[0];
                        var unsubscribe = typeof(IEventAggregator).GetMethod("Unsubscribe").MakeGenericMethod(entry.Key);
                        foreach (var subscriber in entry.Value.ToArray())
                            if (!previous.Any(item => ReferenceEquals(item, subscriber)))
                                unsubscribe.Invoke(EventAggregator.Instance, new[] { subscriber });
                    }
                    Settings.Default.CodeEditor = previousEditor;
                    Settings.Default.ScriptingDirectory = previousScripts;
                    Settings.Default.CustomFunctionsDirectory = previousFunctions;
                    Directory.Delete(directory, true);
                }
            }
        }

        private static IDictionary<Type, List<object>> GlobalSubscriptions()
            => (IDictionary<Type, List<object>>)typeof(EventAggregator)
                .GetField("subscriptions", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(EventAggregator.Instance);

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }

        private static void Focus(Control control)
        {
            control.Focus();
            var timeout = Stopwatch.StartNew();
            do
            {
                Application.DoEvents();
                if (control.Focused) return;
                Thread.Sleep(10);
            } while (timeout.ElapsedMilliseconds < 2000);
            throw new InvalidOperationException("Could not establish native focus on " + control.GetType().Name + ".");
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
