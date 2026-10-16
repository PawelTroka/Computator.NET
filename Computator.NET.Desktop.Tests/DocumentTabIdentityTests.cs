using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        [TestCase(CodeEditorType.Scintilla, "closeOtherTabsToolStripMenuItem", 2)]
        [TestCase(CodeEditorType.AvalonEdit, "closeOtherTabsToolStripMenuItem", 2)]
        [TestCase(CodeEditorType.TextEditor, "closeOtherTabsToolStripMenuItem", 2)]
        [TestCase(CodeEditorType.Scintilla, "closeTabsToTheRightToolStripMenuItem", 0)]
        [TestCase(CodeEditorType.AvalonEdit, "closeTabsToTheRightToolStripMenuItem", 0)]
        [TestCase(CodeEditorType.TextEditor, "closeTabsToTheRightToolStripMenuItem", 0)]
        public void BulkCloseKeepsOnlyTheSelectedDocumentAndItsSaveTarget(CodeEditorType kind, string menuName, int selectedIndex)
        {
            var paths = OpenDocuments(kind);
            const string draft = "write(303);";
            _editor.SwitchTab(paths[selectedIndex]);
            _editor.Text = draft;

            ((ToolStripMenuItem)_tabs.ContextMenuStrip.Items[menuName]).PerformClick();

            AssertSelectedDocument(paths[selectedIndex], draft);
            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(2), "The surviving document and the new-tab button.");
            for (var i = 0; i < paths.Length; i++)
                NUnit.Framework.Assert.That(_editor.ContainsDocument(paths[i]), Is.EqualTo(i == selectedIndex));
            _editor.Save();
            AssertFileContents(paths, selectedIndex, draft);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void FailedOpenLeavesThePreviousTabBufferAndSaveDestinationUntouched(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            var blockedPath = Path.Combine(_directory, Guid.NewGuid().ToString("N") + ".tsl");
            File.WriteAllText(blockedPath, "do not overwrite this file");
            _editor.SwitchTab(paths[0]);
            _editor.Text = "recoverable draft";
            var tabCount = _tabs.TabCount;

            using (new FileStream(blockedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                NUnit.Framework.Assert.Throws<IOException>(() => _editor.NewDocument(blockedPath));
                AssertSelectedDocument(paths[0], "recoverable draft");
                NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(tabCount));
                NUnit.Framework.Assert.That(_editor.ContainsDocument(blockedPath), Is.False);
            }

            _editor.Save();
            AssertFileContents(paths, 0, "recoverable draft");
            NUnit.Framework.Assert.That(File.ReadAllText(blockedPath), Is.EqualTo("do not overwrite this file"));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void ReopeningCaseAndDotAliasesSelectsTheExistingUnsavedBuffer(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            var alias = Path.Combine(_directory, ".", Path.GetFileName(paths[0]).ToUpperInvariant());
            _editor.SwitchTab(paths[0]);
            _editor.Text = "one shared unsaved document";
            _editor.SwitchTab(paths[2]);
            var tabCount = _tabs.TabCount;

            _editor.NewDocument(alias);

            AssertSelectedDocument(paths[0], "one shared unsaved document");
            NUnit.Framework.Assert.That(_tabs.SelectedTab.Tag, Is.EqualTo(paths[0]));
            NUnit.Framework.Assert.That(_editor.ContainsDocument(alias), Is.True);
            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(tabCount));
            _editor.Save();
            AssertFileContents(paths, 0, "one shared unsaved document");
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void PublicDocumentSwitchAndCloseKeepTheTabAndBufferTogether(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            _editor.SwitchDocument(Path.Combine(_directory, ".", Path.GetFileName(paths[1]).ToUpperInvariant()));
            AssertSelectedDocument(paths[1], OriginalText[1]);
            _editor.Text = "public API draft";

            _editor.CloseDocument(Path.Combine(_directory, ".", Path.GetFileName(paths[0]).ToUpperInvariant()));

            AssertSelectedDocument(paths[1], "public API draft");
            NUnit.Framework.Assert.That(_editor.ContainsDocument(paths[0]), Is.False);
            NUnit.Framework.Assert.That(_tabs.TabPages.ContainsKey(paths[0]), Is.False);
            _editor.Save();
            AssertFileContents(paths, 1, "public API draft");
        }

        [TestCase(CodeEditorType.Scintilla, false)]
        [TestCase(CodeEditorType.AvalonEdit, false)]
        [TestCase(CodeEditorType.TextEditor, false)]
        [TestCase(CodeEditorType.Scintilla, true)]
        [TestCase(CodeEditorType.AvalonEdit, true)]
        [TestCase(CodeEditorType.TextEditor, true)]
        public void RenameCannotReplaceAnotherOpenBufferEvenIfItsFileWasDeleted(CodeEditorType kind, bool deleteTarget)
        {
            var paths = OpenDocuments(kind);
            _editor.SwitchTab(paths[1]);
            _editor.Text = "target unsaved";
            if (deleteTarget) File.Delete(paths[1]);
            _editor.SwitchTab(paths[0]);
            _editor.Text = "source unsaved";
            var alias = Path.Combine(_directory, ".", Path.GetFileName(paths[1]).ToUpperInvariant());

            NUnit.Framework.Assert.Throws<IOException>(() => _editor.RenameDocument(paths[0], alias));

            AssertSelectedDocument(paths[0], "source unsaved");
            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(4));
            _editor.SwitchTab(paths[1]);
            AssertSelectedDocument(paths[1], "target unsaved");
            NUnit.Framework.Assert.That(File.ReadAllText(paths[0]), Is.EqualTo(OriginalText[0]));
            NUnit.Framework.Assert.That(File.Exists(paths[1]), Is.EqualTo(!deleteTarget));
            if (!deleteTarget) NUnit.Framework.Assert.That(File.ReadAllText(paths[1]), Is.EqualTo(OriginalText[1]));
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void DirectoryRenameChecksOpenBuffersBeforeMovingAFile(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            _editor.SwitchTab(paths[1]);
            _editor.Text = "missing target unsaved";
            File.Delete(paths[1]);
            _editor.SwitchTab(paths[0]);
            _editor.Text = "source unsaved";
            using var tree = new DirectoryTree { CodeEditorWrapper = _editor };

            // Exercise the same filesystem operation invoked by AfterLabelEdit;
            // the handler's modal error-message presentation is tested manually.
            var rename = typeof(DirectoryTree).GetMethod("RenameFile", BindingFlags.Instance | BindingFlags.NonPublic);
            var error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() => rename.Invoke(tree, new object[] { paths[0], paths[1] }));

            NUnit.Framework.Assert.That(error.InnerException, Is.InstanceOf<IOException>());
            NUnit.Framework.Assert.That(File.ReadAllText(paths[0]), Is.EqualTo(OriginalText[0]));
            NUnit.Framework.Assert.That(File.Exists(paths[1]), Is.False);
            AssertSelectedDocument(paths[0], "source unsaved");
            _editor.SwitchTab(paths[1]);
            AssertSelectedDocument(paths[1], "missing target unsaved");
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void FailedDirectoryDeleteDoesNotCloseItsUnsavedBuffer(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            _editor.SwitchTab(paths[0]);
            _editor.Text = "preserve after failed deletion";
            using var tree = new DirectoryTree { CodeEditorWrapper = _editor, Path = _directory };
            var node = tree.Nodes[0].Nodes.Cast<TreeNode>().Single(item => item.FullPath == paths[0]);
            tree.SelectedNode = node;
            typeof(DirectoryTree).GetField("ctxNode", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tree, node);

            using (new FileStream(paths[0], FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                NUnit.Framework.Assert.Throws<IOException>(() => ((ToolStripMenuItem)tree.ContextMenuStrip.Items[2]).PerformClick());

            AssertSelectedDocument(paths[0], "preserve after failed deletion");
            NUnit.Framework.Assert.That(node.TreeView, Is.SameAs(tree));
            NUnit.Framework.Assert.That(File.ReadAllText(paths[0]), Is.EqualTo(OriginalText[0]));
            _editor.Save();
            AssertFileContents(paths, 0, "preserve after failed deletion");
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void RelativeOpenKeepsItsOriginalAbsoluteIdentityAfterWorkingDirectoryChanges(CodeEditorType kind)
        {
            OpenDocuments(kind);
            var previousDirectory = Environment.CurrentDirectory;
            var origin = Path.Combine(_directory, Guid.NewGuid().ToString("N"));
            var elsewhere = Path.Combine(_directory, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(origin);
            Directory.CreateDirectory(elsewhere);
            var originalPath = Path.Combine(origin, "relative.tsl");
            var otherPath = Path.Combine(elsewhere, "relative.tsl");
            File.WriteAllText(originalPath, "original file");
            File.WriteAllText(otherPath, "unrelated file");
            try
            {
                Environment.CurrentDirectory = origin;
                _editor.NewDocument("relative.tsl");
                _editor.Text = "relative file draft";
                Environment.CurrentDirectory = elsewhere;

                AssertSelectedDocument(originalPath, "relative file draft");
                _editor.Save();

                NUnit.Framework.Assert.That(File.ReadAllText(originalPath), Is.EqualTo("relative file draft"));
                NUnit.Framework.Assert.That(File.ReadAllText(otherPath), Is.EqualTo("unrelated file"));
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
            }
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void EditorChangesPreserveAllBuffersWithoutReadingLockedReplacedOrDeletedFiles(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            var drafts = new[] { "write(\"a*b α\"); // *\r\nwrite(3*4);", "write(\"β * γ\");\n", "write(3*5);\r\n" };
            for (var i = 0; i < paths.Length; i++)
            {
                _editor.SwitchTab(paths[i]);
                _editor.Text = drafts[i];
                NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(drafts[i]));
            }
            File.WriteAllText(paths[1], "external replacement");
            File.Delete(paths[2]);
            _editor.SwitchTab(paths[0]);

            using (new FileStream(paths[0], FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                foreach (var next in new[] { CodeEditorType.Scintilla, CodeEditorType.AvalonEdit, CodeEditorType.TextEditor, kind })
                {
                    Settings.Default.CodeEditor = next;
                    AssertSelectedDocument(paths[0], drafts[0]);
                    for (var i = 0; i < paths.Length; i++)
                    {
                        _editor.SwitchTab(paths[i]);
                        AssertSelectedDocument(paths[i], drafts[i]);
                    }
                    _editor.SwitchTab(paths[0]);
                }
            }

            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(4));
            NUnit.Framework.Assert.That(File.ReadAllText(paths[0]), Is.EqualTo(OriginalText[0]));
            NUnit.Framework.Assert.That(File.ReadAllText(paths[1]), Is.EqualTo("external replacement"));
            NUnit.Framework.Assert.That(File.Exists(paths[2]), Is.False);
        }

        [TestCase(CodeEditorType.Scintilla)]
        [TestCase(CodeEditorType.AvalonEdit)]
        [TestCase(CodeEditorType.TextEditor)]
        public void AmbiguousWindowsPathSpellingsCannotCreateOrRenameTabs(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            _editor.SwitchTab(paths[0]);
            _editor.Text = "unchanged draft";
            var tabCount = _tabs.TabCount;

            foreach (var suffix in new[] { ".", " ", ":stream" })
            {
                NUnit.Framework.Assert.Throws<IOException>(() => _editor.NewDocument(paths[0] + suffix));
                NUnit.Framework.Assert.Throws<IOException>(() => _editor.RenameDocument(paths[0], paths[0] + suffix));
                AssertSelectedDocument(paths[0], "unchanged draft");
                NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(tabCount));
            }
            AssertFileContents(paths, 0, OriginalText[0]);
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

            _editor.RenameDocument(Path.Combine(_directory, ".", Path.GetFileName(paths[0]).ToUpperInvariant()), renamed);
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
        public void CurrentFileNameRenameKeepsBufferIdentityAcrossEditorChanges(CodeEditorType kind)
        {
            var paths = OpenDocuments(kind);
            const string draft = "write(606);";
            var renamedPath = Path.Combine(_directory, Guid.NewGuid().ToString("N") + ".tslf");
            _editor.SwitchTab(paths[0]);
            _editor.Text = draft;

            // A directory rename moves the original file before changing its
            // editor identity. The unsaved buffer must survive this transition.
            File.Move(paths[0], renamedPath);
            _editor.CurrentFileName = renamedPath;
            _editor.CurrentFileName = renamedPath;
            NUnit.Framework.Assert.That(_editor.ContainsDocument(paths[0]), Is.False);
            NUnit.Framework.Assert.That(_editor.ContainsDocument(renamedPath), Is.True);
            NUnit.Framework.Assert.That(_tabs.TabPages.ContainsKey(renamedPath), Is.True);
            NUnit.Framework.Assert.That(_tabs.TabPages.ContainsKey(paths[0]), Is.False);

            File.WriteAllText(paths[0], OriginalText[0]);
            _editor.NewDocument(paths[0]);
            AssertSelectedDocument(paths[0], OriginalText[0]);
            _editor.NewDocument(renamedPath);
            AssertSelectedDocument(renamedPath, draft);
            NUnit.Framework.Assert.That(_tabs.TabCount, Is.EqualTo(5));

            Settings.Default.CodeEditor = kind == CodeEditorType.Scintilla
                ? CodeEditorType.AvalonEdit
                : CodeEditorType.Scintilla;
            AssertSelectedDocument(renamedPath, draft);
            _editor.Text = "write(707);";
            _editor.Save();
            NUnit.Framework.Assert.That(File.ReadAllText(renamedPath), Is.EqualTo("write(707);"));
            for (var i = 0; i < paths.Length; i++)
                NUnit.Framework.Assert.That(File.ReadAllText(paths[i]), Is.EqualTo(OriginalText[i]));
        }

        [Test]
        public void DisposedEditorDoesNotReactToSettingsChanges()
        {
            var originalKind = Settings.Default.CodeEditor;
            var visibleText = _editor.Text;
            var isolatedBootstrapper = new CoreBootstrapper();
            using (isolatedBootstrapper.Container)
            {
                var disposedEditor = isolatedBootstrapper.Create<CodeEditorControlWrapper>();
                try
                {
                    NUnit.Framework.Assert.That(disposedEditor.Handle, Is.Not.EqualTo(IntPtr.Zero));
                    disposedEditor.Dispose();
                    NUnit.Framework.Assert.That(disposedEditor.IsDisposed, Is.True);

                    Settings.Default.CodeEditor = originalKind == CodeEditorType.Scintilla
                        ? CodeEditorType.AvalonEdit
                        : CodeEditorType.Scintilla;
                    NUnit.Framework.Assert.That(_editor.Text, Is.EqualTo(visibleText));
                }
                finally
                {
                    disposedEditor.Dispose();
                    Settings.Default.CodeEditor = originalKind;
                }
            }
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
            NUnit.Framework.Assert.That(_tabs.SelectedTab.Name, Is.EqualTo(path));
            NUnit.Framework.Assert.That(_editor.CurrentFileName, Is.EqualTo(path));
            NUnit.Framework.Assert.That(_editor.ContainsDocument(path), Is.True);
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
