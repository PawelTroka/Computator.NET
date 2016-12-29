using System.Windows.Forms;
using Computator.NET.Controls.CodeEditors;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;

namespace Computator.NET.Views
{
    public partial class CustomFunctionsView : UserControl, ICustomFunctionsView
    {
        private CustomFunctionsView()
        {
            InitializeComponent();
        }

        public CustomFunctionsView(CodeEditorControlWrapper customFunctionsCodeEditor, SolutionExplorerView solutionExplorerView): this()
        {
            customFunctionsCodeEditor.Dock=DockStyle.Fill;
            solutionExplorerView.Dock=DockStyle.Fill;
            splitContainer3.Panel1.Controls.Add(customFunctionsCodeEditor);
            splitContainer3.Panel2.Controls.Add(solutionExplorerView);

            CustomFunctionsEditor = customFunctionsCodeEditor;
            SolutionExplorerView = solutionExplorerView;
        }

        public ISolutionExplorerView SolutionExplorerView { get; }

        public ICodeDocumentsEditor CustomFunctionsEditor { get; }
    }
}