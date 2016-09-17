using System;
using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class ScriptingView : UserControl, IScriptingView
    {
        private ScriptingView()
        {
            InitializeComponent();
            if (!DesignMode)
                consoleOutputTextBox.Font = CustomFonts.GetMathFont(consoleOutputTextBox.Font.Size);
        }
        public ScriptingView(CodeEditorControlWrapper scriptingCodeEditor, SolutionExplorerView solutionExplorerView1) : this()
        {
            scriptingCodeEditor.Dock=DockStyle.Fill;
            splitContainer2.Panel1.Controls.Add(scriptingCodeEditor);
            splitContainer2.Panel1.Controls[0].Dock = DockStyle.Fill;

            solutionExplorerView1.Dock=DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(solutionExplorerView1);
            splitContainer1.Panel2.Controls[1].BringToFront();

            CodeEditorView = scriptingCodeEditor;
            SolutionExplorerView = solutionExplorerView1;
        }

        public ICodeDocumentsEditor CodeEditorView { get; }
        public ISolutionExplorerView SolutionExplorerView { get; }

        public event EventHandler ProcessClicked
        {
            add { processButton.Click += value; }
            remove { processButton.Click -= value; }
        }


        public string ConsoleOutput
        {
            set { consoleOutputTextBox.Text = value; }
        }

        public void AppendToConsole(string output)
        {
            consoleOutputTextBox.AppendText(output);
        }
    }
}