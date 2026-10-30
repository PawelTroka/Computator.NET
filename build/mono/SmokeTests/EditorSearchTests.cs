using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using Settings = Computator.NET.Core.Properties.Settings;

internal static partial class Program
{
    private static void DesktopFindReplace()
    {
        var previousEditor = Settings.Default.CodeEditor;
        var previousScripts = Settings.Default.ScriptingDirectory;
        var previousFunctions = Settings.Default.CustomFunctionsDirectory;
        var directory = Path.Combine(outputDirectory, "search-documents");
        Directory.CreateDirectory(directory);
        Settings.Default.CodeEditor = CodeEditorType.TextEditor;
        Settings.Default.ScriptingDirectory = Settings.Default.CustomFunctionsDirectory = directory;
        var bootstrapper = new WinFormsBootstrapper();
        try
        {
            using (bootstrapper.Container)
            using (var window = bootstrapper.Create<MainView>())
            {
                window.StartPosition = FormStartPosition.Manual;
                window.Location = new Point(5, 5);
                window.Size = new Size(1100, 760);
                window.Show();
                Application.DoEvents();
                var state = bootstrapper.Create<ISharedViewState>();
                var scripts = bootstrapper.Create<ScriptingView>().CodeEditorView;
                var functions = bootstrapper.Create<CustomFunctionsView>().CustomFunctionsEditor;
                var native = SearchDescendants((Control)scripts).OfType<TextEditorCodeEditor>().Single();
                var tabs = SearchDescendants((Control)scripts).OfType<DocumentsTabControl>().Single();
                var menu = window.Controls.OfType<MenuStripView>().Single().Controls.OfType<MenuStrip>().Single();
                var commands = menu.Items.OfType<ToolStripMenuItem>()
                    .SelectMany(item => item.DropDownItems.OfType<ToolStripMenuItem>()).ToArray();
                var findMenu = commands.Single(item => item.ShortcutKeys == (Keys.Control | Keys.F));
                var replaceMenu = commands.Single(item => item.ShortcutKeys == (Keys.Control | Keys.H));
                const string original = "😀 a.* b a.* α a.*";
                var path = Path.Combine(directory, "literal-unicode.tsl");
                var other = Path.Combine(directory, "other.tsl");
                var library = Path.Combine(directory, "library.tslf");
                File.WriteAllText(path, original);
                File.WriteAllText(other, "other disk text");
                File.WriteAllText(library, "library disk text");
                scripts.NewDocument(other);
                scripts.Text = "preserve this draft";
                scripts.NewDocument(path);
                functions.NewDocument(library);
                Form scriptDialog = null;
                Form functionsDialog = null;

                foreach (ViewName view in Enum.GetValues(typeof(ViewName)))
                {
                    state.CurrentView = view;
                    Application.DoEvents();
                    var editable = view == ViewName.Scripting || view == ViewName.CustomFunctions;
                    Check(findMenu.Enabled == editable && replaceMenu.Enabled == editable,
                        "Find/Replace menu enablement is incorrect for " + view + ".");
                    if (!editable) continue;
                    findMenu.PerformClick();
                    var dialog = SearchDialog(window);
                    Equal(Find(dialog, "searchScope").Text,
                        "Current document: " + (view == ViewName.Scripting ? "literal-unicode.tsl" : "library.tslf"));
                    Check(!Find(dialog, "replacementText").Visible, "Find mode must hide replacement controls.");
                    replaceMenu.PerformClick();
                    Same(SearchDialog(window), dialog, "Switching modes must reuse the modeless dialog.");
                    Check(Find(dialog, "replacementText").Visible, "Replace mode must expose the replacement field.");
                    if (view == ViewName.Scripting) scriptDialog = dialog;
                    else functionsDialog = dialog;
                    dialog.Hide();
                }

                state.CurrentView = ViewName.Scripting;
                SelectSearchText(native, 0, 0);
                replaceMenu.PerformClick();
                Same(SearchDialog(window), scriptDialog, "Reopening search must reuse the scripting dialog.");
                Find(scriptDialog, "findText").Text = "a.*";
                Find(scriptDialog, "replacementText").Text = @"$1\β";
                SearchClick(scriptDialog, "findNext");
                AssertSearchSelection(native, 3, "a.*");
                SearchClick(scriptDialog, "findPrevious");
                AssertSearchSelection(native, 15, "a.*");
                SearchClick(scriptDialog, "findNext");
                AssertSearchSelection(native, 3, "a.*");
                scriptDialog.Location = new Point(90, 210);
                scriptDialog.BringToFront();
                Application.DoEvents();
                Capture(window, "find-replace.png");
                Capture(scriptDialog, "find-replace-dialog.png");

                SearchClick(scriptDialog, "replaceAll");
                Equal(scripts.Text, @"😀 $1\β b $1\β α $1\β");
                AssertSearchSelection(native, 3, @"$1\β");
                Equal(Find(scriptDialog, "searchStatus").Text, "Replaced 3 occurrence(s).");
                Check(tabs.SelectedTab.ImageIndex == 1, "Direct legacy document replacement must mark the tab dirty.");
                scripts.Undo();
                Equal(scripts.Text, original);
                Check(tabs.SelectedTab.ImageIndex == 0, "One Undo must restore the saved content and clean indicator.");
                scripts.Redo();
                Equal(scripts.Text, @"😀 $1\β b $1\β α $1\β");
                scripts.Undo();
                scripts.SwitchTab(other);
                Equal(scripts.Text, "preserve this draft");
                Check(tabs.SelectedTab.ImageIndex == 1, "Search must preserve the other tab's dirty marker.");
                Check(Find(scriptDialog, "searchScope").Text.EndsWith("other.tsl", StringComparison.Ordinal),
                    "An open dialog must follow the selected document.");
                Find(scriptDialog, "findText").Text = "draft";
                Find(scriptDialog, "replacementText").Text = "buffer";
                SearchClick(scriptDialog, "replaceAll");
                Equal(scripts.Text, "preserve this buffer");
                scripts.SwitchTab(path);
                Equal(scripts.Text, original);
                Equal(File.ReadAllText(path), original);
                Equal(File.ReadAllText(other), "other disk text");

                SelectSearchText(native, 3, 3);
                scripts.ShowFindReplace(true);
                Equal(Find(scriptDialog, "findText").Text, "a.*");
                Find(scriptDialog, "replacementText").Text = "a replacement longer than the document";
                native.Document.ReadOnly = true;
                SearchClick(scriptDialog, "replaceCurrent");
                Equal(scripts.Text, original);
                AssertSearchSelection(native, 3, "a.*");
                Check(!Find(scriptDialog, "replaceAll").Enabled && !Find(scriptDialog, "replaceCurrent").Enabled,
                    "Read-only state must disable both replacement commands.");
                SearchClick(scriptDialog, "findNext");
                AssertSearchSelection(native, 9, "a.*");
                native.Document.ReadOnly = false;
                scripts.ShowFindReplace(true);
                Find(scriptDialog, "replacementText").Text = "x";
                SearchClick(scriptDialog, "replaceCurrent");
                Equal(scripts.Text, "😀 a.* b x α a.*");
                scripts.Undo();
                Equal(scripts.Text, original);

                // Document switching must discard a selection from a longer buffer.
                SelectSearchText(native, 15, 3);
                var empty = Path.Combine(directory, "empty.tsl");
                File.WriteAllText(empty, string.Empty);
                scripts.NewDocument(empty);
                scripts.ShowFindReplace(true);
                SearchClick(scriptDialog, "findNext");
                SearchClick(scriptDialog, "replaceCurrent");
                Equal(scripts.Text, string.Empty);
                Check(Find(scriptDialog, "searchScope").Text.EndsWith("empty.tsl", StringComparison.Ordinal),
                    "Reopening search must use the new empty document's scope.");

                // Whole-word replacement must not separate an enclosing combining
                // mark from its base letter, on the native Mono editor too.
                var combining = Path.Combine(directory, "combining.tsl");
                File.WriteAllText(combining, "x\u20DD x x\u20E3");
                scripts.NewDocument(combining);
                SelectSearchText(native, 0, 0);
                Find(scriptDialog, "findText").Text = "x";
                Find(scriptDialog, "replacementText").Text = "y";
                ((CheckBox)Find(scriptDialog, "wholeWord")).Checked = true;
                SearchClick(scriptDialog, "findNext");
                AssertSearchSelection(native, 3, "x");
                SearchClick(scriptDialog, "replaceAll");
                Equal(scripts.Text, "x\u20DD y x\u20E3");
                scripts.Undo();
                Equal(scripts.Text, "x\u20DD x x\u20E3");
                scripts.SwitchTab(path);

                state.CurrentView = ViewName.CustomFunctions;
                Check(!scriptDialog.Visible, "Changing workspace must hide its search window.");
                replaceMenu.PerformClick();
                Same(SearchDialog(window), functionsDialog, "Custom functions must retain their own search dialog.");
                Find(functionsDialog, "findText").Text = "disk";
                Find(functionsDialog, "replacementText").Text = "draft";
                SearchClick(functionsDialog, "replaceAll");
                Equal(functions.Text, "library draft text");
                Equal(scripts.Text, original);
                // Dispose the owner, without prompting to save intentional test drafts.
                window.Dispose();
                Check(scriptDialog.IsDisposed && functionsDialog.IsDisposed,
                    "Disposing the desktop must dispose both modeless search windows.");
            }
        }
        finally
        {
            Settings.Default.CodeEditor = previousEditor;
            Settings.Default.ScriptingDirectory = previousScripts;
            Settings.Default.CustomFunctionsDirectory = previousFunctions;
        }
    }

    private static Form SearchDialog(Form owner) => owner.OwnedForms.Single(form => form.Name == "findReplaceDialog" && form.Visible);
    private static void SearchClick(Control dialog, string name) => ((Button)Find(dialog, name)).PerformClick();

    private static void SelectSearchText(TextEditorCodeEditor editor, int start, int length)
    {
        var area = editor.ActiveTextAreaControl;
        area.SelectionManager.ClearSelection();
        area.Caret.Position = editor.Document.OffsetToPosition(start + length);
        area.SelectionManager.SetSelection(new ICSharpCode.TextEditor.Document.DefaultSelection(editor.Document,
            editor.Document.OffsetToPosition(start), editor.Document.OffsetToPosition(start + length)));
    }

    private static void AssertSearchSelection(TextEditorCodeEditor editor, int start, string text)
    {
        var selection = editor.ActiveTextAreaControl.SelectionManager.SelectionCollection.Single();
        Check(selection.Offset == start && selection.Length == text.Length,
            "Wrong UTF-16 search selection: " + selection.Offset + "/" + selection.Length + "; expected " + start + "/" + text.Length + ".");
        Equal(editor.Text.Substring(selection.Offset, selection.Length), text);
    }

    private static IEnumerable<Control> SearchDescendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var nested in SearchDescendants(child)) yield return nested;
        }
    }
}
