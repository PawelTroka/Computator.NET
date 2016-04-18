using System.Windows.Forms;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public partial class CustomFunctionsView : UserControl, ICustomFunctionsView
    {
        public CustomFunctionsView()
        {
            InitializeComponent();
            splitContainer3.Panel1.Controls.Add(CustomFunctionsEditor as Control);
            splitContainer3.Panel2.Controls.Add(SolutionExplorerView as Control);
        }

        public ISolutionExplorerView SolutionExplorerView { get; } = new SolutionExplorerView {Dock = DockStyle.Fill};

        public ICodeDocumentsEditor CustomFunctionsEditor { get; } = new CodeEditorControlWrapper
        {
            Dock = DockStyle.Fill
        };
    }
}