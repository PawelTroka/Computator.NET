using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Computator.NET.UI.Views
{
    public partial class SolutionExplorerView : UserControl, ISolutionExplorerView
    {
        public IDirectoryTree DirectoryTree { get { return scriptingDirectoryTree; } }

        public event DirectoryTree.DirectorySelectedDelegate DirectoryChanged;


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
    }
}
