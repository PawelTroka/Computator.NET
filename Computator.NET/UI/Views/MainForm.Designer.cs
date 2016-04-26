namespace Computator.NET.UI.Views
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.XYRatioToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.modeToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.dd212ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fdsfdsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mode3DFxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.chartingTabPage = new System.Windows.Forms.TabPage();
            this.calculationsTabPage = new System.Windows.Forms.TabPage();
            this.numericalCalculationsTabPage = new System.Windows.Forms.TabPage();
            this.symbolicCalculationsTabPage = new System.Windows.Forms.TabPage();
            this.symbolicOperationButton = new System.Windows.Forms.Button();
            this.scriptingTabPage = new System.Windows.Forms.TabPage();
            this.customFunctionsTabPage = new System.Windows.Forms.TabPage();
            this.openCustomFunctionsFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveCustomFunctionsFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openScriptFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveScriptFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.symbolicCalculationsTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.XYRatioToolStripStatusLabel,
            this.modeToolStripDropDownButton});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // XYRatioToolStripStatusLabel
            // 
            this.XYRatioToolStripStatusLabel.Name = "XYRatioToolStripStatusLabel";
            resources.ApplyResources(this.XYRatioToolStripStatusLabel, "XYRatioToolStripStatusLabel");
            // 
            // modeToolStripDropDownButton
            // 
            this.modeToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.modeToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dd212ToolStripMenuItem,
            this.fdsfdsToolStripMenuItem,
            this.mode3DFxyToolStripMenuItem});
            resources.ApplyResources(this.modeToolStripDropDownButton, "modeToolStripDropDownButton");
            this.modeToolStripDropDownButton.Name = "modeToolStripDropDownButton";
            // 
            // dd212ToolStripMenuItem
            // 
            this.dd212ToolStripMenuItem.Name = "dd212ToolStripMenuItem";
            resources.ApplyResources(this.dd212ToolStripMenuItem, "dd212ToolStripMenuItem");
            // 
            // fdsfdsToolStripMenuItem
            // 
            this.fdsfdsToolStripMenuItem.Name = "fdsfdsToolStripMenuItem";
            resources.ApplyResources(this.fdsfdsToolStripMenuItem, "fdsfdsToolStripMenuItem");
            // 
            // mode3DFxyToolStripMenuItem
            // 
            this.mode3DFxyToolStripMenuItem.Name = "mode3DFxyToolStripMenuItem";
            resources.ApplyResources(this.mode3DFxyToolStripMenuItem, "mode3DFxyToolStripMenuItem");
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.chartingTabPage);
            this.tabControl1.Controls.Add(this.calculationsTabPage);
            this.tabControl1.Controls.Add(this.numericalCalculationsTabPage);
            this.tabControl1.Controls.Add(this.symbolicCalculationsTabPage);
            this.tabControl1.Controls.Add(this.scriptingTabPage);
            this.tabControl1.Controls.Add(this.customFunctionsTabPage);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // chartingTabPage
            // 
            resources.ApplyResources(this.chartingTabPage, "chartingTabPage");
            this.chartingTabPage.Name = "chartingTabPage";
            this.chartingTabPage.UseVisualStyleBackColor = true;
            // 
            // calculationsTabPage
            // 
            resources.ApplyResources(this.calculationsTabPage, "calculationsTabPage");
            this.calculationsTabPage.Name = "calculationsTabPage";
            this.calculationsTabPage.UseVisualStyleBackColor = true;
            // 
            // numericalCalculationsTabPage
            // 
            resources.ApplyResources(this.numericalCalculationsTabPage, "numericalCalculationsTabPage");
            this.numericalCalculationsTabPage.Name = "numericalCalculationsTabPage";
            this.numericalCalculationsTabPage.UseVisualStyleBackColor = true;
            // 
            // symbolicCalculationsTabPage
            // 
            this.symbolicCalculationsTabPage.Controls.Add(this.symbolicOperationButton);
            resources.ApplyResources(this.symbolicCalculationsTabPage, "symbolicCalculationsTabPage");
            this.symbolicCalculationsTabPage.Name = "symbolicCalculationsTabPage";
            this.symbolicCalculationsTabPage.UseVisualStyleBackColor = true;
            // 
            // symbolicOperationButton
            // 
            resources.ApplyResources(this.symbolicOperationButton, "symbolicOperationButton");
            this.symbolicOperationButton.Name = "symbolicOperationButton";
            this.symbolicOperationButton.UseVisualStyleBackColor = true;
            // 
            // scriptingTabPage
            // 
            resources.ApplyResources(this.scriptingTabPage, "scriptingTabPage");
            this.scriptingTabPage.Name = "scriptingTabPage";
            this.scriptingTabPage.UseVisualStyleBackColor = true;
            // 
            // customFunctionsTabPage
            // 
            resources.ApplyResources(this.customFunctionsTabPage, "customFunctionsTabPage");
            this.customFunctionsTabPage.Name = "customFunctionsTabPage";
            this.customFunctionsTabPage.UseVisualStyleBackColor = true;
            // 
            // openCustomFunctionsFileDialog
            // 
            this.openCustomFunctionsFileDialog.FileName = "custom functions.tslf";
            resources.ApplyResources(this.openCustomFunctionsFileDialog, "openCustomFunctionsFileDialog");
            this.openCustomFunctionsFileDialog.InitialDirectory = "_CustomFunctions";
            // 
            // saveCustomFunctionsFileDialog
            // 
            this.saveCustomFunctionsFileDialog.FileName = "custom functions.tslf";
            resources.ApplyResources(this.saveCustomFunctionsFileDialog, "saveCustomFunctionsFileDialog");
            this.saveCustomFunctionsFileDialog.InitialDirectory = "_CustomFunctions";
            // 
            // openScriptFileDialog
            // 
            this.openScriptFileDialog.FileName = "custom script.tsl";
            resources.ApplyResources(this.openScriptFileDialog, "openScriptFileDialog");
            this.openScriptFileDialog.InitialDirectory = "_Scripts";
            // 
            // saveScriptFileDialog
            // 
            this.saveScriptFileDialog.FileName = "custom script.tsl";
            resources.ApplyResources(this.saveScriptFileDialog, "saveScriptFileDialog");
            this.saveScriptFileDialog.InitialDirectory = "_Scripts";
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "MainForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.symbolicCalculationsTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage chartingTabPage;
        private System.Windows.Forms.TabPage calculationsTabPage;
        private System.Windows.Forms.TabPage numericalCalculationsTabPage;
        private System.Windows.Forms.TabPage symbolicCalculationsTabPage;
        private System.Windows.Forms.Button symbolicOperationButton;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TabPage scriptingTabPage;
        private System.Windows.Forms.TabPage customFunctionsTabPage;
        private System.Windows.Forms.OpenFileDialog openCustomFunctionsFileDialog;
        private System.Windows.Forms.SaveFileDialog saveCustomFunctionsFileDialog;
        private System.Windows.Forms.OpenFileDialog openScriptFileDialog;
        private System.Windows.Forms.SaveFileDialog saveScriptFileDialog;
        private System.Windows.Forms.ToolStripStatusLabel XYRatioToolStripStatusLabel;
        private System.Windows.Forms.ToolStripDropDownButton modeToolStripDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem dd212ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fdsfdsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mode3DFxyToolStripMenuItem;
    }
}

