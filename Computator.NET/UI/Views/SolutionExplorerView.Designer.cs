using Computator.NET.UI.Controls;

namespace Computator.NET.UI.Views
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SolutionExplorerView));
            this.scriptingDirectoryTree = new DirectoryTree();
            this.openScriptingDirectoryButton = new System.Windows.Forms.Button();
            this.directoryBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // scriptingDirectoryTree
            // 
            this.scriptingDirectoryTree.CodeEditorWrapper = null;
            resources.ApplyResources(this.scriptingDirectoryTree, "scriptingDirectoryTree");
            this.scriptingDirectoryTree.Name = "scriptingDirectoryTree";
            this.scriptingDirectoryTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("scriptingDirectoryTree.Nodes")))});
            this.scriptingDirectoryTree.Path = null;
            // 
            // openScriptingDirectoryButton
            // 
            resources.ApplyResources(this.openScriptingDirectoryButton, "openScriptingDirectoryButton");
            this.openScriptingDirectoryButton.Name = "openScriptingDirectoryButton";
            this.openScriptingDirectoryButton.UseVisualStyleBackColor = true;
            // 
            // SolutionExplorerView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.scriptingDirectoryTree);
            this.Controls.Add(this.openScriptingDirectoryButton);
            this.Name = "SolutionExplorerView";
            resources.ApplyResources(this, "$this");
            this.ResumeLayout(false);

        }

        #endregion

        private DirectoryTree scriptingDirectoryTree;
        private System.Windows.Forms.Button openScriptingDirectoryButton;
        private System.Windows.Forms.FolderBrowserDialog directoryBrowserDialog;
    }
}
