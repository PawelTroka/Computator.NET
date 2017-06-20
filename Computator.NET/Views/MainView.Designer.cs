using System.Windows.Forms;
using Computator.NET.Core.Properties;

namespace Computator.NET.Views
{
    partial class MainView : Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 764);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1005, 26);
            this.statusStrip1.TabIndex = 5;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 21);
            // 
            // XYRatioToolStripStatusLabel
            // 
            this.XYRatioToolStripStatusLabel.Name = "XYRatioToolStripStatusLabel";
            this.XYRatioToolStripStatusLabel.Size = new System.Drawing.Size(0, 21);
            // 
            // modeToolStripDropDownButton
            // 
            this.modeToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.modeToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dd212ToolStripMenuItem,
            this.fdsfdsToolStripMenuItem,
            this.mode3DFxyToolStripMenuItem});
            this.modeToolStripDropDownButton.Image = DataTypes.Localization.Views.MainView.modeToolStripDropDownButton_Image;
            this.modeToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.modeToolStripDropDownButton.Name = "modeToolStripDropDownButton";
            this.modeToolStripDropDownButton.Size = new System.Drawing.Size(134, 24);
            this.modeToolStripDropDownButton.Text = "Mode[Real : f(x)]";
            // 
            // dd212ToolStripMenuItem
            // 
            this.dd212ToolStripMenuItem.Name = "dd212ToolStripMenuItem";
            this.dd212ToolStripMenuItem.Size = new System.Drawing.Size(176, 26);
            this.dd212ToolStripMenuItem.Text = "Real : f(x)";
            // 
            // fdsfdsToolStripMenuItem
            // 
            this.fdsfdsToolStripMenuItem.Name = "fdsfdsToolStripMenuItem";
            this.fdsfdsToolStripMenuItem.Size = new System.Drawing.Size(176, 26);
            this.fdsfdsToolStripMenuItem.Text = "Complex : f(z)";
            // 
            // mode3DFxyToolStripMenuItem
            // 
            this.mode3DFxyToolStripMenuItem.Name = "mode3DFxyToolStripMenuItem";
            this.mode3DFxyToolStripMenuItem.Size = new System.Drawing.Size(176, 26);
            this.mode3DFxyToolStripMenuItem.Text = "3D : f(x,y)";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.chartingTabPage);
            this.tabControl1.Controls.Add(this.calculationsTabPage);
            this.tabControl1.Controls.Add(this.numericalCalculationsTabPage);
            this.tabControl1.Controls.Add(this.symbolicCalculationsTabPage);
            this.tabControl1.Controls.Add(this.scriptingTabPage);
            this.tabControl1.Controls.Add(this.customFunctionsTabPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1005, 764);
            this.tabControl1.TabIndex = 4;
            // 
            // chartingTabPage
            // 
            this.chartingTabPage.Location = new System.Drawing.Point(4, 29);
            this.chartingTabPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chartingTabPage.Name = "chartingTabPage";
            this.chartingTabPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chartingTabPage.Size = new System.Drawing.Size(997, 731);
            this.chartingTabPage.TabIndex = 0;
            this.chartingTabPage.Text = DataTypes.Localization.Views.MainView.chartingTabPage_Text;
            this.chartingTabPage.UseVisualStyleBackColor = true;
            // 
            // calculationsTabPage
            // 
            this.calculationsTabPage.Location = new System.Drawing.Point(4, 29);
            this.calculationsTabPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.calculationsTabPage.Name = "calculationsTabPage";
            this.calculationsTabPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.calculationsTabPage.Size = new System.Drawing.Size(997, 730);
            this.calculationsTabPage.TabIndex = 1;
            this.calculationsTabPage.Text = DataTypes.Localization.Views.MainView.calculationsTabPage_Text;
            this.calculationsTabPage.UseVisualStyleBackColor = true;
            // 
            // numericalCalculationsTabPage
            // 
            this.numericalCalculationsTabPage.Location = new System.Drawing.Point(4, 29);
            this.numericalCalculationsTabPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericalCalculationsTabPage.Name = "numericalCalculationsTabPage";
            this.numericalCalculationsTabPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericalCalculationsTabPage.Size = new System.Drawing.Size(997, 730);
            this.numericalCalculationsTabPage.TabIndex = 2;
            this.numericalCalculationsTabPage.Text = DataTypes.Localization.Views.MainView.numericalCalculationsTabPage_Text;
            this.numericalCalculationsTabPage.UseVisualStyleBackColor = true;
            // 
            // symbolicCalculationsTabPage
            // 
            this.symbolicCalculationsTabPage.Controls.Add(this.symbolicOperationButton);
            this.symbolicCalculationsTabPage.Location = new System.Drawing.Point(4, 29);
            this.symbolicCalculationsTabPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.symbolicCalculationsTabPage.Name = "symbolicCalculationsTabPage";
            this.symbolicCalculationsTabPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.symbolicCalculationsTabPage.Size = new System.Drawing.Size(997, 730);
            this.symbolicCalculationsTabPage.TabIndex = 3;
            this.symbolicCalculationsTabPage.Text = DataTypes.Localization.Views.MainView.symbolicCalculationsTabPage_Text;
            this.symbolicCalculationsTabPage.UseVisualStyleBackColor = true;
            // 
            // symbolicOperationButton
            // 
            this.symbolicOperationButton.Enabled = false;
            this.symbolicOperationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.symbolicOperationButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.symbolicOperationButton.Location = new System.Drawing.Point(501, 172);
            this.symbolicOperationButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.symbolicOperationButton.Name = "symbolicOperationButton";
            this.symbolicOperationButton.Size = new System.Drawing.Size(168, 73);
            this.symbolicOperationButton.TabIndex = 0;
            this.symbolicOperationButton.Text = DataTypes.Localization.Views.MainView.symbolicOperationButton_Text;
            this.symbolicOperationButton.UseVisualStyleBackColor = true;
            // 
            // scriptingTabPage
            // 
            this.scriptingTabPage.Location = new System.Drawing.Point(4, 29);
            this.scriptingTabPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.scriptingTabPage.Name = "scriptingTabPage";
            this.scriptingTabPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.scriptingTabPage.Size = new System.Drawing.Size(997, 730);
            this.scriptingTabPage.TabIndex = 4;
            this.scriptingTabPage.Text = DataTypes.Localization.Views.MainView.scriptingTabPage_Text;
            this.scriptingTabPage.UseVisualStyleBackColor = true;
            // 
            // customFunctionsTabPage
            // 
            this.customFunctionsTabPage.Location = new System.Drawing.Point(4, 29);
            this.customFunctionsTabPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.customFunctionsTabPage.Name = "customFunctionsTabPage";
            this.customFunctionsTabPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.customFunctionsTabPage.Size = new System.Drawing.Size(997, 730);
            this.customFunctionsTabPage.TabIndex = 5;
            this.customFunctionsTabPage.Text = DataTypes.Localization.Views.MainView.customFunctionsTabPage_Text;
            this.customFunctionsTabPage.UseVisualStyleBackColor = true;
            // 
            // openCustomFunctionsFileDialog
            // 
            this.openCustomFunctionsFileDialog.FileName = "custom functions.tslf";
            this.openCustomFunctionsFileDialog.Filter = "Troka Scripting Language Functions (*.tslf)|*.tslf";
            this.openCustomFunctionsFileDialog.InitialDirectory = "_CustomFunctions";
            // 
            // saveCustomFunctionsFileDialog
            // 
            this.saveCustomFunctionsFileDialog.FileName = "custom functions.tslf";
            this.saveCustomFunctionsFileDialog.Filter = "Troka Scripting Language Functions (*.tslf)|*.tslf";
            this.saveCustomFunctionsFileDialog.InitialDirectory = "_CustomFunctions";
            // 
            // openScriptFileDialog
            // 
            this.openScriptFileDialog.FileName = "custom script.tsl";
            this.openScriptFileDialog.Filter = "Troka Scripting Language (*.tsl)|*.tsl";
            this.openScriptFileDialog.InitialDirectory = "_Scripts";
            // 
            // saveScriptFileDialog
            // 
            this.saveScriptFileDialog.FileName = "custom script.tsl";
            this.saveScriptFileDialog.Filter = "Troka Scripting Language (*.tsl)|*.tsl";
            this.saveScriptFileDialog.InitialDirectory = "_Scripts";
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1005, 790);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = Resources.computator_net_icon;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(993, 717);
            this.Name = "MainView";
            this.Text = "Computator.NET © Pawel Troka";
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

