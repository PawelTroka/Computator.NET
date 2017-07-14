using Computator.NET.Desktop.Controls;

namespace Computator.NET.Desktop.Views
{
    partial class SolutionExplorerView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("_Scripts");
            this.openScriptingDirectoryButton = new System.Windows.Forms.Button();
            this.directoryBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.scriptingDirectoryTree = new DirectoryTree();
            this.SuspendLayout();
            // 
            // openScriptingDirectoryButton
            // 
            this.openScriptingDirectoryButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.openScriptingDirectoryButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F);
            this.openScriptingDirectoryButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.openScriptingDirectoryButton.Location = new System.Drawing.Point(0, 0);
            this.openScriptingDirectoryButton.Margin = new System.Windows.Forms.Padding(2);
            this.openScriptingDirectoryButton.Name = "openScriptingDirectoryButton";
            this.openScriptingDirectoryButton.Size = new System.Drawing.Size(182, 46);
            this.openScriptingDirectoryButton.TabIndex = 5;
            this.openScriptingDirectoryButton.Text = Localization.Views.SolutionExplorerView.openScriptingDirectoryButton_Text;
            this.openScriptingDirectoryButton.UseVisualStyleBackColor = true;
            // 
            // scriptingDirectoryTree
            // 
            this.scriptingDirectoryTree.CodeEditorWrapper = null;
            this.scriptingDirectoryTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptingDirectoryTree.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scriptingDirectoryTree.Location = new System.Drawing.Point(0, 46);
            this.scriptingDirectoryTree.Margin = new System.Windows.Forms.Padding(2);
            this.scriptingDirectoryTree.Name = "scriptingDirectoryTree";
            treeNode1.Name = "";
            treeNode1.Text = "_Scripts";
            this.scriptingDirectoryTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.scriptingDirectoryTree.Path = null;
            this.scriptingDirectoryTree.Size = new System.Drawing.Size(182, 350);
            this.scriptingDirectoryTree.TabIndex = 6;
            // 
            // SolutionExplorerView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.scriptingDirectoryTree);
            this.Controls.Add(this.openScriptingDirectoryButton);
            this.Name = "SolutionExplorerView";
            this.Size = new System.Drawing.Size(182, 396);
            this.ResumeLayout(false);

        }

        #endregion

        private DirectoryTree scriptingDirectoryTree;
        private System.Windows.Forms.Button openScriptingDirectoryButton;
        private System.Windows.Forms.FolderBrowserDialog directoryBrowserDialog;
    }
}
