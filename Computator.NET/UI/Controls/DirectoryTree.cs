using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Computator.NET.DataTypes.Localization;
using Computator.NET.Localization;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET
{
    public enum ButtonClick { None, New, Delete, Rename }

    public class DirectoryTree : TreeView
    {
        public CodeEditorControlWrapper CodeEditorWrapper { get; set; }
        private int id = 1;
        //private ContextMenu ctxMenu;
        //private ButtonClick _buttonClicked;

        private TreeNode ctxNode;


        public DirectoryTree()
        {
            InitializeComponent();
            AfterSelect += _AfterSelect;
            ContextMenu = new ContextMenu(new MenuItem[] {
            new MenuItem(Strings.DirectoryTree_DirectoryTree_New_file, (o, e) =>
            {
                var attr = File.GetAttributes(ctxNode.FullPath);

                TreeNode newNode = null;
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    newNode = ctxNode.Nodes.Add(Strings.DirectoryTree_DirectoryTree_New_file+" " + id);

                }
                else
                {
                    newNode = ctxNode.Parent.Nodes.Add(Strings.DirectoryTree_DirectoryTree_New_file+" " + id);
                }
                id++;

                this.LabelEdit = true;
                if(!newNode.IsEditing)
                {
                    newNode.BeginEdit();
                }
                //CodeEditorWrapper?.NewDocument();
                //RefreshDisplay();
                //_buttonClicked = ButtonClick.New;
            }),
            new MenuItem(Strings.DirectoryTree_DirectoryTree_Rename_file, (o, e) =>
            {
                //oldPath = ctxNode.FullPath;
                if (ctxNode == TopNode)
                    return;
                this.LabelEdit = true;

                if (!ctxNode.IsEditing)
                {
                    ctxNode.BeginEdit();
                    //ctxNode.EndEdit(true);
                   /* if (oldPath != ctxNode.FullPath)
                    {
                        System.IO.File.Move(oldPath, ctxNode.FullPath);
                        CodeEditorWrapper?.RenameDocument(oldPath, ctxNode.FullPath);
                    }*/
                    //RefreshDisplay();
                    //_buttonClicked = ButtonClick.Rename;
                }
            }),
            new MenuItem(Strings.DirectoryTree_DirectoryTree_Delete_file, (o, e) =>
            {
                if (ctxNode == TopNode)
                    return;
                CodeEditorWrapper?.RemoveTab(ctxNode.FullPath);
                File.Delete(ctxNode.FullPath);
                Nodes.Remove(ctxNode);
                SelectedNode = TopNode;
                //RefreshDisplay();
                //_buttonClicked =  ButtonClick.Delete;
            })
            });

            NodeMouseClick += DirectoryTree_NodeMouseDoubleClick;
            //NodeMouseDoubleClick += DirectoryTree_NodeMouseDoubleClick;
            AfterLabelEdit += treeView1_AfterLabelEdit;
        }

        private void treeView1_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
        {
            if (CodeEditorWrapper == null)
                return;
            //MessageBox.Show(e.Label);
            //MessageBox.Show(e.Node.Text);
            if (e.Label == null && System.IO.File.Exists(e.Node.FullPath))
            {
                createFile(e.Node.FullPath);
                e.Node.EndEdit(false);
                return;
            }
            if (e.Label == null)
                return;

            if (e.Label.Length > 0)
            {
                if (e.Label.IndexOfAny(new char[] { '/', '\\', ':', '*', '<', '>', '|', '?', '"' }) == -1)
                {
                    // Stop editing without canceling the label change.


                    if (e.Node.Text != e.Label)
                    {
                        var oldPath = e.Node.FullPath;
                        e.Node.Text = e.Label;
                        if (oldPath != e.Node.FullPath || !System.IO.File.Exists(oldPath))
                        {
                            File.Delete(e.Node.FullPath);
                            //if(File.Exists(oldPath))


                            var containsDocument = CodeEditorWrapper.ContainsDocument(oldPath);
                            if (containsDocument)
                            {
                                System.IO.File.Move(oldPath, e.Node.FullPath);
                                CodeEditorWrapper?.RenameDocument(oldPath, e.Node.FullPath);
                            }
                            else
                            {
                                createFile(e.Node.FullPath);
                            }
                        }
                    }

                    e.Node.EndEdit(false);
                }
                else
                {
                    // Cancel the label edit action, inform the user, and 
                    //  place the node in edit mode again.
                    e.CancelEdit = true;
                    MessageBox.Show(Strings.DirectoryTree_treeView1_AfterLabelEdit_ +
                       Strings.DirectoryTree_treeView1_AfterLabelEdit_The_invalid_characters,
                       Strings.DirectoryTree_treeView1_AfterLabelEdit_Node_Label_Edit);
                    e.Node.BeginEdit();
                }
            }
            else
            {
                /* Cancel the label edit action, inform the user, and 
                       place the node in edit mode again. */
                e.CancelEdit = true;
                MessageBox.Show(Strings.DirectoryTree_treeView1_AfterLabelEdit_+Strings.DirectoryTree_treeView1_AfterLabelEdit_The_label_cannot_be_blank
                    ,
                    Strings.DirectoryTree_treeView1_AfterLabelEdit_Node_Label_Edit);
                e.Node.BeginEdit();
            }
        }

        private void createFile(string fullPath)
        {
            var sr = System.IO.File.CreateText(fullPath);
            sr.Close();
            CodeEditorWrapper.NewDocument(fullPath);
        }

        private void DirectoryTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //ContextMenu.Show();
            if (e.Button == MouseButtons.Right)
            {
                ctxNode = e.Node;
                //ctxMenu.Show(this,e.Location);
            }
        }

        private void InitializeComponent()
        {
            Font = new Font(FontFamily.GenericSansSerif, 9.0F, FontStyle.Regular, GraphicsUnit.Point);
        }

        private void _AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (CodeEditorWrapper == null) return;

            if (CodeEditorWrapper.ContainsDocument(this.SelectedNode.FullPath))
            {
                if (CodeEditorWrapper.CurrentFileName != this.SelectedNode.FullPath)
                {
                    CodeEditorWrapper.SwitchTab(this.SelectedNode.FullPath);
                    //CodeEditorWrapper.CurrentFileName = this.SelectedNode.FullPath;
                    //CodeEditorWrapper.SwitchDocument(this.SelectedNode.FullPath);
                }
            }
            else if (File.Exists(this.SelectedNode.FullPath))
            {
                CodeEditorWrapper.NewDocument(this.SelectedNode.FullPath);
            }
        }

        public delegate void DirectorySelectedDelegate(object sender, DirectorySelectedEventArgs e);

        private string _path;

        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                if (_path != null)
                    RefreshDisplay();
            }
        }

        public event DirectorySelectedDelegate DirectorySelected;
        // This is public so a Refresh can be triggered manually.
        public void RefreshDisplay()
        {
            // Erase the existing tree.
            Nodes.Clear();

            // Set the first node.
            var rootNode = new TreeNode(_path);
            Nodes.Add(rootNode);

            // Fill the first level and expand it.

            Fill(rootNode);
            Nodes[0].Expand();
        }

        private void Fill(TreeNode dirNode)
        {
            var dir = new DirectoryInfo(dirNode.FullPath);

            // An exception could be thrown in this code if you don't
            // have sufficient security permissions for a file or directory.
            // You can catch and then ignore this exception.

            foreach (var dirItem in dir.GetDirectories())
            {
                // Add node for the directory.
                var newNode = new TreeNode(dirItem.Name);

                dirNode.Nodes.Add(newNode);
                newNode.Nodes.Add("*");
            }
            foreach (var dirItem in dir.GetFiles())
            {
                // Add node for the directory.
                var newNode = new TreeNode(dirItem.Name);
                dirNode.Nodes.Add(newNode);
                //newNode.Nodes.Add("*");
            }
        }

        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            base.OnBeforeExpand(e);

            // If a dummy node is found, remove it and read the real directory list.
            // ReSharper disable once LocalizableElement
            if (e.Node.Nodes[0].Text == "*")
            {
                e.Node.Nodes.Clear();
                Fill(e.Node);
            }
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);

            // Raise the DirectorySelected event.
            if (DirectorySelected != null)
            {
                DirectorySelected(this,
                    new DirectorySelectedEventArgs(e.Node.FullPath));
            }
        }
    }

    public class DirectorySelectedEventArgs : EventArgs
    {
        public string DirectoryName;

        public DirectorySelectedEventArgs(string directoryName)
        {
            DirectoryName = directoryName;
        }
    }
}