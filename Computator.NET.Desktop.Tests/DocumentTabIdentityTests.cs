using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using NUnit.Framework;
using Settings = Computator.NET.Core.Properties.Settings;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class DocumentTabIdentityTests
    {
        private static readonly string[] OriginalText = { "write(11);", "write(22);", "write(33);" };
        private CoreBootstrapper _bootstrapper;
        private Form _window;
        private CodeEditorControlWrapper _editor;
        private DocumentsTabControl _tabs;
        private CodeEditorType _oldEditor;
        private string _directory;

        [OneTimeSetUp]
        public void OpenRealDocumentEditors()
        {
            GSLInitializer.Initialize();
            _oldEditor = Settings.Default.CodeEditor;
            _directory = Path.Combine(Path.GetTempPath(), "Computator-DocumentTabs-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _bootstrapper = new CoreBootstrapper();
            _editor = _bootstrapper.Create<CodeEditorControlWrapper>();
            _window = new Form();
            _window.Controls.Add(_editor);
            NUnit.Framework.Assert.That(_window.Handle, Is.Not.EqualTo(IntPtr.Zero));
            _tabs = Descendants(_editor).OfType<DocumentsTabControl>().Single();
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ClosingBackgroundDocumentPreservesUnsavedTextAndSaveDestination(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            const string draft = "write(101);";
            _editor.SwitchTab(paths[0]);
            _editor.Text = draft;

            _editor.RemoveTab(paths[1]);

            AssertSelectedDocument(paths[0], draft);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(paths[1]), Is.False);
            _editor.Save();
            AssertFileContents(paths, 0, draft);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ClosingSelectedDocumentSavesTheDocumentOfTheSurvivingSelectedTab(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            var drafts = new[] { "write(111);", "write(222);", "write(333);" };
            for (var i = 0; i < paths.Length; i++)
            {
                _editor.SwitchTab(paths[i]);
                _editor.Text = drafts[i];
            }
            _editor.SwitchTab(paths[1]);

            _editor.RemoveTab(paths[1]);

            var selectedPath = _tabs.SelectedTab.Text;
            NUnit.Framework.Assert.That(selectedPath, Is.AnyOf(paths[0], paths[2]));
            var selectedIndex = Array.IndexOf(paths, selectedPath);
            AssertSelectedDocument(selectedPath, drafts[selectedIndex]);
            _editor.Save();
            AssertFileContents(paths, selectedIndex, drafts[selectedIndex]);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void CloseTabsToTheLeftKeepsTheSelectedFileAndTextTogether(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            const string draft = "write(202);";
            _editor.SwitchTab(paths[1]);
            _editor.Text = draft;

            ((ToolStripMenuItem)_tabs.ContextMenuStrip.Items["closeTabsToTheLeftToolStripMenuItem"]).PerformClick();

            AssertSelectedDocument(paths[1], draft);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(paths[0]), Is.False);
            _editor.Save();
            AssertFileContents(paths, 1, draft);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ClosingTheLastDocumentSelectsANewEmptyDocument(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            foreach (var path in paths)
                _editor.RemoveTab(path);

            NUnit.Framework.Assert.That(_tabs.SelectedTab, Is.Not.Null);
            NUnit.Framework.Assert.That(paths, Does.Not.Contain(_editor.CurrentFileName));
            NUnit.Framework.Assert.That(_editor.ContainsDocument(_editor.CurrentFileName), Is.True);
            NUnit.Framework.Assert.That(_editor.Text, Is.Empty);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void RenamedFileReopensItsExistingTabAndTheOldNameCanBeReused(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            const string draft = "write(404);";
            var renamed = Path.Combine(_directory, "Żółć " + Guid.NewGuid().ToString("N") + ".tsl");
            _editor.SwitchTab(paths[0]);
            _editor.Text = draft;
            File.Move(paths[0], renamed);

            _editor.RenameDocument(paths[0], renamed);
            _editor.SwitchTab(paths[2]);
            _editor.NewDocument(renamed.ToUpperInvariant());

            AssertSelectedDocument(renamed, draft);
            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(4), "Three documents and the new-tab button; reopening must not duplicate the renamed tab.");
            NUnit.Framework.Assert.That(_tabs.TabPages.ContainsKey(renamed), Is.True);
            NUnit.Framework.Assert.That(_tabs.TabPages.ContainsKey(paths[0]), Is.False);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(paths[0]), Is.False);
            _editor.Save();
            NUnit.Framework.Assert.That(File.ReadAllText(renamed), Is.EqualTo(draft));

            const string replacement = "write(505);";
            File.WriteAllText(paths[0], replacement);
            _editor.NewDocument(paths[0]);
            AssertSelectedDocument(paths[0], replacement);
            _editor.NewDocument(renamed);
            AssertSelectedDocument(renamed, draft);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void CurrentFileNameChangeKeepsSaveAsIdentityAcrossEditorChanges(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            const string draft = "write(606);";
            var savedAs = Path.Combine(_directory, Guid.NewGuid().ToString("N") + ".tslf");
            _editor.SwitchTab(paths[0]);
            _editor.Text = draft;

            // Save and Save As use this same property after the native file dialog
            // writes the chosen path. Exercise that identity transition without a dialog.
            File.WriteAllText(savedAs, draft);
            _editor.CurrentFileName = savedAs;
            _editor.CurrentFileName = savedAs;
            NUnit.Framework.Assert.That(_editor.ContainsDocument(paths[0]), Is.False);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(savedAs), Is.True);
            NUnit.Framework.Assert.That(_tabs.TabPages.ContainsKey(savedAs), Is.True);
            NUnit.Framework.Assert.That(_tabs.TabPages.ContainsKey(paths[0]), Is.False);

            _editor.NewDocument(paths[0]);
            AssertSelectedDocument(paths[0], OriginalText[0]);
            _editor.NewDocument(savedAs);
            AssertSelectedDocument(savedAs, draft);
            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(5));

            Settings.Default.CodeEditor = kind == CodeEditorType.Scintilla
                ? CodeEditorType.AvalonEdit
                : CodeEditorType.Scintilla;
            AssertSelectedDocument(savedAs, draft);
            _editor.Text = "write(707);";
            _editor.Save();
            NUnit.Framework.Assert.That(File.ReadAllText(savedAs), Is.EqualTo("write(707);"));
            for (var i = 0; i < paths.Length; i++)
                NUnit.Framework.Assert.That(File.ReadAllText(paths[i]), Is.EqualTo(OriginalText[i]));
        }

        private string[] OpenDocuments(CodeEditorType kind)
        {
            Settings.Default.CodeEditor = kind;
            var previousTabs = _tabs.TabPages.Cast<TabPage>()
                .Where(tab => tab.Name != "addTabPage").Select(tab => tab.Text).ToArray();
            var paths = OriginalText.Select(text => Path.Combine(_directory, Guid.NewGuid().ToString("N") + ".tsl")).ToArray();
            for (var i = 0; i < paths.Length; i++)
            {
                File.WriteAllText(paths[i], OriginalText[i]);
                _editor.NewDocument(paths[i]);
            }
            foreach (var path in previousTabs)
                _editor.RemoveTab(path);
            return paths;
        }

        private void AssertSelectedDocument(string path, string text)
        {
            NUnit.Framework.Assert.That(_tabs.SelectedTab.Text, Is.EqualTo(path));
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(path));
            NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(text));
        }

        private static void AssertFileContents(string[] paths, int savedIndex, string savedText)
        {
            for (var i = 0; i < paths.Length; i++)
                NUnit.Framework.Assert.That(File.ReadAllText(paths[i]), Is.EqualTo(i == savedIndex ? savedText : OriginalText[i]),
                    "Saving one selected document must leave every other file unchanged.");
        }

        private static IEnumerable<Control> Descendants(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Descendants(child)) yield return descendant;
            }
        }

        [OneTimeTearDown]
        public void CloseRealDocumentEditors()
        {
            Settings.Default.CodeEditor = _oldEditor;
            _window?.Dispose();
            _bootstrapper?.Container.Dispose();
            if (_directory != null && Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }
    }
}
