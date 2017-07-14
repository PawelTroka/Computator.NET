using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Model;

namespace Computator.NET.Views
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

        public event DirectorySelectedDelegate DirectoryChanged;
    }
}