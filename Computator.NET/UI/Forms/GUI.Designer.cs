using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Charting;
using Computator.NET.Charting.RealCharting;
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.consoleOutputTextBox = new System.Windows.Forms.RichTextBox();
            this.scriptingDirectoryTree = new Computator.NET.DirectoryTree();
            this.openScriptingDirectoryButton = new System.Windows.Forms.Button();
            this.processButton = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.customFunctionsDirectoryTree = new Computator.NET.DirectoryTree();
            this.openCustomFunctionsDirectoryButton = new System.Windows.Forms.Button();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.calculationsImZnumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.calculationsRealLabel = new System.Windows.Forms.Label();
            this.valueForCalculationNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.calculationsComplexLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.calculationValueTextBox = new System.Windows.Forms.TextBox();
            this.calculateButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.calculationsHistoryDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.maxErrorGroupBox = new System.Windows.Forms.GroupBox();
            this.epsTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.stepsGroupBox = new System.Windows.Forms.GroupBox();
            this.nStepsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.resultNumericalCalculationsTextBox = new System.Windows.Forms.TextBox();
            this.derivativeAtPointGroupBox = new System.Windows.Forms.GroupBox();
            this.nOrderDerivativeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.xDerivativePointNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.intervalGroupBox = new System.Windows.Forms.GroupBox();
            this.bIntervalNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.aIntervalNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.numericalCalculationsDataGridView = new System.Windows.Forms.DataGridView();
            this.function = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.operation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.method = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parameters = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.XYRatioToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.modeToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.dd212ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fdsfdsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mode3DFxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.chartingTabPage = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.yNNumericUpDown = new ScientificNumericUpDown();
            this.y0NumericUpDown = new ScientificNumericUpDown();
            this.yNlabel = new System.Windows.Forms.Label();
            this.y0label = new System.Windows.Forms.Label();
            this.xnNumericUpDown = new ScientificNumericUpDown();
            this.x0NumericUpDown = new ScientificNumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.clearChartButton = new System.Windows.Forms.Button();
            this.addToChartButton = new System.Windows.Forms.Button();
            this.calculationsTabPage = new System.Windows.Forms.TabPage();
            this.numericalCalculationsTabPage = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.methodNumericalCalculationsComboBox = new System.Windows.Forms.ComboBox();
            this.operationNumericalCalculationsComboBox = new System.Windows.Forms.ComboBox();
            this.numericalOperationButton = new System.Windows.Forms.Button();
            this.symbolicCalculationsTabPage = new System.Windows.Forms.TabPage();
            this.symbolicOperationButton = new System.Windows.Forms.Button();
            this.scriptingTabPage = new System.Windows.Forms.TabPage();
            this.customFunctionsTabPage = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.expressionTextBox = new Computator.NET.UI.Controls.ExpressionTextBox();
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
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripSeparator();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripSeparator();
            new System.Windows.Forms.ToolStripMenuItem();
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
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripSeparator();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripComboBox();
            new System.Windows.Forms.ToolStripSeparator();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripSeparator();
            new System.Windows.Forms.ToolStripMenuItem();

            new System.Windows.Forms.ToolStripSeparator();
            new System.Windows.Forms.ToolStripMenuItem();
            new System.Windows.Forms.ToolStripMenuItem();
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
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.newToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.openToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.printToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.copyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.pasteToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.runToolStripButton = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsImZnumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueForCalculationNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsHistoryDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.maxErrorGroupBox.SuspendLayout();
            this.stepsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nStepsNumericUpDown)).BeginInit();
            this.derivativeAtPointGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).BeginInit();
            this.intervalGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.chartingTabPage.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yNNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.y0NumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xnNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.x0NumericUpDown)).BeginInit();
            this.calculationsTabPage.SuspendLayout();
            this.numericalCalculationsTabPage.SuspendLayout();
            this.symbolicCalculationsTabPage.SuspendLayout();
            this.scriptingTabPage.SuspendLayout();
            this.customFunctionsTabPage.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.scriptingDirectoryTree);
            this.splitContainer1.Panel2.Controls.Add(this.openScriptingDirectoryButton);
            this.splitContainer1.Panel2.Controls.Add(this.processButton);
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.consoleOutputTextBox);
            // 
            // consoleOutputTextBox
            // 
            this.consoleOutputTextBox.BackColor = System.Drawing.Color.White;
            this.consoleOutputTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.consoleOutputTextBox.Cursor = System.Windows.Forms.Cursors.Default;
            resources.ApplyResources(this.consoleOutputTextBox, "consoleOutputTextBox");
            this.consoleOutputTextBox.ForeColor = System.Drawing.Color.Black;
            this.consoleOutputTextBox.Name = "consoleOutputTextBox";
            this.consoleOutputTextBox.ReadOnly = true;
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
            this.openScriptingDirectoryButton.Click += new System.EventHandler(this.openScriptDirectoryButton_Click);
            // 
            // processButton
            // 
            resources.ApplyResources(this.processButton, "processButton");
            this.processButton.Name = "processButton";
            this.processButton.UseVisualStyleBackColor = true;
            this.processButton.Click += new System.EventHandler(this.processButton_Click);
            // 
            // splitContainer3
            // 
            resources.ApplyResources(this.splitContainer3, "splitContainer3");
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.customFunctionsDirectoryTree);
            this.splitContainer3.Panel2.Controls.Add(this.openCustomFunctionsDirectoryButton);
            // 
            // customFunctionsDirectoryTree
            // 
            this.customFunctionsDirectoryTree.CodeEditorWrapper = null;
            resources.ApplyResources(this.customFunctionsDirectoryTree, "customFunctionsDirectoryTree");
            this.customFunctionsDirectoryTree.Name = "customFunctionsDirectoryTree";
            this.customFunctionsDirectoryTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("customFunctionsDirectoryTree.Nodes")))});
            this.customFunctionsDirectoryTree.Path = null;
            // 
            // openCustomFunctionsDirectoryButton
            // 
            resources.ApplyResources(this.openCustomFunctionsDirectoryButton, "openCustomFunctionsDirectoryButton");
            this.openCustomFunctionsDirectoryButton.Name = "openCustomFunctionsDirectoryButton";
            this.openCustomFunctionsDirectoryButton.UseVisualStyleBackColor = true;
            this.openCustomFunctionsDirectoryButton.Click += new System.EventHandler(this.openCustomFunctionsDirectoryButton_Click);
            // 
            // splitContainer4
            // 
            resources.ApplyResources(this.splitContainer4, "splitContainer4");
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.groupBox3);
            this.splitContainer4.Panel1.Controls.Add(this.label5);
            this.splitContainer4.Panel1.Controls.Add(this.calculationValueTextBox);
            this.splitContainer4.Panel1.Controls.Add(this.calculateButton);
            this.splitContainer4.Panel1.Controls.Add(this.label7);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.calculationsHistoryDataGridView);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.calculationsImZnumericUpDown);
            this.groupBox3.Controls.Add(this.calculationsRealLabel);
            this.groupBox3.Controls.Add(this.valueForCalculationNumericUpDown);
            this.groupBox3.Controls.Add(this.calculationsComplexLabel);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // calculationsImZnumericUpDown
            // 
            this.calculationsImZnumericUpDown.DecimalPlaces = 5;
            resources.ApplyResources(this.calculationsImZnumericUpDown, "calculationsImZnumericUpDown");
            this.calculationsImZnumericUpDown.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.calculationsImZnumericUpDown.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.calculationsImZnumericUpDown.Name = "calculationsImZnumericUpDown";
            // 
            // calculationsRealLabel
            // 
            resources.ApplyResources(this.calculationsRealLabel, "calculationsRealLabel");
            this.calculationsRealLabel.Name = "calculationsRealLabel";
            // 
            // valueForCalculationNumericUpDown
            // 
            this.valueForCalculationNumericUpDown.DecimalPlaces = 5;
            resources.ApplyResources(this.valueForCalculationNumericUpDown, "valueForCalculationNumericUpDown");
            this.valueForCalculationNumericUpDown.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.valueForCalculationNumericUpDown.Minimum = new decimal(new int[] {
            1000000000,
            0,
            0,
            -2147483648});
            this.valueForCalculationNumericUpDown.Name = "valueForCalculationNumericUpDown";
            // 
            // calculationsComplexLabel
            // 
            resources.ApplyResources(this.calculationsComplexLabel, "calculationsComplexLabel");
            this.calculationsComplexLabel.Name = "calculationsComplexLabel";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // calculationValueTextBox
            // 
            resources.ApplyResources(this.calculationValueTextBox, "calculationValueTextBox");
            this.calculationValueTextBox.Name = "calculationValueTextBox";
            // 
            // calculateButton
            // 
            resources.ApplyResources(this.calculateButton, "calculateButton");
            this.calculateButton.Name = "calculateButton";
            this.calculateButton.UseVisualStyleBackColor = true;
            this.calculateButton.Click += new System.EventHandler(this.calculateButton_Click);
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // calculationsHistoryDataGridView
            // 
            this.calculationsHistoryDataGridView.AllowUserToAddRows = false;
            this.calculationsHistoryDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.calculationsHistoryDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.calculationsHistoryDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5});
            resources.ApplyResources(this.calculationsHistoryDataGridView, "calculationsHistoryDataGridView");
            this.calculationsHistoryDataGridView.Name = "calculationsHistoryDataGridView";
            this.calculationsHistoryDataGridView.ReadOnly = true;
            this.calculationsHistoryDataGridView.RowTemplate.Height = 24;
            this.calculationsHistoryDataGridView.RowTemplate.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.dataGridViewTextBoxColumn4, "dataGridViewTextBoxColumn4");
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.dataGridViewTextBoxColumn5.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.dataGridViewTextBoxColumn5, "dataGridViewTextBoxColumn5");
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            // 
            // splitContainer5
            // 
            resources.ApplyResources(this.splitContainer5, "splitContainer5");
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.maxErrorGroupBox);
            this.splitContainer5.Panel1.Controls.Add(this.stepsGroupBox);
            this.splitContainer5.Panel1.Controls.Add(this.resultNumericalCalculationsTextBox);
            this.splitContainer5.Panel1.Controls.Add(this.derivativeAtPointGroupBox);
            this.splitContainer5.Panel1.Controls.Add(this.intervalGroupBox);
            this.splitContainer5.Panel1.Controls.Add(this.label8);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.numericalCalculationsDataGridView);
            // 
            // maxErrorGroupBox
            // 
            this.maxErrorGroupBox.Controls.Add(this.epsTextBox);
            this.maxErrorGroupBox.Controls.Add(this.label10);
            resources.ApplyResources(this.maxErrorGroupBox, "maxErrorGroupBox");
            this.maxErrorGroupBox.Name = "maxErrorGroupBox";
            this.maxErrorGroupBox.TabStop = false;
            // 
            // epsTextBox
            // 
            resources.ApplyResources(this.epsTextBox, "epsTextBox");
            this.epsTextBox.Name = "epsTextBox";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // stepsGroupBox
            // 
            this.stepsGroupBox.Controls.Add(this.nStepsNumericUpDown);
            this.stepsGroupBox.Controls.Add(this.label19);
            resources.ApplyResources(this.stepsGroupBox, "stepsGroupBox");
            this.stepsGroupBox.Name = "stepsGroupBox";
            this.stepsGroupBox.TabStop = false;
            // 
            // nStepsNumericUpDown
            // 
            resources.ApplyResources(this.nStepsNumericUpDown, "nStepsNumericUpDown");
            this.nStepsNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.nStepsNumericUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nStepsNumericUpDown.Name = "nStepsNumericUpDown";
            this.nStepsNumericUpDown.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // label19
            // 
            resources.ApplyResources(this.label19, "label19");
            this.label19.Name = "label19";
            // 
            // resultNumericalCalculationsTextBox
            // 
            resources.ApplyResources(this.resultNumericalCalculationsTextBox, "resultNumericalCalculationsTextBox");
            this.resultNumericalCalculationsTextBox.Name = "resultNumericalCalculationsTextBox";
            // 
            // derivativeAtPointGroupBox
            // 
            this.derivativeAtPointGroupBox.Controls.Add(this.nOrderDerivativeNumericUpDown);
            this.derivativeAtPointGroupBox.Controls.Add(this.xDerivativePointNumericUpDown);
            this.derivativeAtPointGroupBox.Controls.Add(this.label14);
            this.derivativeAtPointGroupBox.Controls.Add(this.label15);
            resources.ApplyResources(this.derivativeAtPointGroupBox, "derivativeAtPointGroupBox");
            this.derivativeAtPointGroupBox.Name = "derivativeAtPointGroupBox";
            this.derivativeAtPointGroupBox.TabStop = false;
            // 
            // nOrderDerivativeNumericUpDown
            // 
            resources.ApplyResources(this.nOrderDerivativeNumericUpDown, "nOrderDerivativeNumericUpDown");
            this.nOrderDerivativeNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.nOrderDerivativeNumericUpDown.Name = "nOrderDerivativeNumericUpDown";
            this.nOrderDerivativeNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // xDerivativePointNumericUpDown
            // 
            this.xDerivativePointNumericUpDown.DecimalPlaces = 9;
            resources.ApplyResources(this.xDerivativePointNumericUpDown, "xDerivativePointNumericUpDown");
            this.xDerivativePointNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.xDerivativePointNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.xDerivativePointNumericUpDown.Minimum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            -2147483648});
            this.xDerivativePointNumericUpDown.Name = "xDerivativePointNumericUpDown";
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // intervalGroupBox
            // 
            this.intervalGroupBox.Controls.Add(this.bIntervalNumericUpDown);
            this.intervalGroupBox.Controls.Add(this.aIntervalNumericUpDown);
            this.intervalGroupBox.Controls.Add(this.label12);
            this.intervalGroupBox.Controls.Add(this.label13);
            resources.ApplyResources(this.intervalGroupBox, "intervalGroupBox");
            this.intervalGroupBox.Name = "intervalGroupBox";
            this.intervalGroupBox.TabStop = false;
            // 
            // bIntervalNumericUpDown
            // 
            this.bIntervalNumericUpDown.DecimalPlaces = 9;
            resources.ApplyResources(this.bIntervalNumericUpDown, "bIntervalNumericUpDown");
            this.bIntervalNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.bIntervalNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.bIntervalNumericUpDown.Minimum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            -2147483648});
            this.bIntervalNumericUpDown.Name = "bIntervalNumericUpDown";
            this.bIntervalNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // aIntervalNumericUpDown
            // 
            this.aIntervalNumericUpDown.DecimalPlaces = 9;
            resources.ApplyResources(this.aIntervalNumericUpDown, "aIntervalNumericUpDown");
            this.aIntervalNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.aIntervalNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.aIntervalNumericUpDown.Minimum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            -2147483648});
            this.aIntervalNumericUpDown.Name = "aIntervalNumericUpDown";
            this.aIntervalNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // numericalCalculationsDataGridView
            // 
            this.numericalCalculationsDataGridView.AllowUserToAddRows = false;
            this.numericalCalculationsDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.numericalCalculationsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.numericalCalculationsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.function,
            this.operation,
            this.method,
            this.parameters,
            this.result});
            resources.ApplyResources(this.numericalCalculationsDataGridView, "numericalCalculationsDataGridView");
            this.numericalCalculationsDataGridView.Name = "numericalCalculationsDataGridView";
            this.numericalCalculationsDataGridView.ReadOnly = true;
            this.numericalCalculationsDataGridView.RowTemplate.Height = 24;
            this.numericalCalculationsDataGridView.RowTemplate.ReadOnly = true;
            // 
            // function
            // 
            this.function.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.function.DefaultCellStyle = dataGridViewCellStyle4;
            resources.ApplyResources(this.function, "function");
            this.function.Name = "function";
            this.function.ReadOnly = true;
            // 
            // operation
            // 
            this.operation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.operation, "operation");
            this.operation.Name = "operation";
            this.operation.ReadOnly = true;
            // 
            // method
            // 
            this.method.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.method, "method");
            this.method.Name = "method";
            this.method.ReadOnly = true;
            // 
            // parameters
            // 
            this.parameters.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.parameters.DefaultCellStyle = dataGridViewCellStyle5;
            resources.ApplyResources(this.parameters, "parameters");
            this.parameters.Name = "parameters";
            this.parameters.ReadOnly = true;
            // 
            // result
            // 
            this.result.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.result.DefaultCellStyle = dataGridViewCellStyle6;
            resources.ApplyResources(this.result, "result");
            this.result.Name = "result";
            this.result.ReadOnly = true;
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
            this.dd212ToolStripMenuItem.Click += new System.EventHandler(this.modeRealToolStripMenuItem_Click);
            // 
            // fdsfdsToolStripMenuItem
            // 
            this.fdsfdsToolStripMenuItem.Name = "fdsfdsToolStripMenuItem";
            resources.ApplyResources(this.fdsfdsToolStripMenuItem, "fdsfdsToolStripMenuItem");
            this.fdsfdsToolStripMenuItem.Click += new System.EventHandler(this.modeRealToolStripMenuItem_Click);
            // 
            // mode3DFxyToolStripMenuItem
            // 
            this.mode3DFxyToolStripMenuItem.Name = "mode3DFxyToolStripMenuItem";
            resources.ApplyResources(this.mode3DFxyToolStripMenuItem, "mode3DFxyToolStripMenuItem");
            this.mode3DFxyToolStripMenuItem.Click += new System.EventHandler(this.modeRealToolStripMenuItem_Click);
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
            this.chartingTabPage.Controls.Add(this.panel2);
            resources.ApplyResources(this.chartingTabPage, "chartingTabPage");
            this.chartingTabPage.Name = "chartingTabPage";
            this.chartingTabPage.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel1);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.trackBar1);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.clearChartButton);
            this.panel1.Controls.Add(this.addToChartButton);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // trackBar1
            // 
            this.trackBar1.LargeChange = 20;
            resources.ApplyResources(this.trackBar1, "trackBar1");
            this.trackBar1.Maximum = 200;
            this.trackBar1.Minimum = 10;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.SmallChange = 10;
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar1.Value = 100;
            this.trackBar1.MouseCaptureChanged += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.yNNumericUpDown);
            this.groupBox2.Controls.Add(this.y0NumericUpDown);
            this.groupBox2.Controls.Add(this.yNlabel);
            this.groupBox2.Controls.Add(this.y0label);
            this.groupBox2.Controls.Add(this.xnNumericUpDown);
            this.groupBox2.Controls.Add(this.x0NumericUpDown);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // yNNumericUpDown
            // 
            this.yNNumericUpDown.DecimalPlaces = 4;
            resources.ApplyResources(this.yNNumericUpDown, "yNNumericUpDown");
            
            this.yNNumericUpDown.Name = "yNNumericUpDown";
            this.yNNumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // y0NumericUpDown
            // 
            this.y0NumericUpDown.DecimalPlaces = 4;
            resources.ApplyResources(this.y0NumericUpDown, "y0NumericUpDown");

            this.y0NumericUpDown.Name = "y0NumericUpDown";
            this.y0NumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            -2147483648});
            // 
            // yNlabel
            // 
            resources.ApplyResources(this.yNlabel, "yNlabel");
            this.yNlabel.Name = "yNlabel";
            // 
            // y0label
            // 
            resources.ApplyResources(this.y0label, "y0label");
            this.y0label.Name = "y0label";
            // 
            // xnNumericUpDown
            // 
            this.xnNumericUpDown.DecimalPlaces = 4;
            resources.ApplyResources(this.xnNumericUpDown, "xnNumericUpDown");

            this.xnNumericUpDown.Name = "xnNumericUpDown";
            this.xnNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // x0NumericUpDown
            // 
            this.x0NumericUpDown.DecimalPlaces = 4;
            resources.ApplyResources(this.x0NumericUpDown, "x0NumericUpDown");

            this.x0NumericUpDown.Name = "x0NumericUpDown";
            this.x0NumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            -2147483648});
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // clearChartButton
            // 
            resources.ApplyResources(this.clearChartButton, "clearChartButton");
            this.clearChartButton.Name = "clearChartButton";
            this.clearChartButton.UseVisualStyleBackColor = true;
            this.clearChartButton.Click += new System.EventHandler(this.clearChartButton_Click);
            // 
            // addToChartButton
            // 
            resources.ApplyResources(this.addToChartButton, "addToChartButton");
            this.addToChartButton.Name = "addToChartButton";
            this.addToChartButton.UseVisualStyleBackColor = true;
            this.addToChartButton.Click += new System.EventHandler(this.addToChartButton_Click);
            // 
            // calculationsTabPage
            // 
            this.calculationsTabPage.Controls.Add(this.splitContainer4);
            resources.ApplyResources(this.calculationsTabPage, "calculationsTabPage");
            this.calculationsTabPage.Name = "calculationsTabPage";
            this.calculationsTabPage.UseVisualStyleBackColor = true;
            // 
            // numericalCalculationsTabPage
            // 
            this.numericalCalculationsTabPage.Controls.Add(this.label9);
            this.numericalCalculationsTabPage.Controls.Add(this.label6);
            this.numericalCalculationsTabPage.Controls.Add(this.methodNumericalCalculationsComboBox);
            this.numericalCalculationsTabPage.Controls.Add(this.operationNumericalCalculationsComboBox);
            this.numericalCalculationsTabPage.Controls.Add(this.numericalOperationButton);
            this.numericalCalculationsTabPage.Controls.Add(this.splitContainer5);
            resources.ApplyResources(this.numericalCalculationsTabPage, "numericalCalculationsTabPage");
            this.numericalCalculationsTabPage.Name = "numericalCalculationsTabPage";
            this.numericalCalculationsTabPage.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // methodNumericalCalculationsComboBox
            // 
            this.methodNumericalCalculationsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.methodNumericalCalculationsComboBox, "methodNumericalCalculationsComboBox");
            this.methodNumericalCalculationsComboBox.FormattingEnabled = true;
            this.methodNumericalCalculationsComboBox.Name = "methodNumericalCalculationsComboBox";
            // 
            // operationNumericalCalculationsComboBox
            // 
            this.operationNumericalCalculationsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.operationNumericalCalculationsComboBox, "operationNumericalCalculationsComboBox");
            this.operationNumericalCalculationsComboBox.FormattingEnabled = true;
            this.operationNumericalCalculationsComboBox.Name = "operationNumericalCalculationsComboBox";
            this.operationNumericalCalculationsComboBox.SelectedIndexChanged += new System.EventHandler(this.operationNumericalCalculationsComboBox_SelectedIndexChanged);
            // 
            // numericalOperationButton
            // 
            resources.ApplyResources(this.numericalOperationButton, "numericalOperationButton");
            this.numericalOperationButton.Name = "numericalOperationButton";
            this.numericalOperationButton.UseVisualStyleBackColor = true;
            this.numericalOperationButton.Click += new System.EventHandler(this.numericalOperationButton_Click);
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
            this.symbolicOperationButton.Click += new System.EventHandler(this.symbolicOperationButton_Click);
            // 
            // scriptingTabPage
            // 
            this.scriptingTabPage.Controls.Add(this.splitContainer1);
            resources.ApplyResources(this.scriptingTabPage, "scriptingTabPage");
            this.scriptingTabPage.Name = "scriptingTabPage";
            this.scriptingTabPage.UseVisualStyleBackColor = true;
            // 
            // customFunctionsTabPage
            // 
            this.customFunctionsTabPage.Controls.Add(this.splitContainer3);
            resources.ApplyResources(this.customFunctionsTabPage, "customFunctionsTabPage");
            this.customFunctionsTabPage.Name = "customFunctionsTabPage";
            this.customFunctionsTabPage.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.expressionTextBox, 1, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // expressionTextBox
            // 
            resources.ApplyResources(this.expressionTextBox, "expressionTextBox");
            this.expressionTextBox.ExponentMode = false;
            this.expressionTextBox.Name = "expressionTextBox";
            this.expressionTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.expressionTextBox_KeyPress);
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
            //this.chartToolStripMenuItem,
            this.transformToolStripMenuItem,
            //this.comlexChartToolStripMenuItem,
            //this.chart3dToolStripMenuItem,
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
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
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
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            resources.ApplyResources(this.saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
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
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // printPreviewToolStripMenuItem
            // 
            resources.ApplyResources(this.printPreviewToolStripMenuItem, "printPreviewToolStripMenuItem");
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.printPreviewToolStripMenuItem_Click);
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
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
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
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            resources.ApplyResources(this.redoToolStripMenuItem, "redoToolStripMenuItem");
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
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
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            resources.ApplyResources(this.pasteToolStripMenuItem, "pasteToolStripMenuItem");
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
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
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
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
            transformToolStripMenuItem.Visible = false;
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
            this.optionsToolStripMenuItem1.Click += new System.EventHandler(this.preferencesToolStripMenuItem_Click);
            // 
            // fullscreenToolStripMenuItem
            // 
            this.fullscreenToolStripMenuItem.CheckOnClick = true;
            this.fullscreenToolStripMenuItem.Name = "fullscreenToolStripMenuItem";
            resources.ApplyResources(this.fullscreenToolStripMenuItem, "fullscreenToolStripMenuItem");
            this.fullscreenToolStripMenuItem.CheckedChanged += new System.EventHandler(this.fullscreenToolStripMenuItem_CheckedChanged);
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
            this.languageToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.languageToolStripComboBox_SelectedIndexChanged_1);
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
            this.toolStripMenuItem40.Click += new System.EventHandler(this.benchmarkToolStripMenuItem_Click);
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
            this.toolStripMenuItem41.Click += new System.EventHandler(this.logsToolStripMenuItem_Click);
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
            this.toolStripMenuItem43.Click += new System.EventHandler(this.featuresToolStripMenuItem_Click);
            // 
            // toolStripMenuItem45
            // 
            this.toolStripMenuItem45.Name = "toolStripMenuItem45";
            resources.ApplyResources(this.toolStripMenuItem45, "toolStripMenuItem45");
            this.toolStripMenuItem45.Click += new System.EventHandler(this.changelogToolStripMenuItem_Click);
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
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
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
            this.toolStripMenuItem44.Click += new System.EventHandler(this.thanksToToolStripMenuItem_Click);
            // 
            // toolStripMenuItem46
            // 
            this.toolStripMenuItem46.Name = "toolStripMenuItem46";
            resources.ApplyResources(this.toolStripMenuItem46, "toolStripMenuItem46");
            this.toolStripMenuItem46.Click += new System.EventHandler(this.bugReportingToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripButton,
            this.openToolStripButton,
            this.saveToolStripButton,
            this.printToolStripButton,
            this.toolStripSeparator6,
            this.cutToolStripButton,
            this.copyToolStripButton,
            this.pasteToolStripButton,
            this.toolStripSeparator7,
            this.helpToolStripButton,
            this.toolStripSeparator17,
            this.runToolStripButton});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // newToolStripButton
            // 
            this.newToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.newToolStripButton, "newToolStripButton");
            this.newToolStripButton.Name = "newToolStripButton";
            this.newToolStripButton.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.openToolStripButton, "openToolStripButton");
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.saveToolStripButton, "saveToolStripButton");
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // printToolStripButton
            // 
            this.printToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.printToolStripButton, "printToolStripButton");
            this.printToolStripButton.Name = "printToolStripButton";
            this.printToolStripButton.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // cutToolStripButton
            // 
            this.cutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.cutToolStripButton, "cutToolStripButton");
            this.cutToolStripButton.Name = "cutToolStripButton";
            this.cutToolStripButton.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripButton
            // 
            this.copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.copyToolStripButton, "copyToolStripButton");
            this.copyToolStripButton.Name = "copyToolStripButton";
            this.copyToolStripButton.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripButton
            // 
            this.pasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.pasteToolStripButton, "pasteToolStripButton");
            this.pasteToolStripButton.Name = "pasteToolStripButton";
            this.pasteToolStripButton.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // helpToolStripButton
            // 
            this.helpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.helpToolStripButton, "helpToolStripButton");
            this.helpToolStripButton.Name = "helpToolStripButton";
            this.helpToolStripButton.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            resources.ApplyResources(this.toolStripSeparator17, "toolStripSeparator17");
            // 
            // runToolStripButton
            // 
            this.runToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.runToolStripButton, "runToolStripButton");
            this.runToolStripButton.Name = "runToolStripButton";
            this.runToolStripButton.Click += new System.EventHandler(this.runToolStripButton_Click);
            // 
            // GUI
            // 
            resources.ApplyResources(this, "$this");
            MinimumSize = new Size(591,628);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip2);
            this.Name = "GUI";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsImZnumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueForCalculationNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsHistoryDataGridView)).EndInit();
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel1.PerformLayout();
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.maxErrorGroupBox.ResumeLayout(false);
            this.maxErrorGroupBox.PerformLayout();
            this.stepsGroupBox.ResumeLayout(false);
            this.stepsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nStepsNumericUpDown)).EndInit();
            this.derivativeAtPointGroupBox.ResumeLayout(false);
            this.derivativeAtPointGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).EndInit();
            this.intervalGroupBox.ResumeLayout(false);
            this.intervalGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.chartingTabPage.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yNNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.y0NumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xnNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.x0NumericUpDown)).EndInit();
            this.calculationsTabPage.ResumeLayout(false);
            this.numericalCalculationsTabPage.ResumeLayout(false);
            this.numericalCalculationsTabPage.PerformLayout();
            this.symbolicCalculationsTabPage.ResumeLayout(false);
            this.scriptingTabPage.ResumeLayout(false);
            this.customFunctionsTabPage.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage chartingTabPage;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabPage calculationsTabPage;
        private System.Windows.Forms.TabPage numericalCalculationsTabPage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox calculationValueTextBox;
        private System.Windows.Forms.Button calculateButton;
        private System.Windows.Forms.NumericUpDown valueForCalculationNumericUpDown;
        private System.Windows.Forms.Label calculationsRealLabel;
        private System.Windows.Forms.TabPage symbolicCalculationsTabPage;
        private System.Windows.Forms.Button numericalOperationButton;
        private System.Windows.Forms.Button symbolicOperationButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private ScientificNumericUpDown yNNumericUpDown;
        private ScientificNumericUpDown y0NumericUpDown;
        private System.Windows.Forms.Label yNlabel;
        private System.Windows.Forms.Label y0label;
        private ScientificNumericUpDown xnNumericUpDown;
        private ScientificNumericUpDown x0NumericUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button clearChartButton;
        private System.Windows.Forms.Button addToChartButton;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ComboBox operationNumericalCalculationsComboBox;
        private System.Windows.Forms.ComboBox methodNumericalCalculationsComboBox;
        private System.Windows.Forms.DataGridView numericalCalculationsDataGridView;
        private System.Windows.Forms.TextBox resultNumericalCalculationsTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox intervalGroupBox;
        private System.Windows.Forms.NumericUpDown bIntervalNumericUpDown;
        private System.Windows.Forms.NumericUpDown aIntervalNumericUpDown;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox derivativeAtPointGroupBox;
        private System.Windows.Forms.NumericUpDown nOrderDerivativeNumericUpDown;
        private System.Windows.Forms.NumericUpDown xDerivativePointNumericUpDown;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.DataGridView calculationsHistoryDataGridView;
        private System.Windows.Forms.NumericUpDown calculationsImZnumericUpDown;
        private System.Windows.Forms.Label calculationsComplexLabel;
        private System.Windows.Forms.TabPage scriptingTabPage;
        private System.Windows.Forms.SplitContainer splitContainer1;

        private System.Windows.Forms.Button processButton;
        private System.Windows.Forms.TabPage customFunctionsTabPage;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox consoleOutputTextBox;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private DirectoryTree customFunctionsDirectoryTree;
        private System.Windows.Forms.OpenFileDialog openCustomFunctionsFileDialog;
        private System.Windows.Forms.SaveFileDialog saveCustomFunctionsFileDialog;
        private System.Windows.Forms.OpenFileDialog openScriptFileDialog;
        private System.Windows.Forms.SaveFileDialog saveScriptFileDialog;
        private System.Windows.Forms.Button openScriptingDirectoryButton;

        private DirectoryTree scriptingDirectoryTree;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripStatusLabel XYRatioToolStripStatusLabel;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private ExpressionTextBox expressionTextBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn function;
        private System.Windows.Forms.DataGridViewTextBoxColumn operation;
        private System.Windows.Forms.DataGridViewTextBoxColumn method;
        private System.Windows.Forms.DataGridViewTextBoxColumn parameters;
        private System.Windows.Forms.DataGridViewTextBoxColumn result;
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
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton newToolStripButton;
        private System.Windows.Forms.ToolStripButton openToolStripButton;
        private System.Windows.Forms.ToolStripButton saveToolStripButton;
        private System.Windows.Forms.ToolStripButton printToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton cutToolStripButton;
        private System.Windows.Forms.ToolStripButton copyToolStripButton;
        private System.Windows.Forms.ToolStripButton pasteToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton helpToolStripButton;
        private System.Windows.Forms.ToolStripDropDownButton modeToolStripDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem dd212ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fdsfdsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mode3DFxyToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.ToolStripButton runToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private BindableToolStripMenuItem exponentiationToolStripMenuItem;
        private NumericUpDown nStepsNumericUpDown;
        private Label label19;
        private TextBox epsTextBox;
        private Label label10;
        private GroupBox maxErrorGroupBox;
        private GroupBox stepsGroupBox;
        private Button openCustomFunctionsDirectoryButton;
        private ToolStripMenuItem fullscreenToolStripMenuItem;
    }
}

