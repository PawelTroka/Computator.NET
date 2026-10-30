using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Controls.CodeEditors.AvalonEdit;
using Computator.NET.Desktop.Views;
using ICSharpCode.AvalonEdit.Search;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class FindReplaceWorkflowTests
    {
        private WinFormsBootstrapper _services;
        private MainView _window;
        private ISharedViewState _state;
        private ICodeDocumentsEditor _scripts;
        private ICodeDocumentsEditor _functions;
        private ToolStripMenuItem _findMenu;
        private ToolStripMenuItem _replaceMenu;
        private CodeEditorType _previousEditor;
        private string _previousScripts;
        private string _previousFunctions;
        private string _directory;

        [OneTimeSetUp]
        public void InitializeGsl() => GSLInitializer.Initialize();

        [SetUp]
        public void OpenDesktop()
        {
            _previousEditor = Settings.Default.CodeEditor;
            _previousScripts = Settings.Default.ScriptingDirectory;
            _previousFunctions = Settings.Default.CustomFunctionsDirectory;
            _directory = Path.Combine(Path.GetTempPath(), "Computator-Search-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            Settings.Default.ScriptingDirectory = Settings.Default.CustomFunctionsDirectory = _directory;
            _services = new WinFormsBootstrapper();
            _window = _services.Create<MainView>();
            _window.StartPosition = FormStartPosition.Manual;
            _window.Location = new Point(10, 10);
            _window.Size = new Size(1000, 700);
            _window.TopMost = true;
            _window.Show();
            _state = _services.Create<ISharedViewState>();
            _scripts = _services.Create<ScriptingView>().CodeEditorView;
            _functions = _services.Create<CustomFunctionsView>().CustomFunctionsEditor;
            var menu = _window.Controls.OfType<MenuStripView>().Single().Controls.OfType<MenuStrip>().Single();
            var commands = menu.Items.OfType<ToolStripMenuItem>()
                .SelectMany(item => item.DropDownItems.OfType<ToolStripMenuItem>()).ToArray();
            _findMenu = commands.Single(item => item.ShortcutKeys == (Keys.Control | Keys.F));
            _replaceMenu = commands.Single(item => item.ShortcutKeys == (Keys.Control | Keys.H));
        }

        [TearDown]
        public void CloseDesktop()
        {
            // Disposal avoids an unrelated save prompt for the intentional test drafts.
            _window?.Dispose();
            _services?.Container.Dispose();
            Settings.Default.CodeEditor = _previousEditor;
            Settings.Default.ScriptingDirectory = _previousScripts;
            Settings.Default.CustomFunctionsDirectory = _previousFunctions;
            if (_directory != null && Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void MenuRoutesOnlyTheTwoVisibleDocumentWorkspaces(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = kind;
            var script = WriteFile("script.tsl", "script text");
            var functions = WriteFile("library.tslf", "function text");
            _scripts.NewDocument(script);
            _functions.NewDocument(functions);
            foreach (ViewName view in Enum.GetValues(typeof(ViewName)))
            {
                _state.CurrentView = view;
                Application.DoEvents();
                var editable = view == ViewName.Scripting || view == ViewName.CustomFunctions;
                NUnit.Framework.Assert.That(_findMenu.Enabled, Is.EqualTo(editable), view.ToString());
                NUnit.Framework.Assert.That(_replaceMenu.Enabled, Is.EqualTo(editable), view.ToString());
                if (!editable)
                {
                    _findMenu.PerformClick();
                    NUnit.Framework.Assert.That(_window.OwnedForms.Any(form => form.Visible), Is.False);
                    continue;
                }
                _findMenu.PerformClick();
                var dialog = Dialog();
                NUnit.Framework.Assert.That(Field<Label>(dialog, "searchScope").Text,
                    Does.Contain(view == ViewName.Scripting ? "script.tsl" : "library.tslf"));
                NUnit.Framework.Assert.That(Field<TextBox>(dialog, "replacementText").Visible, Is.False);
                _replaceMenu.PerformClick();
                NUnit.Framework.Assert.That(Dialog(), Is.SameAs(dialog));
                NUnit.Framework.Assert.That(Field<TextBox>(dialog, "replacementText").Visible, Is.True);
                dialog.Hide();
            }
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo("script text"));
            NUnit.Framework.Assert.That(_functions.Text, Is.EqualTo("function text"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        [SetCulture("de-DE"), SetUICulture("de-DE")]
        public void GermanResourcesLoadInTheNativeFindReplaceWorkflow(CodeEditorType kind) =>
            AssertLocalizedFindReplaceWorkflow(kind, "de-DE");

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        [SetCulture("cs-CZ"), SetUICulture("cs-CZ")]
        public void CzechResourcesLoadInTheNativeFindReplaceWorkflow(CodeEditorType kind) =>
            AssertLocalizedFindReplaceWorkflow(kind, "cs-CZ");

        private void AssertLocalizedFindReplaceWorkflow(CodeEditorType kind, string culture)
        {
            NUnit.Framework.Assert.That(CultureInfo.CurrentUICulture.Name, Is.EqualTo(culture));
            var german = culture == "de-DE";
            const string original = "größer žluťoučký größer";
            const string filename = "größer-žluťoučký.tsl";
            var path = OpenScript(kind, filename, original);
            var target = Target(_scripts);
            target.Select(0, 0);
            NUnit.Framework.Assert.That(_findMenu.Text, Is.EqualTo(german ? "&Suchen…" : "&Najít…"));
            NUnit.Framework.Assert.That(_replaceMenu.Text, Is.EqualTo(german ? "&Ersetzen…" : "Na&hradit…"));
            _replaceMenu.PerformClick();
            var dialog = Dialog();
            NUnit.Framework.Assert.That(dialog.Text, Is.EqualTo(german ? "Suchen und ersetzen" : "Najít a nahradit"));
            NUnit.Framework.Assert.That(Field<Label>(dialog, "searchScope").Text,
                Is.EqualTo((german ? "Aktuelles Dokument: " : "Aktuální dokument: ") + filename));
            NUnit.Framework.Assert.That(Field<CheckBox>(dialog, "wholeWord").Text,
                Is.EqualTo(german ? "Ganzes Wort" : "Celé slovo"));
            NUnit.Framework.Assert.That(Field<Button>(dialog, "replaceAll").Text,
                Is.EqualTo(german ? "Alle ersetzen" : "Nahradit vše"));

            Field<TextBox>(dialog, "findText").Text = "größer";
            Field<TextBox>(dialog, "replacementText").Text = "příliš";
            Click(dialog, "replaceAll");
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo("příliš žluťoučký příliš"));
            AssertSelection(target, 0, "příliš");
            NUnit.Framework.Assert.That(Field<Label>(dialog, "searchStatus").Text,
                Is.EqualTo(german ? "2 Vorkommen ersetzt." : "Počet nahrazených výskytů: 2."));
            SaveScreenshot(dialog, "find-replace-" + culture + "-" + kind + ".png");
            _scripts.Undo();
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo(original));
            NUnit.Framework.Assert.That(File.ReadAllText(path), Is.EqualTo(original));

            SetReadOnly(Native(_scripts), true);
            dialog.RefreshScope();
            NUnit.Framework.Assert.That(Field<Label>(dialog, "searchStatus").Text,
                Is.EqualTo(german ? "Dieses Dokument ist schreibgeschützt." : "Tento dokument je jen pro čtení."));
            NUnit.Framework.Assert.That(Field<Button>(dialog, "replaceAll").Enabled, Is.False);
            NUnit.Framework.Assert.That(Field<Button>(dialog, "replaceCurrent").Enabled, Is.False);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void WholeWordNavigationWrapsAndReplaceCurrentUsesUtf16Selection(CodeEditorType kind)
        {
            const string original = "😀x x2 x_ x 😀 x\u20DD";
            OpenScript(kind, "unicode.tsl", original);
            var target = Target(_scripts);
            target.Select(0, 0);
            _replaceMenu.PerformClick();
            var dialog = Dialog();
            Field<TextBox>(dialog, "findText").Text = "x";
            Field<CheckBox>(dialog, "wholeWord").Checked = true;
            Click(dialog, "findNext");
            AssertSelection(target, 2, "x");
            Click(dialog, "findNext");
            AssertSelection(target, 10, "x");
            Click(dialog, "findNext");
            AssertSelection(target, 2, "x");
            Click(dialog, "findPrevious");
            AssertSelection(target, 10, "x");
            Field<TextBox>(dialog, "replacementText").Text = "ββ";
            Click(dialog, "replaceCurrent");
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo("😀x x2 x_ ββ 😀 x\u20DD"));
            AssertSelection(target, 2, "x");
            _scripts.Undo();
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo(original));
            Field<TextBox>(dialog, "findText").Text = "absent";
            var previousStart = target.SelectionStart;
            var previousLength = target.SelectionLength;
            Click(dialog, "findNext");
            NUnit.Framework.Assert.That(target.SelectionStart, Is.EqualTo(previousStart));
            NUnit.Framework.Assert.That(target.SelectionLength, Is.EqualTo(previousLength));
            Field<TextBox>(dialog, "findText").Clear();
            NUnit.Framework.Assert.That(Field<Button>(dialog, "findNext").Enabled, Is.False);
            NUnit.Framework.Assert.That(Field<Button>(dialog, "replaceAll").Enabled, Is.False);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void LiteralReplaceAllIsOneUndoAndPreservesTheOtherDirtyTab(CodeEditorType kind)
        {
            const string original = "😀 a.* b a.* α a.*";
            var script = OpenScript(kind, "replace-all.tsl", original);
            var other = WriteFile("other.tsl", "other saved text");
            _scripts.NewDocument(other);
            _scripts.Text = "keep this unsaved draft";
            _scripts.SwitchTab(script);
            Target(_scripts).Select(0, 0);
            _replaceMenu.PerformClick();
            var dialog = Dialog();
            Field<TextBox>(dialog, "findText").Text = "a.*";
            Field<TextBox>(dialog, "replacementText").Text = @"$1\β";
            Click(dialog, "findNext");
            AssertSelection(Target(_scripts), 3, "a.*");
            SaveScreenshot(dialog, "find-replace-" + kind + ".png");
            Click(dialog, "replaceAll");
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo(@"😀 $1\β b $1\β α $1\β"));
            AssertSelection(Target(_scripts), 3, @"$1\β");
            NUnit.Framework.Assert.That(Field<Label>(dialog, "searchStatus").Text, Does.Contain("3"));
            NUnit.Framework.Assert.That(Tabs(_scripts).SelectedTab.ImageIndex, Is.EqualTo(1), "Replacement must mark the tab dirty.");
            _scripts.Undo();
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo(original), "One Undo must revert the entire replacement batch.");
            NUnit.Framework.Assert.That(Tabs(_scripts).SelectedTab.ImageIndex, Is.EqualTo(0));
            _scripts.Redo();
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo(@"😀 $1\β b $1\β α $1\β"));
            _scripts.SwitchTab(other);
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo("keep this unsaved draft"));
            NUnit.Framework.Assert.That(Tabs(_scripts).SelectedTab.ImageIndex, Is.EqualTo(1));
            NUnit.Framework.Assert.That(File.ReadAllText(script), Is.EqualTo(original));
            NUnit.Framework.Assert.That(File.ReadAllText(other), Is.EqualTo("other saved text"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ReadOnlyChangesAfterOpeningCannotBeBypassedByReplacement(CodeEditorType kind)
        {
            OpenScript(kind, "read-only.tsl", "x x");
            var target = Target(_scripts);
            target.Select(0, 1);
            _replaceMenu.PerformClick();
            var dialog = Dialog();
            NUnit.Framework.Assert.That(Field<TextBox>(dialog, "findText").Text, Is.EqualTo("x"));
            Field<TextBox>(dialog, "replacementText").Text = "long replacement";
            SetReadOnly(Native(_scripts), true);
            Click(dialog, "replaceCurrent");
            NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo("x x"));
            AssertSelection(target, 0, "x");
            NUnit.Framework.Assert.That(Field<Button>(dialog, "replaceCurrent").Enabled, Is.False);
            NUnit.Framework.Assert.That(Field<Button>(dialog, "replaceAll").Enabled, Is.False);
            NUnit.Framework.Assert.That(target.Replace(new[] { 0, 2 }, 1, "changed"), Is.False);
            Click(dialog, "findNext");
            AssertSelection(target, 2, "x");
            NUnit.Framework.Assert.That(Tabs(_scripts).SelectedTab.ImageIndex, Is.EqualTo(0));
            SetReadOnly(Native(_scripts), false);
            dialog.RefreshScope();
            NUnit.Framework.Assert.That(Field<Button>(dialog, "replaceAll").Enabled, Is.True);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ReopeningSearchAfterSwitchingToAShorterDocumentCannotReuseItsOldSelection(CodeEditorType kind)
        {
            OpenScript(kind, "long.tsl", "a long document with a selection at the end");
            Target(_scripts).Select(_scripts.Text.Length - 3, 3);
            _replaceMenu.PerformClick();
            var dialog = Dialog();
            foreach (var text in new[] { "x", string.Empty })
            {
                _scripts.NewDocument(WriteFile("short-" + text.Length + ".tsl", text));
                _replaceMenu.PerformClick();
                Click(dialog, "findNext");
                Click(dialog, "replaceCurrent");
                NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo(text));
                NUnit.Framework.Assert.That(Field<Label>(dialog, "searchScope").Text, Does.Contain("short-" + text.Length + ".tsl"));
            }
        }

        [Test]
        public void OpenDialogTracksActiveTabAndEditorChangesWithoutEditingInactiveBuffers()
        {
            var original = OpenScript(CodeEditorType.Scintilla, "preserve.tsl", "target original");
            _scripts.Text = "target unsaved";
            Target(_scripts).Select(0, 0);
            _replaceMenu.PerformClick();
            var dialog = Dialog();
            Field<TextBox>(dialog, "findText").Text = "target";
            foreach (var kind in new[] { CodeEditorType.AvalonEdit, CodeEditorType.TextEditor, CodeEditorType.Scintilla })
            {
                var path = WriteFile(kind + ".tsl", "target " + kind);
                _scripts.NewDocument(path);
                Settings.Default.CodeEditor = kind;
                NUnit.Framework.Assert.That(dialog.Visible, Is.True);
                NUnit.Framework.Assert.That(Field<Label>(dialog, "searchScope").Text, Does.Contain(kind + ".tsl"));
                NUnit.Framework.Assert.That(Field<Button>(dialog, "advancedSearch").Visible,
                    Is.EqualTo(kind == CodeEditorType.AvalonEdit));
                Field<TextBox>(dialog, "replacementText").Text = "changed";
                Click(dialog, "replaceAll");
                NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo("changed " + kind));
                _scripts.SwitchTab(original);
                NUnit.Framework.Assert.That(_scripts.Text, Is.EqualTo("target unsaved"));
                NUnit.Framework.Assert.That(Field<Label>(dialog, "searchScope").Text, Does.Contain("preserve.tsl"));
            }
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void NativeKeyboardOpensBothModesAndNavigatesExistingSearch(CodeEditorType kind)
        {
            OpenScript(kind, "shortcuts.tsl", "x x");
            Target(_scripts).Select(0, 1);
            FocusNative(_scripts);
            SendKeys.SendWait("^f");
            PumpUntil(() => _window.OwnedForms.OfType<FindReplaceDialog>().Any(form => form.Visible));
            var dialog = Dialog();
            NUnit.Framework.Assert.That(Field<TextBox>(dialog, "findText").Text, Is.EqualTo("x"));
            NUnit.Framework.Assert.That(Field<TextBox>(dialog, "replacementText").Visible, Is.False);
            SendKeys.SendWait("^h");
            PumpUntil(() => Field<TextBox>(dialog, "replacementText").Visible);
            dialog.Hide();
            FocusNative(_scripts);
            SendKeys.SendWait("{F3}");
            PumpUntil(() => dialog.Visible);
            AssertSelection(Target(_scripts), 2, "x");
            SendKeys.SendWait("+{F3}");
            Application.DoEvents();
            AssertSelection(Target(_scripts), 0, "x");
            SendKeys.SendWait("{ESC}");
            PumpUntil(() => dialog.IsDisposed);
        }

        [Test]
        public void AvalonAdvancedSearchRemainsAvailableWithItsNativeRegexAndF3()
        {
            OpenScript(CodeEditorType.AvalonEdit, "advanced.tsl", "😀 a1 a2");
            _findMenu.PerformClick();
            var dialog = Dialog();
            Click(dialog, "advancedSearch");
            Application.DoEvents();
            NUnit.Framework.Assert.That(dialog.Visible, Is.False);
            var avalon = (AvalonEditCodeEditor)Native(_scripts);
            NUnit.Framework.Assert.That(avalon.IsAdvancedSearchOpen, Is.True);
            var panel = WpfDescendants(avalon).OfType<SearchPanel>().Single();
            panel.UseRegex = true;
            panel.SearchPattern = @"a\d";
            Target(_scripts).Select(0, 0);
            panel.FindNext();
            AssertSelection(Target(_scripts), 3, "a1");
            FocusNative(_scripts);
            SendKeys.SendWait("{F3}");
            Application.DoEvents();
            AssertSelection(Target(_scripts), 6, "a2");
            NUnit.Framework.Assert.That(dialog.Visible, Is.False);
            _replaceMenu.PerformClick();
            NUnit.Framework.Assert.That(Dialog(), Is.SameAs(dialog));
            NUnit.Framework.Assert.That(avalon.IsAdvancedSearchOpen, Is.False);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void WorkspaceVisibilityAndDisposalCannotLeaveAnOrphanedSearchWindow(CodeEditorType kind)
        {
            OpenScript(kind, "lifetime.tsl", "x");
            _findMenu.PerformClick();
            var scriptDialog = Dialog();
            _state.CurrentView = ViewName.CustomFunctions;
            NUnit.Framework.Assert.That(scriptDialog.Visible, Is.False);
            _replaceMenu.PerformClick();
            var functionsDialog = Dialog();
            NUnit.Framework.Assert.That(functionsDialog, Is.Not.SameAs(scriptDialog));
            _state.CurrentView = ViewName.Scripting;
            NUnit.Framework.Assert.That(functionsDialog.Visible, Is.False);
            _findMenu.PerformClick();
            NUnit.Framework.Assert.That(Dialog(), Is.SameAs(scriptDialog));
            ((Control)_scripts).Dispose();
            NUnit.Framework.Assert.That(scriptDialog.IsDisposed, Is.True);
            _window.Dispose();
            NUnit.Framework.Assert.That(functionsDialog.IsDisposed, Is.True);
        }

        private string OpenScript(CodeEditorType kind, string name, string text)
        {
            Settings.Default.CodeEditor = kind;
            var path = WriteFile(name, text);
            _scripts.NewDocument(path);
            _state.CurrentView = ViewName.Scripting;
            Application.DoEvents();
            return path;
        }

        private string WriteFile(string name, string text)
        {
            var path = Path.Combine(_directory, name);
            File.WriteAllText(path, text);
            return path;
        }

        private FindReplaceDialog Dialog() => _window.OwnedForms.OfType<FindReplaceDialog>().Single(form => form.Visible);
        private static T Field<T>(Control dialog, string name) where T : Control => (T)dialog.Controls.Find(name, true).Single();
        private static void Click(Control dialog, string name) => Field<Button>(dialog, name).PerformClick();
        private static DocumentsTabControl Tabs(ICodeDocumentsEditor editor) => Descendants((Control)editor).OfType<DocumentsTabControl>().Single();
        private static CodeEditorSearchTarget Target(ICodeDocumentsEditor editor) => new CodeEditorSearchTarget(Native(editor));

        private static ICodeEditorControl Native(ICodeDocumentsEditor editor)
        {
            var controls = Descendants((Control)editor).ToArray();
            if (Settings.Default.CodeEditor == CodeEditorType.AvalonEdit)
                return (AvalonEditCodeEditor)controls.OfType<ElementHost>().Single().Child;
            if (Settings.Default.CodeEditor == CodeEditorType.Scintilla) return controls.OfType<ScintillaCodeEditorControl>().Single();
            return controls.OfType<TextEditorCodeEditor>().Single();
        }

        private static void SetReadOnly(ICodeEditorControl editor, bool value)
        {
            if (editor is AvalonEditCodeEditor avalon) avalon.IsReadOnly = value;
            else if (editor is ScintillaCodeEditorControl scintilla) scintilla.ReadOnly = value;
            else ((TextEditorCodeEditor)editor).Document.ReadOnly = value;
        }

        private void FocusNative(ICodeDocumentsEditor editor)
        {
            _window.Activate();
            var native = Native(editor);
            if (native is AvalonEditCodeEditor avalon)
                System.Windows.Input.Keyboard.Focus(avalon.TextArea);
            else if (native is TextEditorCodeEditor legacy) legacy.ActiveTextAreaControl.TextArea.Focus();
            else ((Control)native).Focus();
            Application.DoEvents();
        }

        private static void AssertSelection(CodeEditorSearchTarget target, int start, string text)
        {
            NUnit.Framework.Assert.That(target.SelectionStart, Is.EqualTo(start));
            NUnit.Framework.Assert.That(target.SelectionLength, Is.EqualTo(text.Length));
            NUnit.Framework.Assert.That(target.Text.Substring(target.SelectionStart, target.SelectionLength), Is.EqualTo(text));
        }

        private static IEnumerable<Control> Descendants(Control control)
        {
            foreach (Control child in control.Controls)
            {
                yield return child;
                foreach (var nested in Descendants(child)) yield return nested;
            }
        }

        private static IEnumerable<System.Windows.DependencyObject> WpfDescendants(System.Windows.DependencyObject parent)
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                var child = VisualTreeHelper.GetChild(parent, index);
                yield return child;
                foreach (var nested in WpfDescendants(child)) yield return nested;
            }
        }

        private static void PumpUntil(Func<bool> condition)
        {
            var watch = Stopwatch.StartNew();
            while (!condition() && watch.ElapsedMilliseconds < 3000)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
            NUnit.Framework.Assert.That(condition(), Is.True, "Native window did not reach the expected state.");
        }

        private void SaveScreenshot(FindReplaceDialog dialog, string name)
        {
            dialog.Location = new Point(80, 180);
            _window.BringToFront();
            dialog.BringToFront();
            dialog.Activate();
            _window.Refresh();
            dialog.Refresh();
            Application.DoEvents();
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, name);
            using (var image = new Bitmap(_window.Width, _window.Height))
            {
                using (var graphics = Graphics.FromImage(image))
                    graphics.CopyFromScreen(_window.Location, Point.Empty, image.Size);
                image.Save(path, ImageFormat.Png);
            }
            TestContext.AddTestAttachment(path, "Actual desktop find/replace dialog and active " + Settings.Default.CodeEditor + " document.");
        }
    }
}
