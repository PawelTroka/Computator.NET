using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Core.Model;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;

internal static partial class Program
{
    private static void DesktopCloneDocuments()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Computator-CloneTabs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using (var editor = new CloneTestEditor(new TextEditorCodeEditor(new SharedViewState())))
            using (var window = new Form())
            {
                window.Controls.Add(editor);
                window.Show();
                Application.DoEvents();
                var tabs = SearchDescendants(editor).OfType<DocumentsTabControl>().Single();
                var clone = (ToolStripMenuItem)tabs.ContextMenuStrip.Items["cloneTabToolStripMenuItem"];
                var originalPath = Path.Combine(directory, "original.tslf");
                File.WriteAllText(originalPath, "saved text");
                editor.NewDocument(originalPath);
                const string draft = "write(\"α * β\");\r\nwrite(2*3); // unsaved\n";
                editor.Text = draft;
                var originalTab = tabs.SelectedTab;
                var count = tabs.TabCount;

                clone.PerformClick();

                Check(tabs.TabCount == count + 1, "Clone tab must create a new document.");
                Check(tabs.SelectedTab != originalTab && tabs.SelectedTab.Tag == null,
                    "The clone must be untitled and must not inherit its source's file identity.");
                Equal(editor.Text, draft);
                Check(tabs.SelectedTab.ImageIndex == 1, "A nonempty clone must be dirty.");
                var clonedName = editor.CurrentFileName;
                editor.CloseDocument(clonedName);
                Check(editor.ContainsDocument(clonedName) && editor.ClosePrompts == 1,
                    "Cancel must retain the new unsaved clone.");
                editor.Text = "independent clone";
                editor.Save();
                Check(editor.SaveRequests == 1 && tabs.SelectedTab.Tag == null,
                    "Cancelling Save As must keep the clone untitled.");
                Equal(File.ReadAllText(originalPath), "saved text");
                editor.SavePath = Path.Combine(directory, "cloned.tslf");
                editor.Save();
                Check(editor.SaveRequests == 2, "Saving the clone requires its own destination.");
                Equal(File.ReadAllText(editor.SavePath), "independent clone");
                editor.SwitchTab(originalPath);
                Equal(editor.Text, draft);
                editor.Save();
                Equal(File.ReadAllText(originalPath), draft);
                Equal(File.ReadAllText(editor.SavePath), "independent clone");

                editor.NewDocument();
                var blank = editor.CurrentFileName;
                clone.PerformClick();
                var first = editor.CurrentFileName;
                Check(first != blank && tabs.SelectedTab.Tag == null && tabs.SelectedTab.ImageIndex == 0,
                    "An empty clone must have a distinct untitled document.");
                Equal(editor.Text, string.Empty);
                editor.Text = "first clone";
                clone.PerformClick();
                Check(editor.CurrentFileName != first && editor.CurrentFileName != blank,
                    "Repeated clones need unique names.");
                Equal(editor.Text, "first clone");
                editor.Text = "second clone";
                editor.SwitchTab(first);
                Equal(editor.Text, "first clone");
                editor.SwitchTab(blank);
                Equal(editor.Text, string.Empty);
            }
        }
        finally { Directory.Delete(directory, true); }
    }

    private sealed class CloneTestEditor : CodeEditorControlWrapper
    {
        internal string SavePath;
        internal int SaveRequests;
        internal int ClosePrompts;

        internal CloneTestEditor(TextEditorCodeEditor editor) : base(editor) { }

        protected override string SelectSaveFileName()
        {
            SaveRequests++;
            return SavePath;
        }

        protected override DialogResult PromptToSaveChanges(string filename)
        {
            ClosePrompts++;
            return DialogResult.Cancel;
        }
    }
}
