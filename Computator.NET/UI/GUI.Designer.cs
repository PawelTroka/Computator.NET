using Computator.NET.Charting;
using Computator.NET.Charting.RealCharting;

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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.consoleOutputTextBox = new System.Windows.Forms.RichTextBox();
            this.directoryTree1 = new Computator.NET.DirectoryTree();
            this.saveScriptButton = new System.Windows.Forms.Button();
            this.openScriptButton = new System.Windows.Forms.Button();
            this.processButton = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.customFunctionsDirectoryTree = new Computator.NET.DirectoryTree();
            this.saveCustomFunctions = new System.Windows.Forms.Button();
            this.openCustomFunctions = new System.Windows.Forms.Button();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.calculationsImZnumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.calculationsRealLabel = new System.Windows.Forms.Label();
            this.valueForCalculationNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.calculationsComplexLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.calculationValueTextBox = new Computator.NET.UI.NumericTextBox();
            this.calculateButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.calculationsHistoryDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.plikToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wczytajDaneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eksportujDaneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zakończToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.functionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.elementaryFunctionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.specialFunctionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.constantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mathematicalConstantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.physicalConstantsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.typToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.typeOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.wykresyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.seriesOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.colorsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorsOfChartComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.legendPositionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.położenieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.positionLegendComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.wyrównanieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aligmentLegendComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.editChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editPropertiesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.comlexChartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.countourLinesModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.countourLinesToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.colorAssignmentModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorAssignmentToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chart3dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportChart3dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rescaleChart3dtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chart3dEqualAxesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chart3dFitAxesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editChart3dToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editChart3dPropertiesToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.transformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fFTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iFFTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dSTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iDSTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dCTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iDCTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fHTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iFHTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dHTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.utilsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.benchmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.featuresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.thanksToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changelogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bugReportingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.yNNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.y0NumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.yNlabel = new System.Windows.Forms.Label();
            this.y0label = new System.Windows.Forms.Label();
            this.xnNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.x0NumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.clearChartButton = new System.Windows.Forms.Button();
            this.addToChartButton = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.resultNumericalCalculationsTextBox = new Computator.NET.UI.NumericTextBox();
            this.methodNumericalCalculationsComboBox = new System.Windows.Forms.ComboBox();
            this.operationNumericalCalculationsComboBox = new System.Windows.Forms.ComboBox();
            this.numericalOperationButton = new System.Windows.Forms.Button();
            this.derivativeAtPointGroupBox = new System.Windows.Forms.GroupBox();
            this.epsilonDerrivativeTextBox = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.nOrderDerivativeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.xDerivativePointNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.numericalIntegrationGroupBox = new System.Windows.Forms.GroupBox();
            this.bIntervalIntegrationNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.aIntervalIntegrationNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.rootOfFunctionGroupBox = new System.Windows.Forms.GroupBox();
            this.bFunctionRootNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.aFunctionRootNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.symbolicOperationButton = new System.Windows.Forms.Button();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.expressionTextBox = new Computator.NET.ExpressionTextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.fxyModeRadioBox = new System.Windows.Forms.RadioButton();
            this.realNumbersRadioButton = new System.Windows.Forms.RadioButton();
            this.complexNumbersModeRadioBox = new System.Windows.Forms.RadioButton();
            this.panel4 = new System.Windows.Forms.Panel();
            this.sortExpressionsCheckBox = new System.Windows.Forms.CheckBox();
            this.openCustomFunctionsFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveCustomFunctionsFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openScriptFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveScriptFileDialog = new System.Windows.Forms.SaveFileDialog();
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
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yNNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.y0NumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xnNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.x0NumericUpDown)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.derivativeAtPointGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).BeginInit();
            this.numericalIntegrationGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalIntegrationNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalIntegrationNumericUpDown)).BeginInit();
            this.rootOfFunctionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bFunctionRootNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aFunctionRootNumericUpDown)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel4.SuspendLayout();
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
            this.splitContainer1.Panel2.Controls.Add(this.directoryTree1);
            this.splitContainer1.Panel2.Controls.Add(this.saveScriptButton);
            this.splitContainer1.Panel2.Controls.Add(this.openScriptButton);
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
            resources.ApplyResources(this.consoleOutputTextBox, "consoleOutputTextBox");
            this.consoleOutputTextBox.Name = "consoleOutputTextBox";
            this.consoleOutputTextBox.ReadOnly = true;
            // 
            // directoryTree1
            // 
            resources.ApplyResources(this.directoryTree1, "directoryTree1");
            this.directoryTree1.Drive = null;
            this.directoryTree1.Name = "directoryTree1";
            this.directoryTree1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("directoryTree1.Nodes")))});
            this.directoryTree1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.directoryTree1_AfterSelect);
            // 
            // saveScriptButton
            // 
            resources.ApplyResources(this.saveScriptButton, "saveScriptButton");
            this.saveScriptButton.Name = "saveScriptButton";
            this.saveScriptButton.UseVisualStyleBackColor = true;
            this.saveScriptButton.Click += new System.EventHandler(this.saveScriptButton_Click);
            // 
            // openScriptButton
            // 
            resources.ApplyResources(this.openScriptButton, "openScriptButton");
            this.openScriptButton.Name = "openScriptButton";
            this.openScriptButton.UseVisualStyleBackColor = true;
            this.openScriptButton.Click += new System.EventHandler(this.openScriptButton_Click);
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
            this.splitContainer3.Panel2.Controls.Add(this.saveCustomFunctions);
            this.splitContainer3.Panel2.Controls.Add(this.openCustomFunctions);
            // 
            // customFunctionsDirectoryTree
            // 
            resources.ApplyResources(this.customFunctionsDirectoryTree, "customFunctionsDirectoryTree");
            this.customFunctionsDirectoryTree.Drive = null;
            this.customFunctionsDirectoryTree.Name = "customFunctionsDirectoryTree";
            this.customFunctionsDirectoryTree.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("customFunctionsDirectoryTree.Nodes")))});
            this.customFunctionsDirectoryTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.customFunctionsDirectoryTree_AfterSelect);
            // 
            // saveCustomFunctions
            // 
            resources.ApplyResources(this.saveCustomFunctions, "saveCustomFunctions");
            this.saveCustomFunctions.Name = "saveCustomFunctions";
            this.saveCustomFunctions.UseVisualStyleBackColor = true;
            this.saveCustomFunctions.Click += new System.EventHandler(this.saveCustomFunctions_Click);
            // 
            // openCustomFunctions
            // 
            resources.ApplyResources(this.openCustomFunctions, "openCustomFunctions");
            this.openCustomFunctions.Name = "openCustomFunctions";
            this.openCustomFunctions.UseVisualStyleBackColor = true;
            this.openCustomFunctions.Click += new System.EventHandler(this.openCustomFunctions_Click);
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
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.calculationsHistoryDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.dataGridViewTextBoxColumn1, "dataGridViewTextBoxColumn1");
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.dataGridViewTextBoxColumn4, "dataGridViewTextBoxColumn4");
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.dataGridViewTextBoxColumn5.DefaultCellStyle = dataGridViewCellStyle4;
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
            this.splitContainer5.Panel1.Controls.Add(this.label8);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.numericalCalculationsDataGridView);
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
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.numericalCalculationsDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.numericalCalculationsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.numericalCalculationsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.function,
            this.operation,
            this.method,
            this.parameters,
            this.result});
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.numericalCalculationsDataGridView.DefaultCellStyle = dataGridViewCellStyle9;
            resources.ApplyResources(this.numericalCalculationsDataGridView, "numericalCalculationsDataGridView");
            this.numericalCalculationsDataGridView.Name = "numericalCalculationsDataGridView";
            this.numericalCalculationsDataGridView.ReadOnly = true;
            this.numericalCalculationsDataGridView.RowTemplate.Height = 24;
            this.numericalCalculationsDataGridView.RowTemplate.ReadOnly = true;
            // 
            // function
            // 
            this.function.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.function.DefaultCellStyle = dataGridViewCellStyle6;
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
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.parameters.DefaultCellStyle = dataGridViewCellStyle7;
            resources.ApplyResources(this.parameters, "parameters");
            this.parameters.Name = "parameters";
            this.parameters.ReadOnly = true;
            // 
            // result
            // 
            this.result.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.result.DefaultCellStyle = dataGridViewCellStyle8;
            resources.ApplyResources(this.result, "result");
            this.result.Name = "result";
            this.result.ReadOnly = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.XYRatioToolStripStatusLabel});
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
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Gainsboro;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.plikToolStripMenuItem,
            this.functionsToolStripMenuItem,
            this.constantsToolStripMenuItem,
            this.chartToolStripMenuItem,
            this.comlexChartToolStripMenuItem,
            this.chart3dToolStripMenuItem,
            this.transformToolStripMenuItem,
            this.utilsToolStripMenuItem,
            this.languageToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // plikToolStripMenuItem
            // 
            this.plikToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wczytajDaneToolStripMenuItem,
            this.eksportujDaneToolStripMenuItem,
            this.zakończToolStripMenuItem});
            this.plikToolStripMenuItem.Name = "plikToolStripMenuItem";
            resources.ApplyResources(this.plikToolStripMenuItem, "plikToolStripMenuItem");
            // 
            // wczytajDaneToolStripMenuItem
            // 
            resources.ApplyResources(this.wczytajDaneToolStripMenuItem, "wczytajDaneToolStripMenuItem");
            this.wczytajDaneToolStripMenuItem.Name = "wczytajDaneToolStripMenuItem";
            // 
            // eksportujDaneToolStripMenuItem
            // 
            resources.ApplyResources(this.eksportujDaneToolStripMenuItem, "eksportujDaneToolStripMenuItem");
            this.eksportujDaneToolStripMenuItem.Name = "eksportujDaneToolStripMenuItem";
            // 
            // zakończToolStripMenuItem
            // 
            this.zakończToolStripMenuItem.Name = "zakończToolStripMenuItem";
            resources.ApplyResources(this.zakończToolStripMenuItem, "zakończToolStripMenuItem");
            this.zakończToolStripMenuItem.Click += new System.EventHandler(this.zakończToolStripMenuItem_Click);
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
            // chartToolStripMenuItem
            // 
            this.chartToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportChartToolStripMenuItem,
            this.typToolStripMenuItem,
            this.wykresyToolStripMenuItem,
            this.colorsMenuItem,
            this.legendPositionsMenuItem,
            this.editChartToolStripMenuItem,
            this.editPropertiesToolStripMenuItem1});
            this.chartToolStripMenuItem.Name = "chartToolStripMenuItem";
            resources.ApplyResources(this.chartToolStripMenuItem, "chartToolStripMenuItem");
            // 
            // exportChartToolStripMenuItem
            // 
            this.exportChartToolStripMenuItem.Name = "exportChartToolStripMenuItem";
            resources.ApplyResources(this.exportChartToolStripMenuItem, "exportChartToolStripMenuItem");
            this.exportChartToolStripMenuItem.Click += new System.EventHandler(this.exportChartToolStripMenuItem_Click);
            // 
            // typToolStripMenuItem
            // 
            this.typToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.typeOfChartComboBox});
            this.typToolStripMenuItem.Name = "typToolStripMenuItem";
            resources.ApplyResources(this.typToolStripMenuItem, "typToolStripMenuItem");
            // 
            // typeOfChartComboBox
            // 
            this.typeOfChartComboBox.Name = "typeOfChartComboBox";
            resources.ApplyResources(this.typeOfChartComboBox, "typeOfChartComboBox");
            this.typeOfChartComboBox.SelectedIndexChanged += new System.EventHandler(this.typeOfChartComboBox_SelectedIndexChanged);
            // 
            // wykresyToolStripMenuItem
            // 
            this.wykresyToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.seriesOfChartComboBox});
            this.wykresyToolStripMenuItem.Name = "wykresyToolStripMenuItem";
            resources.ApplyResources(this.wykresyToolStripMenuItem, "wykresyToolStripMenuItem");
            // 
            // seriesOfChartComboBox
            // 
            this.seriesOfChartComboBox.Name = "seriesOfChartComboBox";
            resources.ApplyResources(this.seriesOfChartComboBox, "seriesOfChartComboBox");
            this.seriesOfChartComboBox.SelectedIndexChanged += new System.EventHandler(this.seriesOfChartComboBox_SelectedIndexChanged);
            // 
            // colorsMenuItem
            // 
            this.colorsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorsOfChartComboBox});
            this.colorsMenuItem.Name = "colorsMenuItem";
            resources.ApplyResources(this.colorsMenuItem, "colorsMenuItem");
            // 
            // colorsOfChartComboBox
            // 
            this.colorsOfChartComboBox.Name = "colorsOfChartComboBox";
            resources.ApplyResources(this.colorsOfChartComboBox, "colorsOfChartComboBox");
            this.colorsOfChartComboBox.SelectedIndexChanged += new System.EventHandler(this.colorsOfChartComboBox_SelectedIndexChanged);
            // 
            // legendPositionsMenuItem
            // 
            this.legendPositionsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.położenieToolStripMenuItem,
            this.wyrównanieToolStripMenuItem});
            this.legendPositionsMenuItem.Name = "legendPositionsMenuItem";
            resources.ApplyResources(this.legendPositionsMenuItem, "legendPositionsMenuItem");
            // 
            // położenieToolStripMenuItem
            // 
            this.położenieToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.positionLegendComboBox});
            this.położenieToolStripMenuItem.Name = "położenieToolStripMenuItem";
            resources.ApplyResources(this.położenieToolStripMenuItem, "położenieToolStripMenuItem");
            // 
            // positionLegendComboBox
            // 
            this.positionLegendComboBox.Name = "positionLegendComboBox";
            resources.ApplyResources(this.positionLegendComboBox, "positionLegendComboBox");
            this.positionLegendComboBox.SelectedIndexChanged += new System.EventHandler(this.positionLegendComboBox_SelectedIndexChanged);
            // 
            // wyrównanieToolStripMenuItem
            // 
            this.wyrównanieToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aligmentLegendComboBox});
            this.wyrównanieToolStripMenuItem.Name = "wyrównanieToolStripMenuItem";
            resources.ApplyResources(this.wyrównanieToolStripMenuItem, "wyrównanieToolStripMenuItem");
            // 
            // aligmentLegendComboBox
            // 
            this.aligmentLegendComboBox.Name = "aligmentLegendComboBox";
            resources.ApplyResources(this.aligmentLegendComboBox, "aligmentLegendComboBox");
            this.aligmentLegendComboBox.SelectedIndexChanged += new System.EventHandler(this.aligmentLegendComboBox_SelectedIndexChanged);
            // 
            // editChartToolStripMenuItem
            // 
            this.editChartToolStripMenuItem.Name = "editChartToolStripMenuItem";
            resources.ApplyResources(this.editChartToolStripMenuItem, "editChartToolStripMenuItem");
            this.editChartToolStripMenuItem.Click += new System.EventHandler(this.editChartToolStripMenuItem_Click);
            // 
            // editPropertiesToolStripMenuItem1
            // 
            this.editPropertiesToolStripMenuItem1.Name = "editPropertiesToolStripMenuItem1";
            resources.ApplyResources(this.editPropertiesToolStripMenuItem1, "editPropertiesToolStripMenuItem1");
            this.editPropertiesToolStripMenuItem1.Click += new System.EventHandler(this.editPropertiesToolStripMenuItem1_Click);
            // 
            // comlexChartToolStripMenuItem
            // 
            this.comlexChartToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.countourLinesModeToolStripMenuItem,
            this.colorAssignmentModeToolStripMenuItem,
            this.editToolStripMenuItem,
            this.editPropertiesToolStripMenuItem});
            this.comlexChartToolStripMenuItem.Name = "comlexChartToolStripMenuItem";
            resources.ApplyResources(this.comlexChartToolStripMenuItem, "comlexChartToolStripMenuItem");
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            resources.ApplyResources(this.exportToolStripMenuItem, "exportToolStripMenuItem");
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // countourLinesModeToolStripMenuItem
            // 
            this.countourLinesModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.countourLinesToolStripComboBox});
            this.countourLinesModeToolStripMenuItem.Name = "countourLinesModeToolStripMenuItem";
            resources.ApplyResources(this.countourLinesModeToolStripMenuItem, "countourLinesModeToolStripMenuItem");
            // 
            // countourLinesToolStripComboBox
            // 
            this.countourLinesToolStripComboBox.Name = "countourLinesToolStripComboBox";
            resources.ApplyResources(this.countourLinesToolStripComboBox, "countourLinesToolStripComboBox");
            this.countourLinesToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.countourLinesToolStripComboBox_SelectedIndexChanged);
            // 
            // colorAssignmentModeToolStripMenuItem
            // 
            this.colorAssignmentModeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorAssignmentToolStripComboBox});
            this.colorAssignmentModeToolStripMenuItem.Name = "colorAssignmentModeToolStripMenuItem";
            resources.ApplyResources(this.colorAssignmentModeToolStripMenuItem, "colorAssignmentModeToolStripMenuItem");
            // 
            // colorAssignmentToolStripComboBox
            // 
            this.colorAssignmentToolStripComboBox.Name = "colorAssignmentToolStripComboBox";
            resources.ApplyResources(this.colorAssignmentToolStripComboBox, "colorAssignmentToolStripComboBox");
            this.colorAssignmentToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.colorAssignmentToolStripComboBox_SelectedIndexChanged);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // editPropertiesToolStripMenuItem
            // 
            this.editPropertiesToolStripMenuItem.Name = "editPropertiesToolStripMenuItem";
            resources.ApplyResources(this.editPropertiesToolStripMenuItem, "editPropertiesToolStripMenuItem");
            this.editPropertiesToolStripMenuItem.Click += new System.EventHandler(this.editPropertiesToolStripMenuItem_Click);
            // 
            // chart3dToolStripMenuItem
            // 
            this.chart3dToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportChart3dToolStripMenuItem,
            this.rescaleChart3dtoolStripMenuItem,
            this.editChart3dToolStripMenuItem,
            this.editChart3dPropertiesToolStripMenuItem2});
            this.chart3dToolStripMenuItem.Name = "chart3dToolStripMenuItem";
            resources.ApplyResources(this.chart3dToolStripMenuItem, "chart3dToolStripMenuItem");
            // 
            // exportChart3dToolStripMenuItem
            // 
            this.exportChart3dToolStripMenuItem.Name = "exportChart3dToolStripMenuItem";
            resources.ApplyResources(this.exportChart3dToolStripMenuItem, "exportChart3dToolStripMenuItem");
            this.exportChart3dToolStripMenuItem.Click += new System.EventHandler(this.exportChart3dToolStripMenuItem_Click);
            // 
            // rescaleChart3dtoolStripMenuItem
            // 
            this.rescaleChart3dtoolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chart3dEqualAxesToolStripMenuItem,
            this.chart3dFitAxesToolStripMenuItem});
            this.rescaleChart3dtoolStripMenuItem.Name = "rescaleChart3dtoolStripMenuItem";
            resources.ApplyResources(this.rescaleChart3dtoolStripMenuItem, "rescaleChart3dtoolStripMenuItem");
            // 
            // chart3dEqualAxesToolStripMenuItem
            // 
            this.chart3dEqualAxesToolStripMenuItem.Name = "chart3dEqualAxesToolStripMenuItem";
            resources.ApplyResources(this.chart3dEqualAxesToolStripMenuItem, "chart3dEqualAxesToolStripMenuItem");
            this.chart3dEqualAxesToolStripMenuItem.Click += new System.EventHandler(this.chart3dEqualAxesToolStripMenuItem_Click);
            // 
            // chart3dFitAxesToolStripMenuItem
            // 
            this.chart3dFitAxesToolStripMenuItem.Name = "chart3dFitAxesToolStripMenuItem";
            resources.ApplyResources(this.chart3dFitAxesToolStripMenuItem, "chart3dFitAxesToolStripMenuItem");
            this.chart3dFitAxesToolStripMenuItem.Click += new System.EventHandler(this.chart3dFitAxesToolStripMenuItem_Click);
            // 
            // editChart3dToolStripMenuItem
            // 
            this.editChart3dToolStripMenuItem.Name = "editChart3dToolStripMenuItem";
            resources.ApplyResources(this.editChart3dToolStripMenuItem, "editChart3dToolStripMenuItem");
            this.editChart3dToolStripMenuItem.Click += new System.EventHandler(this.editChart3dToolStripMenuItem_Click);
            // 
            // editChart3dPropertiesToolStripMenuItem2
            // 
            this.editChart3dPropertiesToolStripMenuItem2.Name = "editChart3dPropertiesToolStripMenuItem2";
            resources.ApplyResources(this.editChart3dPropertiesToolStripMenuItem2, "editChart3dPropertiesToolStripMenuItem2");
            this.editChart3dPropertiesToolStripMenuItem2.Click += new System.EventHandler(this.editChart3dPropertiesToolStripMenuItem2_Click);
            // 
            // transformToolStripMenuItem
            // 
            this.transformToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fFTToolStripMenuItem,
            this.iFFTToolStripMenuItem,
            this.dSTToolStripMenuItem,
            this.iDSTToolStripMenuItem,
            this.dCTToolStripMenuItem,
            this.iDCTToolStripMenuItem,
            this.fHTToolStripMenuItem,
            this.iFHTToolStripMenuItem,
            this.dHTToolStripMenuItem});
            this.transformToolStripMenuItem.Name = "transformToolStripMenuItem";
            resources.ApplyResources(this.transformToolStripMenuItem, "transformToolStripMenuItem");
            // 
            // fFTToolStripMenuItem
            // 
            this.fFTToolStripMenuItem.Name = "fFTToolStripMenuItem";
            resources.ApplyResources(this.fFTToolStripMenuItem, "fFTToolStripMenuItem");
            this.fFTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // iFFTToolStripMenuItem
            // 
            this.iFFTToolStripMenuItem.Name = "iFFTToolStripMenuItem";
            resources.ApplyResources(this.iFFTToolStripMenuItem, "iFFTToolStripMenuItem");
            this.iFFTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // dSTToolStripMenuItem
            // 
            this.dSTToolStripMenuItem.Name = "dSTToolStripMenuItem";
            resources.ApplyResources(this.dSTToolStripMenuItem, "dSTToolStripMenuItem");
            this.dSTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // iDSTToolStripMenuItem
            // 
            this.iDSTToolStripMenuItem.Name = "iDSTToolStripMenuItem";
            resources.ApplyResources(this.iDSTToolStripMenuItem, "iDSTToolStripMenuItem");
            this.iDSTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // dCTToolStripMenuItem
            // 
            this.dCTToolStripMenuItem.Name = "dCTToolStripMenuItem";
            resources.ApplyResources(this.dCTToolStripMenuItem, "dCTToolStripMenuItem");
            this.dCTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // iDCTToolStripMenuItem
            // 
            this.iDCTToolStripMenuItem.Name = "iDCTToolStripMenuItem";
            resources.ApplyResources(this.iDCTToolStripMenuItem, "iDCTToolStripMenuItem");
            this.iDCTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // fHTToolStripMenuItem
            // 
            this.fHTToolStripMenuItem.Name = "fHTToolStripMenuItem";
            resources.ApplyResources(this.fHTToolStripMenuItem, "fHTToolStripMenuItem");
            this.fHTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // iFHTToolStripMenuItem
            // 
            this.iFHTToolStripMenuItem.Name = "iFHTToolStripMenuItem";
            resources.ApplyResources(this.iFHTToolStripMenuItem, "iFHTToolStripMenuItem");
            this.iFHTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // dHTToolStripMenuItem
            // 
            this.dHTToolStripMenuItem.Name = "dHTToolStripMenuItem";
            resources.ApplyResources(this.dHTToolStripMenuItem, "dHTToolStripMenuItem");
            this.dHTToolStripMenuItem.Click += new System.EventHandler(this.transformToolStripMenuItem_Click);
            // 
            // utilsToolStripMenuItem
            // 
            this.utilsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.benchmarkToolStripMenuItem});
            this.utilsToolStripMenuItem.Name = "utilsToolStripMenuItem";
            resources.ApplyResources(this.utilsToolStripMenuItem, "utilsToolStripMenuItem");
            // 
            // benchmarkToolStripMenuItem
            // 
            this.benchmarkToolStripMenuItem.Name = "benchmarkToolStripMenuItem";
            resources.ApplyResources(this.benchmarkToolStripMenuItem, "benchmarkToolStripMenuItem");
            this.benchmarkToolStripMenuItem.Click += new System.EventHandler(this.benchmarkToolStripMenuItem_Click);
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.languageToolStripComboBox});
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            resources.ApplyResources(this.languageToolStripMenuItem, "languageToolStripMenuItem");
            // 
            // languageToolStripComboBox
            // 
            this.languageToolStripComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageToolStripComboBox.Name = "languageToolStripComboBox";
            resources.ApplyResources(this.languageToolStripComboBox, "languageToolStripComboBox");
            this.languageToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.languageToolStripComboBox_SelectedIndexChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.featuresToolStripMenuItem,
            this.thanksToToolStripMenuItem,
            this.changelogToolStripMenuItem,
            this.bugReportingToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // featuresToolStripMenuItem
            // 
            this.featuresToolStripMenuItem.Name = "featuresToolStripMenuItem";
            resources.ApplyResources(this.featuresToolStripMenuItem, "featuresToolStripMenuItem");
            this.featuresToolStripMenuItem.Click += new System.EventHandler(this.featuresToolStripMenuItem_Click);
            // 
            // thanksToToolStripMenuItem
            // 
            this.thanksToToolStripMenuItem.Name = "thanksToToolStripMenuItem";
            resources.ApplyResources(this.thanksToToolStripMenuItem, "thanksToToolStripMenuItem");
            this.thanksToToolStripMenuItem.Click += new System.EventHandler(this.thanksToToolStripMenuItem_Click);
            // 
            // changelogToolStripMenuItem
            // 
            this.changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            resources.ApplyResources(this.changelogToolStripMenuItem, "changelogToolStripMenuItem");
            this.changelogToolStripMenuItem.Click += new System.EventHandler(this.changelogToolStripMenuItem_Click);
            // 
            // bugReportingToolStripMenuItem
            // 
            this.bugReportingToolStripMenuItem.Name = "bugReportingToolStripMenuItem";
            resources.ApplyResources(this.bugReportingToolStripMenuItem, "bugReportingToolStripMenuItem");
            this.bugReportingToolStripMenuItem.Click += new System.EventHandler(this.bugReportingToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage6);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel2);
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
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
            this.yNNumericUpDown.Maximum = new decimal(new int[] {
            -2080374784,
            2047605301,
            430428262,
            0});
            this.yNNumericUpDown.Minimum = new decimal(new int[] {
            -2080374784,
            2047605301,
            430428262,
            -2147483648});
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
            this.y0NumericUpDown.Maximum = new decimal(new int[] {
            -2080374784,
            2047605301,
            430428262,
            0});
            this.y0NumericUpDown.Minimum = new decimal(new int[] {
            -2080374784,
            2047605301,
            430428262,
            -2147483648});
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
            this.xnNumericUpDown.Maximum = new decimal(new int[] {
            -2080374784,
            2047605301,
            430428262,
            0});
            this.xnNumericUpDown.Minimum = new decimal(new int[] {
            -2080374784,
            2047605301,
            430428262,
            -2147483648});
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
            this.x0NumericUpDown.Maximum = new decimal(new int[] {
            -2080374784,
            2047605301,
            430428262,
            0});
            this.x0NumericUpDown.Minimum = new decimal(new int[] {
            -2080374784,
            2047605301,
            430428262,
            -2147483648});
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
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer4);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label9);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Controls.Add(this.resultNumericalCalculationsTextBox);
            this.tabPage3.Controls.Add(this.methodNumericalCalculationsComboBox);
            this.tabPage3.Controls.Add(this.operationNumericalCalculationsComboBox);
            this.tabPage3.Controls.Add(this.numericalOperationButton);
            this.tabPage3.Controls.Add(this.derivativeAtPointGroupBox);
            this.tabPage3.Controls.Add(this.numericalIntegrationGroupBox);
            this.tabPage3.Controls.Add(this.rootOfFunctionGroupBox);
            this.tabPage3.Controls.Add(this.splitContainer5);
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
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
            // resultNumericalCalculationsTextBox
            // 
            resources.ApplyResources(this.resultNumericalCalculationsTextBox, "resultNumericalCalculationsTextBox");
            this.resultNumericalCalculationsTextBox.Name = "resultNumericalCalculationsTextBox";
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
            // derivativeAtPointGroupBox
            // 
            this.derivativeAtPointGroupBox.Controls.Add(this.epsilonDerrivativeTextBox);
            this.derivativeAtPointGroupBox.Controls.Add(this.label16);
            this.derivativeAtPointGroupBox.Controls.Add(this.nOrderDerivativeNumericUpDown);
            this.derivativeAtPointGroupBox.Controls.Add(this.xDerivativePointNumericUpDown);
            this.derivativeAtPointGroupBox.Controls.Add(this.label14);
            this.derivativeAtPointGroupBox.Controls.Add(this.label15);
            resources.ApplyResources(this.derivativeAtPointGroupBox, "derivativeAtPointGroupBox");
            this.derivativeAtPointGroupBox.Name = "derivativeAtPointGroupBox";
            this.derivativeAtPointGroupBox.TabStop = false;
            // 
            // epsilonDerrivativeTextBox
            // 
            resources.ApplyResources(this.epsilonDerrivativeTextBox, "epsilonDerrivativeTextBox");
            this.epsilonDerrivativeTextBox.Name = "epsilonDerrivativeTextBox";
            this.epsilonDerrivativeTextBox.TextChanged += new System.EventHandler(this.epsilonDerrivativeTextBox_TextChanged);
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.Name = "label16";
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
            // numericalIntegrationGroupBox
            // 
            this.numericalIntegrationGroupBox.Controls.Add(this.bIntervalIntegrationNumericUpDown);
            this.numericalIntegrationGroupBox.Controls.Add(this.aIntervalIntegrationNumericUpDown);
            this.numericalIntegrationGroupBox.Controls.Add(this.label11);
            this.numericalIntegrationGroupBox.Controls.Add(this.label10);
            resources.ApplyResources(this.numericalIntegrationGroupBox, "numericalIntegrationGroupBox");
            this.numericalIntegrationGroupBox.Name = "numericalIntegrationGroupBox";
            this.numericalIntegrationGroupBox.TabStop = false;
            // 
            // bIntervalIntegrationNumericUpDown
            // 
            this.bIntervalIntegrationNumericUpDown.DecimalPlaces = 9;
            resources.ApplyResources(this.bIntervalIntegrationNumericUpDown, "bIntervalIntegrationNumericUpDown");
            this.bIntervalIntegrationNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.bIntervalIntegrationNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.bIntervalIntegrationNumericUpDown.Minimum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            -2147483648});
            this.bIntervalIntegrationNumericUpDown.Name = "bIntervalIntegrationNumericUpDown";
            this.bIntervalIntegrationNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // aIntervalIntegrationNumericUpDown
            // 
            this.aIntervalIntegrationNumericUpDown.DecimalPlaces = 9;
            resources.ApplyResources(this.aIntervalIntegrationNumericUpDown, "aIntervalIntegrationNumericUpDown");
            this.aIntervalIntegrationNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.aIntervalIntegrationNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.aIntervalIntegrationNumericUpDown.Minimum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            -2147483648});
            this.aIntervalIntegrationNumericUpDown.Name = "aIntervalIntegrationNumericUpDown";
            this.aIntervalIntegrationNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // rootOfFunctionGroupBox
            // 
            this.rootOfFunctionGroupBox.Controls.Add(this.bFunctionRootNumericUpDown);
            this.rootOfFunctionGroupBox.Controls.Add(this.aFunctionRootNumericUpDown);
            this.rootOfFunctionGroupBox.Controls.Add(this.label12);
            this.rootOfFunctionGroupBox.Controls.Add(this.label13);
            resources.ApplyResources(this.rootOfFunctionGroupBox, "rootOfFunctionGroupBox");
            this.rootOfFunctionGroupBox.Name = "rootOfFunctionGroupBox";
            this.rootOfFunctionGroupBox.TabStop = false;
            // 
            // bFunctionRootNumericUpDown
            // 
            this.bFunctionRootNumericUpDown.DecimalPlaces = 9;
            resources.ApplyResources(this.bFunctionRootNumericUpDown, "bFunctionRootNumericUpDown");
            this.bFunctionRootNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.bFunctionRootNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.bFunctionRootNumericUpDown.Minimum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            -2147483648});
            this.bFunctionRootNumericUpDown.Name = "bFunctionRootNumericUpDown";
            this.bFunctionRootNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // aFunctionRootNumericUpDown
            // 
            this.aFunctionRootNumericUpDown.DecimalPlaces = 9;
            resources.ApplyResources(this.aFunctionRootNumericUpDown, "aFunctionRootNumericUpDown");
            this.aFunctionRootNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.aFunctionRootNumericUpDown.Maximum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            0});
            this.aFunctionRootNumericUpDown.Minimum = new decimal(new int[] {
            -469762048,
            -590869294,
            5421010,
            -2147483648});
            this.aFunctionRootNumericUpDown.Name = "aFunctionRootNumericUpDown";
            this.aFunctionRootNumericUpDown.Value = new decimal(new int[] {
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
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.symbolicOperationButton);
            resources.ApplyResources(this.tabPage4, "tabPage4");
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // symbolicOperationButton
            // 
            resources.ApplyResources(this.symbolicOperationButton, "symbolicOperationButton");
            this.symbolicOperationButton.Name = "symbolicOperationButton";
            this.symbolicOperationButton.UseVisualStyleBackColor = true;
            this.symbolicOperationButton.Click += new System.EventHandler(this.symbolicOperationButton_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.splitContainer1);
            resources.ApplyResources(this.tabPage5, "tabPage5");
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.splitContainer3);
            resources.ApplyResources(this.tabPage6, "tabPage6");
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.expressionTextBox);
            this.panel3.Controls.Add(this.panel5);
            this.panel3.Controls.Add(this.groupBox1);
            this.panel3.Controls.Add(this.panel4);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // expressionTextBox
            // 
            resources.ApplyResources(this.expressionTextBox, "expressionTextBox");
            this.expressionTextBox.Name = "expressionTextBox";
            this.expressionTextBox.Sort = false;
            this.expressionTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.expressionTextBox_KeyPress);
            // 
            // panel5
            // 
            resources.ApplyResources(this.panel5, "panel5");
            this.panel5.Name = "panel5";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.fxyModeRadioBox);
            this.groupBox1.Controls.Add(this.realNumbersRadioButton);
            this.groupBox1.Controls.Add(this.complexNumbersModeRadioBox);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // fxyModeRadioBox
            // 
            resources.ApplyResources(this.fxyModeRadioBox, "fxyModeRadioBox");
            this.fxyModeRadioBox.Name = "fxyModeRadioBox";
            this.fxyModeRadioBox.UseVisualStyleBackColor = true;
            this.fxyModeRadioBox.CheckedChanged += new System.EventHandler(this.realNumbersRadioButton_CheckedChanged);
            // 
            // realNumbersRadioButton
            // 
            resources.ApplyResources(this.realNumbersRadioButton, "realNumbersRadioButton");
            this.realNumbersRadioButton.Checked = true;
            this.realNumbersRadioButton.Name = "realNumbersRadioButton";
            this.realNumbersRadioButton.TabStop = true;
            this.realNumbersRadioButton.UseVisualStyleBackColor = true;
            this.realNumbersRadioButton.CheckedChanged += new System.EventHandler(this.realNumbersRadioButton_CheckedChanged);
            // 
            // complexNumbersModeRadioBox
            // 
            resources.ApplyResources(this.complexNumbersModeRadioBox, "complexNumbersModeRadioBox");
            this.complexNumbersModeRadioBox.Name = "complexNumbersModeRadioBox";
            this.complexNumbersModeRadioBox.UseVisualStyleBackColor = true;
            this.complexNumbersModeRadioBox.CheckedChanged += new System.EventHandler(this.realNumbersRadioButton_CheckedChanged);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.sortExpressionsCheckBox);
            this.panel4.Controls.Add(this.label1);
            resources.ApplyResources(this.panel4, "panel4");
            this.panel4.Name = "panel4";
            // 
            // sortExpressionsCheckBox
            // 
            resources.ApplyResources(this.sortExpressionsCheckBox, "sortExpressionsCheckBox");
            this.sortExpressionsCheckBox.Name = "sortExpressionsCheckBox";
            this.sortExpressionsCheckBox.UseVisualStyleBackColor = true;
            this.sortExpressionsCheckBox.CheckedChanged += new System.EventHandler(this.sortExpressionsCheckBox_CheckedChanged);
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
            // GUI
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
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
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
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
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.derivativeAtPointGroupBox.ResumeLayout(false);
            this.derivativeAtPointGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).EndInit();
            this.numericalIntegrationGroupBox.ResumeLayout(false);
            this.numericalIntegrationGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalIntegrationNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalIntegrationNumericUpDown)).EndInit();
            this.rootOfFunctionGroupBox.ResumeLayout(false);
            this.rootOfFunctionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bFunctionRootNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aFunctionRootNumericUpDown)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem plikToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wczytajDaneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eksportujDaneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zakończToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chartToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label7;
        private Computator.NET.UI.NumericTextBox calculationValueTextBox;
        private System.Windows.Forms.Button calculateButton;
        private System.Windows.Forms.NumericUpDown valueForCalculationNumericUpDown;
        private System.Windows.Forms.Label calculationsRealLabel;
        private System.Windows.Forms.ToolStripMenuItem exportChartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem typToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox typeOfChartComboBox;
        private System.Windows.Forms.ToolStripMenuItem wykresyToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox seriesOfChartComboBox;
        private System.Windows.Forms.ToolStripMenuItem colorsMenuItem;
        private System.Windows.Forms.ToolStripComboBox colorsOfChartComboBox;
        private System.Windows.Forms.ToolStripMenuItem legendPositionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem położenieToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox positionLegendComboBox;
        private System.Windows.Forms.ToolStripMenuItem wyrównanieToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox aligmentLegendComboBox;
        private System.Windows.Forms.ToolStripMenuItem editChartToolStripMenuItem;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton realNumbersRadioButton;
        private System.Windows.Forms.RadioButton complexNumbersModeRadioBox;
        private System.Windows.Forms.TabPage tabPage4;

        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.Button numericalOperationButton;
        private System.Windows.Forms.Button symbolicOperationButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown yNNumericUpDown;
        private System.Windows.Forms.NumericUpDown y0NumericUpDown;
        private System.Windows.Forms.Label yNlabel;
        private System.Windows.Forms.Label y0label;
        private System.Windows.Forms.NumericUpDown xnNumericUpDown;
        private System.Windows.Forms.NumericUpDown x0NumericUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button clearChartButton;
        private System.Windows.Forms.Button addToChartButton;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;

        private System.Windows.Forms.ToolStripMenuItem comlexChartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem countourLinesModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox countourLinesToolStripComboBox;
        private System.Windows.Forms.ToolStripMenuItem colorAssignmentModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox colorAssignmentToolStripComboBox;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ComboBox operationNumericalCalculationsComboBox;
        private System.Windows.Forms.ComboBox methodNumericalCalculationsComboBox;
        private System.Windows.Forms.DataGridView numericalCalculationsDataGridView;
        private Computator.NET.UI.NumericTextBox resultNumericalCalculationsTextBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox numericalIntegrationGroupBox;
        private System.Windows.Forms.NumericUpDown bIntervalIntegrationNumericUpDown;
        private System.Windows.Forms.NumericUpDown aIntervalIntegrationNumericUpDown;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox rootOfFunctionGroupBox;
        private System.Windows.Forms.NumericUpDown bFunctionRootNumericUpDown;
        private System.Windows.Forms.NumericUpDown aFunctionRootNumericUpDown;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox derivativeAtPointGroupBox;
        private System.Windows.Forms.NumericUpDown nOrderDerivativeNumericUpDown;
        private System.Windows.Forms.NumericUpDown xDerivativePointNumericUpDown;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox epsilonDerrivativeTextBox;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.DataGridView calculationsHistoryDataGridView;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown calculationsImZnumericUpDown;
        private System.Windows.Forms.Label calculationsComplexLabel;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.SplitContainer splitContainer1;

        private System.Windows.Forms.Button processButton;
        private System.Windows.Forms.ToolStripMenuItem featuresToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem thanksToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changelogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bugReportingToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox consoleOutputTextBox;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private DirectoryTree customFunctionsDirectoryTree;
        private System.Windows.Forms.Button saveCustomFunctions;
        private System.Windows.Forms.Button openCustomFunctions;
        private System.Windows.Forms.OpenFileDialog openCustomFunctionsFileDialog;
        private System.Windows.Forms.SaveFileDialog saveCustomFunctionsFileDialog;
        private System.Windows.Forms.OpenFileDialog openScriptFileDialog;
        private System.Windows.Forms.SaveFileDialog saveScriptFileDialog;
        private System.Windows.Forms.Button openScriptButton;
        private System.Windows.Forms.Button saveScriptButton;

        private DirectoryTree directoryTree1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripStatusLabel XYRatioToolStripStatusLabel;
        private System.Windows.Forms.ToolStripComboBox languageToolStripComboBox;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.ToolStripMenuItem chart3dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportChart3dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rescaleChart3dtoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chart3dEqualAxesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chart3dFitAxesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editChart3dToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editChart3dPropertiesToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem editPropertiesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem editPropertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem utilsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem benchmarkToolStripMenuItem;
        private System.Windows.Forms.RadioButton fxyModeRadioBox;
        private System.Windows.Forms.Panel panel4;
        private ExpressionTextBox expressionTextBox;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox sortExpressionsCheckBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ToolStripMenuItem transformToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fFTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iFFTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dSTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iDSTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dCTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iDCTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fHTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iFHTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dHTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem functionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem elementaryFunctionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem specialFunctionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem constantsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mathematicalConstantsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem physicalConstantsToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn function;
        private System.Windows.Forms.DataGridViewTextBoxColumn operation;
        private System.Windows.Forms.DataGridViewTextBoxColumn method;
        private System.Windows.Forms.DataGridViewTextBoxColumn parameters;
        private System.Windows.Forms.DataGridViewTextBoxColumn result;
    }
}

