using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using Settings = Computator.NET.Core.Properties.Settings;

internal static partial class Program
{
    private static void ShutdownCleanRepeatedLifetimes()
    {
        for (var attempt = 0; attempt < 3; attempt++)
            WithShutdownWindow((window, scripts, functions, directory) =>
            {
                window.Close();
                Check(scripts.PromptedFiles.Count == 0 && functions.PromptedFiles.Count == 0,
                    "A clean desktop must close without save prompts.");
            });
    }

    private static void ShutdownUnsavedDocuments(DialogResult decision)
    {
        WithShutdownWindow((window, scripts, functions, directory) =>
        {
            var scriptPath = OpenShutdownDocument(scripts, directory, "script.tsl", "script disk", "script draft");
            var functionPath = OpenShutdownDocument(functions, directory, "functions.tslf", "function disk", "function draft");
            scripts.Decisions.Enqueue(decision);
            functions.Decisions.Enqueue(decision);
            window.Close();
            Equal(File.ReadAllText(scriptPath), decision == DialogResult.Yes ? "script draft" : "script disk");
            Equal(File.ReadAllText(functionPath), decision == DialogResult.Yes ? "function draft" : "function disk");
            Check(scripts.PromptedFiles.SequenceEqual(new[] { scriptPath }) &&
                functions.PromptedFiles.SequenceEqual(new[] { functionPath }),
                "Closing the desktop must obtain a decision for each editor's dirty document.");
        });
    }

    private static void ShutdownCancellationPreservesBothEditors()
    {
        WithShutdownWindow((window, scripts, functions, directory) =>
        {
            var scriptPath = OpenShutdownDocument(scripts, directory, "script.tsl", "script disk", "script draft");
            var functionPath = OpenShutdownDocument(functions, directory, "functions.tslf", "function disk", "function draft");
            var scriptTabs = scripts.Tabs.TabCount;
            var functionTabs = functions.Tabs.TabCount;
            scripts.Decisions.Enqueue(DialogResult.No);
            functions.Decisions.Enqueue(DialogResult.Cancel);
            window.Close();
            Check(!window.IsDisposed && window.Visible && !scripts.IsDisposed && !functions.IsDisposed,
                "Cancelling the second editor must leave the entire desktop usable.");
            AssertShutdownDraft(scripts, scriptPath, "script draft", scriptTabs);
            AssertShutdownDraft(functions, functionPath, "function draft", functionTabs);
            Equal(File.ReadAllText(scriptPath), "script disk");
            Equal(File.ReadAllText(functionPath), "function disk");

            // A discarded approval from the first attempt must not survive the
            // cancelled exit. The live editor accepts another edit and saves it.
            scripts.Text = "revised script draft";
            scripts.Decisions.Enqueue(DialogResult.Yes);
            functions.Decisions.Enqueue(DialogResult.No);
            window.Close();
            Equal(File.ReadAllText(scriptPath), "revised script draft");
            Equal(File.ReadAllText(functionPath), "function disk");
            Check(scripts.PromptedFiles.Count == 2 && functions.PromptedFiles.Count == 2,
                "Retrying a cancelled close must request fresh approvals from both editors.");
        });
    }

    private static void ShutdownCancelledSaveAsPreservesUntitledDraft()
    {
        WithShutdownWindow((window, scripts, functions, directory) =>
        {
            var untitled = scripts.CurrentFileName;
            scripts.Text = "untitled script draft";
            var scriptTabs = scripts.Tabs.TabCount;
            var functionPath = OpenShutdownDocument(functions, directory, "functions.tslf", "function disk", "function draft");
            scripts.Decisions.Enqueue(DialogResult.Yes);
            scripts.SavePath = null;
            window.Close();
            Check(!window.IsDisposed && window.Visible, "Cancelling Save As must cancel application exit.");
            AssertShutdownDraft(scripts, untitled, "untitled script draft", scriptTabs);
            Check(scripts.Tabs.SelectedTab.Tag == null && scripts.SaveRequests == 1,
                "A cancelled Save As must retain the untitled identity.");
            Equal(functions.Text, "function draft");
            Equal(File.ReadAllText(functionPath), "function disk");
            Check(functions.PromptedFiles.Count == 0 && Directory.GetFiles(directory).Length == 1,
                "Cancelled Save As must not write a file or proceed to the other editor.");

            scripts.SavePath = Path.Combine(directory, "saved-script.tsl");
            scripts.Decisions.Enqueue(DialogResult.Yes);
            functions.Decisions.Enqueue(DialogResult.No);
            window.Close();
            Equal(File.ReadAllText(scripts.SavePath), "untitled script draft");
            Equal(File.ReadAllText(functionPath), "function disk");
        });
    }

    private static void ShutdownFailedSaveAsPreservesBothDocuments()
    {
        WithShutdownWindow((window, scripts, functions, directory) =>
        {
            var untitled = scripts.CurrentFileName;
            scripts.Text = "untitled script draft";
            var scriptTabs = scripts.Tabs.TabCount;
            var functionPath = OpenShutdownDocument(functions, directory, "functions.tslf", "function disk", "function draft");
            var functionTabs = functions.Tabs.TabCount;
            scripts.SavePath = functionPath;
            scripts.Decisions.Enqueue(DialogResult.Yes);
            window.Close();
            Check(!window.IsDisposed && window.Visible, "A failed Save As must cancel application exit.");
            AssertShutdownDraft(scripts, untitled, "untitled script draft", scriptTabs);
            AssertShutdownDraft(functions, functionPath, "function draft", functionTabs);
            Check(scripts.SaveErrors.Count == 1 && scripts.Tabs.SelectedTab.Tag == null,
                "A rejected cross-editor overwrite must report the failure and keep the untitled identity.");
            Equal(File.ReadAllText(functionPath), "function disk");
            Check(functions.PromptedFiles.Count == 0, "A failed save must stop the close workflow.");

            scripts.SavePath = Path.Combine(directory, "saved-script.tsl");
            scripts.Decisions.Enqueue(DialogResult.Yes);
            functions.Decisions.Enqueue(DialogResult.Yes);
            window.Close();
            Equal(File.ReadAllText(scripts.SavePath), "untitled script draft");
            Equal(File.ReadAllText(functionPath), "function draft");
        });
    }

    private static string OpenShutdownDocument(ShutdownEditor editor, string directory, string name,
        string diskText, string draft)
    {
        var path = Path.Combine(directory, name);
        File.WriteAllText(path, diskText);
        editor.NewDocument(path);
        editor.Text = draft;
        return path;
    }

    private static void AssertShutdownDraft(ShutdownEditor editor, string name, string text, int tabs)
    {
        Check(editor.ContainsDocument(name) && editor.CurrentFileName == name && editor.Tabs.TabCount == tabs,
            "Cancelled shutdown must preserve every document and the selected tab.");
        Equal(editor.Text, text);
        Check(editor.Tabs.SelectedTab.ImageIndex == 1, "Cancelled shutdown must retain the unsaved-change indicator.");
    }

    private static void WithShutdownWindow(Action<MainView, ShutdownEditor, ShutdownEditor, string> scenario)
    {
        GSLInitializer.Initialize();
        var previousEditor = Settings.Default.CodeEditor;
        var directory = Path.Combine(Path.GetTempPath(), "computator-shutdown-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        Settings.Default.CodeEditor = CodeEditorType.TextEditor;
        try
        {
            var bootstrapper = new WinFormsBootstrapper();
            using (bootstrapper.Container)
            using (var scripts = new ShutdownEditor(bootstrapper.Create<TextEditorCodeEditor>()))
            using (var functions = new ShutdownEditor(bootstrapper.Create<TextEditorCodeEditor>()))
            using (var window = new MainView(bootstrapper.Create<MenuStripView>(), bootstrapper.Create<ToolBarView>(),
                bootstrapper.Create<CalculationsView>(), bootstrapper.Create<NumericalCalculationsView>(),
                new ScriptingView(scripts, new SolutionExplorerView()),
                new CustomFunctionsView(functions, new SolutionExplorerView()),
                bootstrapper.Create<ChartingView>(), bootstrapper.Create<ExpressionView>()))
            {
                Exception failure = null;
                var attempted = false;
                window.Shown += (sender, args) => window.BeginInvoke((Action)(() =>
                {
                    attempted = true;
                    try
                    {
                        scenario(window, scripts, functions, directory);
                        Check(window.IsDisposed && scripts.IsDisposed && functions.IsDisposed,
                            "The approved close must dispose the main window and both editors.");
                        Check(scripts.Decisions.Count == 0 && functions.Decisions.Count == 0,
                            "The close workflow did not consume the expected save/discard decisions.");
                        Check(!Application.OpenForms.Cast<Form>().Contains(window),
                            "The closed main window must leave Application.OpenForms.");
                    }
                    catch (Exception error)
                    {
                        failure = error;
                        // Only failure cleanup exits the thread directly. A pass
                        // must return through MainView.Close and Application.Run.
                        Application.ExitThread();
                    }
                }));
                Application.Run(window);
                if (failure != null) throw new InvalidOperationException("Mono shutdown workflow failed.", failure);
                Check(attempted, "The shutdown scenario never reached the live message loop.");
            }
        }
        finally
        {
            Settings.Default.CodeEditor = previousEditor;
            Directory.Delete(directory, true);
        }
    }

    private sealed class ShutdownEditor : CodeEditorControlWrapper
    {
        internal readonly Queue<DialogResult> Decisions = new Queue<DialogResult>();
        internal readonly List<string> PromptedFiles = new List<string>();
        internal readonly List<string> SaveErrors = new List<string>();
        internal string SavePath;
        internal int SaveRequests;
        internal DocumentsTabControl Tabs => SearchDescendants(this).OfType<DocumentsTabControl>().Single();

        internal ShutdownEditor(TextEditorCodeEditor editor) : base(editor) { }

        protected override DialogResult PromptToSaveChanges(string filename)
        {
            PromptedFiles.Add(filename);
            Check(Decisions.Count != 0, "Unexpected unsaved-document prompt for " + filename);
            return Decisions.Dequeue();
        }

        protected override string SelectSaveFileName() { SaveRequests++; return SavePath; }
        protected override void ShowCloseSaveError(string message) { SaveErrors.Add(message); }
    }
}
