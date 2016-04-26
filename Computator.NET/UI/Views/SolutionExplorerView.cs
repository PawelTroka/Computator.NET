using System.Windows.Forms;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Views
{
    public partial class SolutionExplorerView : UserControl, ISolutionExplorerView
    {
        public SolutionExplorerView()
        {
            InitializeComponent();

            openScriptingDirectoryButton.Click +=
                (o, e) =>
                {
                    if (directoryBrowserDialog.ShowDialog(this) == DialogResult.OK)
                        DirectoryChanged?.Invoke(this,
                            new DirectorySelectedEventArgs(directoryBrowserDialog.SelectedPath));
                };
        }

        public IDirectoryTree DirectoryTree
        {
            get { return scriptingDirectoryTree; }
        }

        public event DirectoryTree.DirectorySelectedDelegate DirectoryChanged;
    }
}