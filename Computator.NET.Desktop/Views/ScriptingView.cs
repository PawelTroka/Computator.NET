using System;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.DataTypes.Text;
using Computator.NET.Desktop.Controls.CodeEditors;

namespace Computator.NET.Desktop.Views
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
            //scriptingCodeEditor.Anchor=AnchorStyles.Top| AnchorStyles.Bottom | AnchorStyles.Left| AnchorStyles.Right;
            solutionExplorerView1.Dock = DockStyle.Fill;
            //solutionExplorerView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            splitContainer2.Panel1.Controls.Add(scriptingCodeEditor);
            //splitContainer2.Panel1.Controls[0].BringToFront();

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