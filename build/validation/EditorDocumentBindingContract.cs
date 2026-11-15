using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using Settings = Computator.NET.Core.Properties.Settings;
using CompletionMenu = Computator.NET.Desktop.Controls.AutocompleteMenu.AutocompleteMenu;

namespace Computator.NET.Validation
{
    // Exercise each real workspace, with the legacy editor's normal selection,
    // clipboard and undo operations. Do not substitute a document or selection.
    internal static class EditorDocumentBindingContract
    {
        internal static void Run(ViewName view)
        {
            var previousSubscriptions = GlobalSubscriptions().ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
            var previousEditor = Settings.Default.CodeEditor;
            var previousScripts = Settings.Default.ScriptingDirectory;
            var previousFunctions = Settings.Default.CustomFunctionsDirectory;
            var directory = Path.Combine(Path.GetTempPath(), "Computator-DocumentBinding-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            Settings.Default.CodeEditor = CodeEditorType.TextEditor;
            Settings.Default.ScriptingDirectory = Settings.Default.CustomFunctionsDirectory = directory;
            ExpressionTextBox expression = null;
            CompletionMenu expressionMenu = null;
            try
            {
                var services = new WinFormsBootstrapper();
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
                    var scripts = (CodeEditorControlWrapper)services.Create<ScriptingView>().CodeEditorView;
                    var functions = (CodeEditorControlWrapper)services.Create<CustomFunctionsView>().CustomFunctionsEditor;
                    var editor = view == ViewName.Scripting ? scripts : functions;
                    var other = view == ViewName.Scripting ? functions : scripts;
                    var native = Descendants(editor).OfType<TextEditorCodeEditor>().Single();
                    var tabs = Descendants(editor).OfType<DocumentsTabControl>().Single();
                    services.Create<ISharedViewState>().CurrentView = view;
                    Application.DoEvents();
                    other.Text = "untouched workspace";
                    var area = native.ActiveTextAreaControl;
                    var source = "var α = 1;" + Environment.NewLine + "write(α);" + Environment.NewLine + "// last line";
                    var suffix = view == ViewName.Scripting ? ".tsl" : ".tslf";
                    var firstPath = Path.Combine(directory, "first" + suffix);
                    var secondPath = Path.Combine(directory, "second" + suffix);
                    File.WriteAllText(firstPath, source);
                    File.WriteAllText(secondPath, "second document");
                    editor.NewDocument(firstPath);
                    var first = native.Document;
                    area.TextArea.Focus();

                    Check(area.TextArea.ExecuteDialogKey(Keys.Control | Keys.A), "Native Select All was not handled.");
                    Check(area.SelectionManager.SelectedText == source,
                        "Native Select All must read the active multiline document, not the construction-time buffer.");
                    editor.Copy();
                    Check(Clipboard.GetText() == source, "Copy must put the actual selected source on the clipboard.");
                    editor.Cut();
                    Check(editor.Text == "" && tabs.SelectedTab.ImageIndex == 1, "Cut must remove the active source and mark it dirty.");
                    editor.Undo();
                    Check(editor.Text == source && tabs.SelectedTab.ImageIndex == 0, "One Undo must restore cut text and the clean indicator.");

                    editor.SelectAll();
                    Clipboard.SetText("replacement α");
                    editor.Paste();
                    Check(editor.Text == "replacement α", "Paste must replace the selected document, not append to it.");
                    editor.Undo();
                    Check(editor.Text == source && tabs.SelectedTab.ImageIndex == 0, "A replacement paste must undo in one step.");
                    editor.Redo();
                    Check(editor.Text == "replacement α", "Redo must restore the pasted replacement.");
                    editor.Undo();

                    area.TextArea.ExecuteDialogKey(Keys.Control | Keys.Home);
                    for (var i = 0; i < 4; i++) area.TextArea.ExecuteDialogKey(Keys.Shift | Keys.Right);
                    Check(area.SelectionManager.SelectedText == "var ", "Native Shift+Right must select text from the current buffer.");
                    Clipboard.SetText("let ");
                    editor.Paste();
                    Check(editor.Text == "let " + source.Substring(4), "Selected-range paste must preserve surrounding source.");
                    editor.Undo();
                    Check(editor.Text == source, "Undo must restore a selected-range replacement.");

                    first.ReadOnly = true;
                    editor.SelectAll();
                    editor.Cut();
                    editor.Paste();
                    area.TextArea.ExecuteDialogKey(Keys.Delete);
                    Check(editor.Text == source, "Read-only selections must reject Cut, Paste and Delete.");

                    // A long selection must not survive a switch to a shorter buffer.
                    editor.NewDocument(secondPath);
                    var second = native.Document;
                    Check(ReferenceEquals(area, native.ActiveTextAreaControl), "Document switching must preserve the live text area.");
                    Check(area.SelectionManager.SelectionCollection.Count == 0 && area.Caret.Offset == 0,
                        "A newly active document must have a valid caret and no stale selection.");
                    editor.SelectAll();
                    Check(area.SelectionManager.SelectedText == "second document" && !area.SelectionManager.SelectionIsReadonly,
                        "Selection and read-only state must follow the newly active document.");
                    Clipboard.SetText("second draft");
                    editor.Paste();
                    Check(editor.Text == "second draft" && first.TextContent == source, "Editing the second document must preserve the first.");
                    editor.SwitchTab(firstPath);
                    Check(ReferenceEquals(native.Document, first) && first.ReadOnly, "Document identity and read-only state must survive switching.");
                    first.ReadOnly = false;
                    editor.SwitchTab(secondPath);
                    editor.Undo();
                    Check(editor.Text == "second document" && tabs.SelectedTab.ImageIndex == 0, "The second document must retain its own undo history.");

                    // Native listeners must update geometry without resizing the window.
                    editor.Text = string.Join(Environment.NewLine, Enumerable.Range(0, 150).Select(i => "line " + i));
                    var longMaximum = area.VScrollBar.Maximum;
                    Check(longMaximum > area.Height, "Growing the active buffer must update its vertical scroll range immediately.");
                    area.Caret.Position = new TextLocation(5, 149);
                    editor.Text = "short";
                    Check(area.Caret.Offset == 0 && area.Caret.Line == 0 && area.VScrollBar.Maximum < longMaximum,
                        "Replacing long text with short text must reset the caret and shrink the scroll range.");
                    editor.Text = new string('x', 500);
                    var wideMaximum = area.HScrollBar.Maximum;
                    editor.Text = "short";
                    Check(area.HScrollBar.Maximum < wideMaximum,
                        "Replacing a long line must invalidate the native horizontal-width cache.");

                    var textChanges = 0;
                    native.TextChanged += (sender, args) => textChanges++;
                    first.Insert(0, "// inactive" + Environment.NewLine);
                    first.CommitUpdate();
                    Check(editor.Text == "short" && textChanges == 0 && area.Caret.Offset == 0,
                        "Inactive document changes must not affect the displayed text, caret or TextChanged event.");
                    for (var i = 0; i < 8; i++)
                    {
                        editor.SwitchTab(firstPath);
                        editor.SwitchTab(secondPath);
                    }
                    Check(textChanges == 0, "Switching documents must not emit content-change events.");
                    area.Caret.Position = second.OffsetToPosition(second.TextLength);
                    area.TextArea.InsertString("!");
                    Check(editor.Text == "short!" && textChanges == 1 && tabs.SelectedTab.ImageIndex == 1,
                        "One native insertion after repeated switches must emit exactly one change and mark the current tab dirty.");
                    editor.Undo();
                    Check(editor.Text == "short" && textChanges == 2, "Undo must emit one matching change.");

                    var redraws = 0;
                    area.TextArea.Invalidated += (sender, args) => redraws++;
                    second.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
                    second.CommitUpdate();
                    Check(redraws == 1, "A committed active-document redraw must reach the native area exactly once.");
                    first.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
                    first.CommitUpdate();
                    Check(redraws == 1, "An inactive document must not invalidate the active text area.");
                    second.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
                    Check(redraws > 1, "Active folding changes must invalidate the visible native area.");
                    var activeFoldingRedraws = redraws;
                    first.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
                    Check(redraws == activeFoldingRedraws, "Inactive folding changes must not affect the visible native area.");
                    Check(File.ReadAllText(firstPath) == source && File.ReadAllText(secondPath) == "second document" &&
                        other.Text == "untouched workspace", "Native editing must preserve disk files and the other workspace.");

                    window.Dispose();
                    var changesAfterDisposal = textChanges;
                    first.Insert(0, "after disposal ");
                    second.Insert(0, "after disposal ");
                    first.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
                    second.FoldingManager.NotifyFoldingsChanged(EventArgs.Empty);
                    first.CommitUpdate();
                    second.CommitUpdate();
                    Check(textChanges == changesAfterDisposal, "Disposed editors must not retain document callbacks.");
                    Check(first.UndoStack.TextEditorControl == null && second.UndoStack.TextEditorControl == null,
                        "Inactive and disposed documents must not retain an editor through their undo stack.");
                }
            }
            finally
            {
                try
                {
                    // The existing expression popup has an independent lifetime.
                    if (expressionMenu != null)
                    {
                        expressionMenu.Close();
                        expressionMenu.SetAutocompleteMenu(expression, null);
                        expressionMenu.Dispose();
                    }
                }
                finally
                {
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

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
