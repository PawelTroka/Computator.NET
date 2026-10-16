using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.Localization;

namespace Computator.NET.Desktop.Controls
{
    public sealed class DirectoryTree : TreeView, IDirectoryTree
    {
        private string _path;
        private TreeNode ctxNode;
        private int id = 1;
        private readonly ContextMenuStrip _fileMenu;

        public DirectoryTree()
        {
            Font = new Font(FontFamily.GenericSansSerif, 9.0F, FontStyle.Regular, GraphicsUnit.Point);
            AfterSelect += _AfterSelect;
            _fileMenu = new ContextMenuStrip();
            _fileMenu.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem(Strings.Directory_tree_New_file, null, (o, e) =>
                {
                    if (ctxNode == null || ctxNode.TreeView != this) return;
                    var attr = File.GetAttributes(ctxNode.FullPath);
                    var parent = attr.HasFlag(FileAttributes.Directory) ? ctxNode : ctxNode.Parent;
                    if (parent == null) return;
                    var newNode = parent.Nodes.Add(Strings.Directory_tree_New_file + " " + id++);
                    LabelEdit = true;
                    newNode.BeginEdit();
                }),
                new ToolStripMenuItem(Strings.Rename_file, null, (o, e) =>
                {
                    // TopNode is the first visible node, not necessarily the root.
                    if (ctxNode?.Parent == null || ctxNode.TreeView != this) return;
                    LabelEdit = true;
                    if (!ctxNode.IsEditing) ctxNode.BeginEdit();
                }),
                new ToolStripMenuItem(Strings.Delete_file, null, (o, e) =>
                {
                    if (ctxNode?.Parent == null || ctxNode.TreeView != this || Directory.Exists(ctxNode.FullPath)) return;
                    File.Delete(ctxNode.FullPath);
                    // A failed deletion must leave its unsaved editor buffer open.
                    CodeEditorWrapper?.RemoveTab(ctxNode.FullPath);
                    ctxNode.Remove();
                    SelectedNode = Nodes.Count > 0 ? Nodes[0] : null;
                    ctxNode = SelectedNode;
                })
            });
            _fileMenu.Opening += (sender, args) =>
            {
                ctxNode = SelectedNode ?? ctxNode;
                if (ctxNode == null || ctxNode.TreeView != this) { args.Cancel = true; return; }
                _fileMenu.Items[1].Enabled = ctxNode.Parent != null;
                _fileMenu.Items[2].Enabled = ctxNode.Parent != null && !Directory.Exists(ctxNode.FullPath);
            };
            ContextMenuStrip = _fileMenu;
            NodeMouseClick += (sender, args) =>
            {
                if (args.Button != MouseButtons.Right) return;
                ctxNode = args.Node;
                SelectedNode = args.Node;
            };
            AfterLabelEdit += treeView1_AfterLabelEdit;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _fileMenu?.Dispose();
            base.Dispose(disposing);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDocumentsEditor CodeEditorWrapper { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                if (_path != null) RefreshDisplay();
            }
        }

        public event DirectorySelectedDelegate DirectorySelected;

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (CodeEditorWrapper == null) return;
            if (e.Label == null && !File.Exists(e.Node.FullPath) && !Directory.Exists(e.Node.FullPath))
            {
                createFile(e.Node.FullPath);
                return;
            }
            if (e.Label == null) return;
            if (string.IsNullOrWhiteSpace(e.Label) || e.Label == "." || e.Label == ".." ||
                e.Label.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                e.CancelEdit = true;
                MessageBox.Show(Strings.InvalidLabel + Strings.The_invalid_characters_are, Strings.Label_Edit_error);
                return;
            }
            if (e.Node.Parent == null) { e.CancelEdit = true; return; }
            var oldPath = e.Node.FullPath;
            var newPath = System.IO.Path.Combine(e.Node.Parent.FullPath, e.Label);
            if (string.Equals(oldPath, newPath, StringComparison.Ordinal)) return;
            try
            {
                // Never delete a different document to make a rename succeed.
                if (Directory.Exists(oldPath)) Directory.Move(oldPath, newPath);
                else if (File.Exists(oldPath))
                    RenameFile(oldPath, newPath);
                else createFile(newPath);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                e.CancelEdit = true;
                MessageBox.Show(exception.Message, Strings.Label_Edit_error);
            }
        }

        private void RenameFile(string oldPath, string newPath)
        {
            // The destination may be open with unsaved changes even if
            // another process has deleted its file from disk.
            CodeEditorWrapper.EnsureCanRenameDocument(oldPath, newPath);
            File.Move(oldPath, newPath);
            if (CodeEditorWrapper.ContainsDocument(oldPath))
                CodeEditorWrapper.RenameDocument(oldPath, newPath);
        }

        private void createFile(string fullPath)
        {
            using (new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write)) { }
            CodeEditorWrapper.NewDocument(fullPath);
        }

        private void _AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (CodeEditorWrapper == null || e.Node == null) return;
            if (CodeEditorWrapper.ContainsDocument(e.Node.FullPath))
            {
                if (CodeEditorWrapper.CurrentFileName != e.Node.FullPath)
                    CodeEditorWrapper.SwitchTab(e.Node.FullPath);
            }
            else if (File.Exists(e.Node.FullPath)) CodeEditorWrapper.NewDocument(e.Node.FullPath);
        }

        public void RefreshDisplay()
        {
            BeginUpdate();
            try
            {
                ctxNode = null;
                Nodes.Clear();
                if (string.IsNullOrEmpty(_path)) return;
                var rootNode = new TreeNode(_path);
                Nodes.Add(rootNode);
                Fill(rootNode);
                ctxNode = rootNode;
                rootNode.Expand();
            }
            finally { EndUpdate(); }
        }

        private void Fill(TreeNode dirNode)
        {
            var dir = new DirectoryInfo(dirNode.FullPath);
            foreach (var dirItem in dir.GetDirectories())
            {
                var newNode = dirNode.Nodes.Add(dirItem.Name);
                newNode.Nodes.Add("*");
            }
            foreach (var dirItem in dir.GetFiles()) dirNode.Nodes.Add(dirItem.Name);
        }

        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            base.OnBeforeExpand(e);
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "*")
            {
                e.Node.Nodes.Clear();
                Fill(e.Node);
            }
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);
            if (e.Node != null)
                DirectorySelected?.Invoke(this, new DirectorySelectedEventArgs(e.Node.FullPath));
        }
    }
}
