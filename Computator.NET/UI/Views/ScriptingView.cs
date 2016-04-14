using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public partial class ScriptingView : UserControl, IScriptingView
    {
        public ScriptingView()
        {
            InitializeComponent();

            var codeEditor = new CodeEditorControlWrapper() { Dock = DockStyle.Fill };
            splitContainer2.Panel1.Controls.Add(codeEditor);
            DirectoryTree.CodeEditorWrapper = codeEditor;
            CodeEditorView = codeEditor;
            openScriptingDirectoryButton.Click +=
                (o, e) =>
                {
                    if (directoryBrowserDialog.ShowDialog(this) == DialogResult.OK)
                        DirectoryChanged?.Invoke(this,
                            new DirectorySelectedEventArgs(directoryBrowserDialog.SelectedPath));
                };

            if(!DesignMode)
                consoleOutputTextBox.Font = CustomFonts.GetMathFont(consoleOutputTextBox.Font.Size);
        }

        private FolderBrowserDialog directoryBrowserDialog = new FolderBrowserDialog { ShowNewFolderButton = true };
        public ICodeEditorView CodeEditorView { get; }
        public IDirectoryTree DirectoryTree { get { return scriptingDirectoryTree; } }
        public event EventHandler ProcessClicked
        {
            add { processButton.Click += value; }
            remove { processButton.Click -= value; }
        }

        public event DirectoryTree.DirectorySelectedDelegate DirectoryChanged;

        public string ConsoleOutput
        {
            set
            {
                consoleOutputTextBox.Text = value;
            }
        }

        public void AppendToConsole(string output)
        {
            consoleOutputTextBox.AppendText(output);
        }
    }
}
