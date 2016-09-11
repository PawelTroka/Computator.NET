using System.Windows.Forms;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class CustomFunctionsView : UserControl, ICustomFunctionsView
    {
        private CustomFunctionsView()
        {
            InitializeComponent();
        }

        public CustomFunctionsView(CodeEditorControlWrapper codeEditor, SolutionExplorerView solutionExplorerView): this()
        {
            codeEditor.Dock=DockStyle.Fill;
            solutionExplorerView.Dock=DockStyle.Fill;
            splitContainer3.Panel1.Controls.Add(codeEditor);
            splitContainer3.Panel2.Controls.Add(solutionExplorerView);

            CustomFunctionsEditor = codeEditor;
            SolutionExplorerView = solutionExplorerView;
        }

        public ISolutionExplorerView SolutionExplorerView { get; }

        public ICodeDocumentsEditor CustomFunctionsEditor { get; }
    }
}