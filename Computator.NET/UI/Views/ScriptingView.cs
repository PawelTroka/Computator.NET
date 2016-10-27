using System;
using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class ScriptingView : UserControl, IScriptingView
    {
        public ScriptingView()
        {
            InitializeComponent();

            var codeEditor = new CodeEditorControlWrapper {Dock = DockStyle.Fill};
            splitContainer2.Panel1.Controls.Add(codeEditor);
            splitContainer1.Panel2.Controls.Add(SolutionExplorerView as Control);
            splitContainer1.Panel2.Controls[1].BringToFront();
            CodeEditorView = codeEditor;


            if (!DesignMode)
                consoleOutputTextBox.Font = CustomFonts.GetMathFont(consoleOutputTextBox.Font.Size);
        }

        public ICodeDocumentsEditor CodeEditorView { get; }
        public ISolutionExplorerView SolutionExplorerView { get; } = new SolutionExplorerView {Dock = DockStyle.Fill};

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