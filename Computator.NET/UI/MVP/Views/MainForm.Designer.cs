using System;
using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Benchmarking;
using Computator.NET.Charting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Config;
using Computator.NET.DataTypes.Localization;
using Computator.NET.UI.Controls;
using Computator.NET.UI;

namespace Computator.NET
{
    partial class GUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GUI));
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
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.exponentiationToolStripMenuItem = new Computator.NET.UI.Controls.BindableToolStripMenuItem();
            this.functionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.elementaryFunctionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.specialFunctionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.constantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mathematicalConstantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.physicalConstantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.transformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem31 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem32 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem33 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem34 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem35 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem36 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem37 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem38 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem39 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fullscreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem42 = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem40 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem41 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem43 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem45 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem44 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem46 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.symbolicCalculationsTabPage.SuspendLayout();
            this.menuStrip2.SuspendLayout();
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
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
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
            // menuStrip2
            // 
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem1,
            this.functionsToolStripMenuItem,
            this.constantsToolStripMenuItem,
            this.transformToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem1});
            resources.ApplyResources(this.menuStrip2, "menuStrip2");
            this.menuStrip2.Name = "menuStrip2";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.printToolStripMenuItem,
            this.printPreviewToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // newToolStripMenuItem
            // 
            resources.ApplyResources(this.newToolStripMenuItem, "newToolStripMenuItem");
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            // 
            // openToolStripMenuItem
            // 
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            resources.ApplyResources(this.toolStripSeparator, "toolStripSeparator");
            // 
            // saveToolStripMenuItem
            // 
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            resources.ApplyResources(this.saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // printToolStripMenuItem
            // 
            resources.ApplyResources(this.printToolStripMenuItem, "printToolStripMenuItem");
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            // 
            // printPreviewToolStripMenuItem
            // 
            resources.ApplyResources(this.printPreviewToolStripMenuItem, "printPreviewToolStripMenuItem");
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            // 
            // editToolStripMenuItem1
            // 
            this.editToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator3,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator4,
            this.selectAllToolStripMenuItem,
            this.toolStripSeparator18,
            this.exponentiationToolStripMenuItem});
            this.editToolStripMenuItem1.Name = "editToolStripMenuItem1";
            resources.ApplyResources(this.editToolStripMenuItem1, "editToolStripMenuItem1");
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            resources.ApplyResources(this.undoToolStripMenuItem, "undoToolStripMenuItem");
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            resources.ApplyResources(this.redoToolStripMenuItem, "redoToolStripMenuItem");
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // cutToolStripMenuItem
            // 
            resources.ApplyResources(this.cutToolStripMenuItem, "cutToolStripMenuItem");
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            // 
            // copyToolStripMenuItem
            // 
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            // 
            // pasteToolStripMenuItem
            // 
            resources.ApplyResources(this.pasteToolStripMenuItem, "pasteToolStripMenuItem");
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            resources.ApplyResources(this.selectAllToolStripMenuItem, "selectAllToolStripMenuItem");
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            resources.ApplyResources(this.toolStripSeparator18, "toolStripSeparator18");
            // 
            // exponentiationToolStripMenuItem
            // 
            this.exponentiationToolStripMenuItem.CheckOnClick = true;
            this.exponentiationToolStripMenuItem.Name = "exponentiationToolStripMenuItem";
            resources.ApplyResources(this.exponentiationToolStripMenuItem, "exponentiationToolStripMenuItem");
            // 
            // functionsToolStripMenuItem
            // 
            this.functionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.elementaryFunctionsToolStripMenuItem,
            this.specialFunctionsToolStripMenuItem});
            this.functionsToolStripMenuItem.Name = "functionsToolStripMenuItem";
            resources.ApplyResources(this.functionsToolStripMenuItem, "functionsToolStripMenuItem");
            // 
            // elementaryFunctionsToolStripMenuItem
            // 
            this.elementaryFunctionsToolStripMenuItem.Name = "elementaryFunctionsToolStripMenuItem";
            resources.ApplyResources(this.elementaryFunctionsToolStripMenuItem, "elementaryFunctionsToolStripMenuItem");
            // 
            // specialFunctionsToolStripMenuItem
            // 
            this.specialFunctionsToolStripMenuItem.Name = "specialFunctionsToolStripMenuItem";
            resources.ApplyResources(this.specialFunctionsToolStripMenuItem, "specialFunctionsToolStripMenuItem");
            // 
            // constantsToolStripMenuItem
            // 
            this.constantsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mathematicalConstantsToolStripMenuItem,
            this.physicalConstantsToolStripMenuItem});
            this.constantsToolStripMenuItem.Name = "constantsToolStripMenuItem";
            resources.ApplyResources(this.constantsToolStripMenuItem, "constantsToolStripMenuItem");
            // 
            // mathematicalConstantsToolStripMenuItem
            // 
            this.mathematicalConstantsToolStripMenuItem.Name = "mathematicalConstantsToolStripMenuItem";
            resources.ApplyResources(this.mathematicalConstantsToolStripMenuItem, "mathematicalConstantsToolStripMenuItem");
            // 
            // physicalConstantsToolStripMenuItem
            // 
            this.physicalConstantsToolStripMenuItem.Name = "physicalConstantsToolStripMenuItem";
            resources.ApplyResources(this.physicalConstantsToolStripMenuItem, "physicalConstantsToolStripMenuItem");
            // 
            // transformToolStripMenuItem
            // 
            this.transformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem31,
            this.toolStripMenuItem32,
            this.toolStripMenuItem33,
            this.toolStripMenuItem34,
            this.toolStripMenuItem35,
            this.toolStripMenuItem36,
            this.toolStripMenuItem37,
            this.toolStripMenuItem38,
            this.toolStripMenuItem39});
            this.transformToolStripMenuItem.Name = "transformToolStripMenuItem";
            resources.ApplyResources(this.transformToolStripMenuItem, "transformToolStripMenuItem");
            // 
            // toolStripMenuItem31
            // 
            this.toolStripMenuItem31.Name = "toolStripMenuItem31";
            resources.ApplyResources(this.toolStripMenuItem31, "toolStripMenuItem31");
            this.toolStripMenuItem31.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem32
            // 
            this.toolStripMenuItem32.Name = "toolStripMenuItem32";
            resources.ApplyResources(this.toolStripMenuItem32, "toolStripMenuItem32");
            this.toolStripMenuItem32.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem33
            // 
            this.toolStripMenuItem33.Name = "toolStripMenuItem33";
            resources.ApplyResources(this.toolStripMenuItem33, "toolStripMenuItem33");
            this.toolStripMenuItem33.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem34
            // 
            this.toolStripMenuItem34.Name = "toolStripMenuItem34";
            resources.ApplyResources(this.toolStripMenuItem34, "toolStripMenuItem34");
            this.toolStripMenuItem34.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem35
            // 
            this.toolStripMenuItem35.Name = "toolStripMenuItem35";
            resources.ApplyResources(this.toolStripMenuItem35, "toolStripMenuItem35");
            this.toolStripMenuItem35.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem36
            // 
            this.toolStripMenuItem36.Name = "toolStripMenuItem36";
            resources.ApplyResources(this.toolStripMenuItem36, "toolStripMenuItem36");
            this.toolStripMenuItem36.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem37
            // 
            this.toolStripMenuItem37.Name = "toolStripMenuItem37";
            resources.ApplyResources(this.toolStripMenuItem37, "toolStripMenuItem37");
            this.toolStripMenuItem37.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem38
            // 
            this.toolStripMenuItem38.Name = "toolStripMenuItem38";
            resources.ApplyResources(this.toolStripMenuItem38, "toolStripMenuItem38");
            this.toolStripMenuItem38.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolStripMenuItem39
            // 
            this.toolStripMenuItem39.Name = "toolStripMenuItem39";
            resources.ApplyResources(this.toolStripMenuItem39, "toolStripMenuItem39");
            this.toolStripMenuItem39.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem1,
            this.fullscreenToolStripMenuItem,
            this.toolStripMenuItem42,
            this.toolStripSeparator15,
            this.toolStripMenuItem40,
            this.toolStripSeparator16,
            this.toolStripMenuItem41});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            resources.ApplyResources(this.toolsToolStripMenuItem, "toolsToolStripMenuItem");
            // 
            // optionsToolStripMenuItem1
            // 
            this.optionsToolStripMenuItem1.Name = "optionsToolStripMenuItem1";
            resources.ApplyResources(this.optionsToolStripMenuItem1, "optionsToolStripMenuItem1");
            // 
            // fullscreenToolStripMenuItem
            // 
            this.fullscreenToolStripMenuItem.CheckOnClick = true;
            this.fullscreenToolStripMenuItem.Name = "fullscreenToolStripMenuItem";
            resources.ApplyResources(this.fullscreenToolStripMenuItem, "fullscreenToolStripMenuItem");
            // 
            // toolStripMenuItem42
            // 
            this.toolStripMenuItem42.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.languageToolStripComboBox});
            this.toolStripMenuItem42.Name = "toolStripMenuItem42";
            resources.ApplyResources(this.toolStripMenuItem42, "toolStripMenuItem42");
            // 
            // languageToolStripComboBox
            // 
            this.languageToolStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageToolStripComboBox.Name = "languageToolStripComboBox";
            resources.ApplyResources(this.languageToolStripComboBox, "languageToolStripComboBox");
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            resources.ApplyResources(this.toolStripSeparator15, "toolStripSeparator15");
            // 
            // toolStripMenuItem40
            // 
            this.toolStripMenuItem40.Name = "toolStripMenuItem40";
            resources.ApplyResources(this.toolStripMenuItem40, "toolStripMenuItem40");
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            resources.ApplyResources(this.toolStripSeparator16, "toolStripSeparator16");
            // 
            // toolStripMenuItem41
            // 
            this.toolStripMenuItem41.Name = "toolStripMenuItem41";
            resources.ApplyResources(this.toolStripMenuItem41, "toolStripMenuItem41");
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem43,
            this.toolStripMenuItem45,
            this.toolStripSeparator5,
            this.aboutToolStripMenuItem1,
            this.toolStripSeparator14,
            this.toolStripMenuItem44,
            this.toolStripMenuItem46});
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            resources.ApplyResources(this.helpToolStripMenuItem1, "helpToolStripMenuItem1");
            // 
            // toolStripMenuItem43
            // 
            this.toolStripMenuItem43.Name = "toolStripMenuItem43";
            resources.ApplyResources(this.toolStripMenuItem43, "toolStripMenuItem43");
            // 
            // toolStripMenuItem45
            // 
            this.toolStripMenuItem45.Name = "toolStripMenuItem45";
            resources.ApplyResources(this.toolStripMenuItem45, "toolStripMenuItem45");
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            resources.ApplyResources(this.aboutToolStripMenuItem1, "aboutToolStripMenuItem1");
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            resources.ApplyResources(this.toolStripSeparator14, "toolStripSeparator14");
            // 
            // toolStripMenuItem44
            // 
            this.toolStripMenuItem44.Name = "toolStripMenuItem44";
            resources.ApplyResources(this.toolStripMenuItem44, "toolStripMenuItem44");
            // 
            // toolStripMenuItem46
            // 
            this.toolStripMenuItem46.Name = "toolStripMenuItem46";
            resources.ApplyResources(this.toolStripMenuItem46, "toolStripMenuItem46");
            // 
            // GUI
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip2);
            this.Name = "GUI";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.symbolicCalculationsTabPage.ResumeLayout(false);
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
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
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripDropDownButton modeToolStripDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem dd212ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fdsfdsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mode3DFxyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem functionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem elementaryFunctionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem specialFunctionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem constantsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mathematicalConstantsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem physicalConstantsToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem transformToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem31;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem32;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem33;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem34;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem35;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem36;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem37;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem38;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem39;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem40;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem41;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem42;
        private System.Windows.Forms.ToolStripComboBox languageToolStripComboBox;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem43;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem45;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem44;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem46;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private BindableToolStripMenuItem exponentiationToolStripMenuItem;
        private ToolStripMenuItem fullscreenToolStripMenuItem;
    }
}

