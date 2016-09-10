using System.Windows.Forms;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class CustomFunctionsView : UserControl, ICustomFunctionsView
    {
        public CustomFunctionsView()
        {
            InitializeComponent();
        }

        public CustomFunctionsView(CodeEditorControlWrapper codeEditor, ISolutionExplorerView solutionExplorerView): this()
        {
            codeEditor.Dock=DockStyle.Fill;
            CustomFunctionsEditor = codeEditor;
            SolutionExplorerView = solutionExplorerView;
            splitContainer3.Panel1.Controls.Add(CustomFunctionsEditor as Control);
            splitContainer3.Panel2.Controls.Add(SolutionExplorerView as Control);
        }

        public ISolutionExplorerView SolutionExplorerView { get; }

        public ICodeDocumentsEditor CustomFunctionsEditor { get; }
    }
}