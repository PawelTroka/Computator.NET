using System.Windows.Forms;
using Computator.NET.Desktop.Controls;

namespace Computator.NET.Desktop.Views
{
    partial class NumericalCalculationsView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.resultNumericalCalculationsTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.operationNumericalCalculationsComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.methodNumericalCalculationsComboBox = new System.Windows.Forms.ComboBox();
            this.numericalOperationButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.numericalCalculationsDataGridView = new System.Windows.Forms.DataGridView();
            this.function = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.operation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.method = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parameters = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.intervalGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.aIntervalNumericUpDown = new ScientificNumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.bIntervalNumericUpDown = new ScientificNumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.derivativeAtPointGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.nOrderDerivativeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.xDerivativePointNumericUpDown = new ScientificNumericUpDown();
            this.label14 = new System.Windows.Forms.Label();
            this.maxErrorGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.epsTextBox = new ScientificNumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.stepsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.nStepsNumericUpDown = new ScientificNumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).BeginInit();
            this.flowLayoutPanel3.SuspendLayout();
            this.intervalGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalNumericUpDown)).BeginInit();
            this.derivativeAtPointGroupBox.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).BeginInit();
            this.maxErrorGroupBox.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epsTextBox)).BeginInit();
            this.stepsGroupBox.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nStepsNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // resultNumericalCalculationsTextBox
            // 
            this.resultNumericalCalculationsTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel1.SetColumnSpan(this.resultNumericalCalculationsTextBox, 2);
            this.resultNumericalCalculationsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultNumericalCalculationsTextBox.Font = new System.Drawing.Font("Cambria", 16.2F);
            this.resultNumericalCalculationsTextBox.Location = new System.Drawing.Point(137, 258);
            this.resultNumericalCalculationsTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.resultNumericalCalculationsTextBox.Name = "resultNumericalCalculationsTextBox";
            this.resultNumericalCalculationsTextBox.ReadOnly = true;
            this.resultNumericalCalculationsTextBox.Size = new System.Drawing.Size(638, 39);
            this.resultNumericalCalculationsTextBox.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(44, 263);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 29);
            this.label8.TabIndex = 5;
            this.label8.Text = Localization.Views.NumericalCalculationsView.label8_Text;//"Result:";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(41, 82);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(90, 26);
            this.label9.TabIndex = 7;
            this.label9.Text = Localization.Views.NumericalCalculationsView.label9_Text;
            // 
            // operationNumericalCalculationsComboBox
            // 
            this.operationNumericalCalculationsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.operationNumericalCalculationsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.operationNumericalCalculationsComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.operationNumericalCalculationsComboBox.FormattingEnabled = true;
            this.operationNumericalCalculationsComboBox.Location = new System.Drawing.Point(137, 27);
            this.operationNumericalCalculationsComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.operationNumericalCalculationsComboBox.Name = "operationNumericalCalculationsComboBox";
            this.operationNumericalCalculationsComboBox.Size = new System.Drawing.Size(420, 37);
            this.operationNumericalCalculationsComboBox.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(18, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 26);
            this.label6.TabIndex = 6;
            this.label6.Text = Localization.Views.NumericalCalculationsView.label6_Text; ;
            // 
            // methodNumericalCalculationsComboBox
            // 
            this.methodNumericalCalculationsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.methodNumericalCalculationsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.methodNumericalCalculationsComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.methodNumericalCalculationsComboBox.FormattingEnabled = true;
            this.methodNumericalCalculationsComboBox.Location = new System.Drawing.Point(137, 77);
            this.methodNumericalCalculationsComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.methodNumericalCalculationsComboBox.Name = "methodNumericalCalculationsComboBox";
            this.methodNumericalCalculationsComboBox.Size = new System.Drawing.Size(420, 37);
            this.methodNumericalCalculationsComboBox.TabIndex = 2;
            // 
            // numericalOperationButton
            // 
            this.numericalOperationButton.AutoSize = true;
            this.numericalOperationButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.numericalOperationButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.numericalOperationButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.numericalOperationButton.Location = new System.Drawing.Point(563, 27);
            this.numericalOperationButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.numericalOperationButton.Name = "numericalOperationButton";
            this.tableLayoutPanel1.SetRowSpan(this.numericalOperationButton, 2);
            this.numericalOperationButton.Size = new System.Drawing.Size(179, 96);
            this.numericalOperationButton.TabIndex = 0;
            this.numericalOperationButton.Text = Localization.Views.NumericalCalculationsView.numericalOperationButton_Text; ;
            this.numericalOperationButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.label6, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.numericalCalculationsDataGridView, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.label8, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.resultNumericalCalculationsTextBox, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.operationNumericalCalculationsComboBox, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.methodNumericalCalculationsComboBox, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.numericalOperationButton, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 1, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 10);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(782, 400);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // numericalCalculationsDataGridView
            // 
            this.numericalCalculationsDataGridView.AllowUserToAddRows = false;
            this.numericalCalculationsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.numericalCalculationsDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.numericalCalculationsDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.numericalCalculationsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.numericalCalculationsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.function,
            this.operation,
            this.method,
            this.parameters,
            this.result});
            this.tableLayoutPanel1.SetColumnSpan(this.numericalCalculationsDataGridView, 4);
            this.numericalCalculationsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numericalCalculationsDataGridView.Location = new System.Drawing.Point(3, 301);
            this.numericalCalculationsDataGridView.Margin = new System.Windows.Forms.Padding(3, 2, 7, 2);
            this.numericalCalculationsDataGridView.Name = "numericalCalculationsDataGridView";
            this.numericalCalculationsDataGridView.ReadOnly = true;
            this.numericalCalculationsDataGridView.RowTemplate.Height = 24;
            this.numericalCalculationsDataGridView.RowTemplate.ReadOnly = true;
            this.numericalCalculationsDataGridView.Size = new System.Drawing.Size(772, 97);
            this.numericalCalculationsDataGridView.TabIndex = 5;
            // 
            // function
            // 
            this.function.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.function.DefaultCellStyle = dataGridViewCellStyle1;
            this.function.HeaderText = Localization.Views.NumericalCalculationsView.function_HeaderText; ;
            this.function.Name = "function";
            this.function.ReadOnly = true;
            // 
            // operation
            // 
            this.operation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.operation.HeaderText = Localization.Views.NumericalCalculationsView.operation_HeaderText; ;
            this.operation.Name = "operation";
            this.operation.ReadOnly = true;
            // 
            // method
            // 
            this.method.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.method.HeaderText = Localization.Views.NumericalCalculationsView.method_HeaderText; ;
            this.method.Name = "method";
            this.method.ReadOnly = true;
            // 
            // parameters
            // 
            this.parameters.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.parameters.DefaultCellStyle = dataGridViewCellStyle2;
            this.parameters.HeaderText = Localization.Views.NumericalCalculationsView.parameters_HeaderText; ;
            this.parameters.Name = "parameters";
            this.parameters.ReadOnly = true;
            // 
            // result
            // 
            this.result.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.result.DefaultCellStyle = dataGridViewCellStyle3;
            this.result.HeaderText = Localization.Views.NumericalCalculationsView.result_HeaderText; ;
            this.result.Name = "result";
            this.result.ReadOnly = true;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.SetColumnSpan(this.flowLayoutPanel3, 3);
            this.flowLayoutPanel3.Controls.Add(this.intervalGroupBox);
            this.flowLayoutPanel3.Controls.Add(this.derivativeAtPointGroupBox);
            this.flowLayoutPanel3.Controls.Add(this.maxErrorGroupBox);
            this.flowLayoutPanel3.Controls.Add(this.stepsGroupBox);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(19, 129);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(759, 123);
            this.flowLayoutPanel3.TabIndex = 14;
            // 
            // intervalGroupBox
            // 
            this.intervalGroupBox.AutoSize = true;
            this.intervalGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.intervalGroupBox.Controls.Add(this.tableLayoutPanel2);
            this.intervalGroupBox.Location = new System.Drawing.Point(3, 2);
            this.intervalGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.intervalGroupBox.Name = "intervalGroupBox";
            this.intervalGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.intervalGroupBox.Size = new System.Drawing.Size(256, 81);
            this.intervalGroupBox.TabIndex = 9;
            this.intervalGroupBox.TabStop = false;
            this.intervalGroupBox.Text = Localization.Views.NumericalCalculationsView.intervalGroupBox_Text; ;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.aIntervalNumericUpDown, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label12, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.bIntervalNumericUpDown, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label13, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(250, 62);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // aIntervalNumericUpDown
            // 
            this.aIntervalNumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aIntervalNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.aIntervalNumericUpDown.Font = new System.Drawing.Font("Cambria", 12F);
            this.aIntervalNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.aIntervalNumericUpDown.Location = new System.Drawing.Point(42, 0);
            this.aIntervalNumericUpDown.Margin = new System.Windows.Forms.Padding(0);
            this.aIntervalNumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.aIntervalNumericUpDown.Name = "aIntervalNumericUpDown";
            this.aIntervalNumericUpDown.Size = new System.Drawing.Size(208, 31);
            this.aIntervalNumericUpDown.TabIndex = 2;
            this.aIntervalNumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Addition;
            this.aIntervalNumericUpDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Cambria", 12F);
            this.label12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label12.Location = new System.Drawing.Point(3, 35);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(36, 23);
            this.label12.TabIndex = 1;
            this.label12.Text = "b =";
            // 
            // bIntervalNumericUpDown
            // 
            this.bIntervalNumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bIntervalNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.bIntervalNumericUpDown.Font = new System.Drawing.Font("Cambria", 12F);
            this.bIntervalNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.bIntervalNumericUpDown.Location = new System.Drawing.Point(42, 31);
            this.bIntervalNumericUpDown.Margin = new System.Windows.Forms.Padding(0);
            this.bIntervalNumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.bIntervalNumericUpDown.Name = "bIntervalNumericUpDown";
            this.bIntervalNumericUpDown.Size = new System.Drawing.Size(208, 31);
            this.bIntervalNumericUpDown.TabIndex = 3;
            this.bIntervalNumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Addition;
            this.bIntervalNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Cambria", 12F);
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(4, 4);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(35, 23);
            this.label13.TabIndex = 0;
            this.label13.Text = "a =";
            // 
            // derivativeAtPointGroupBox
            // 
            this.derivativeAtPointGroupBox.AutoSize = true;
            this.derivativeAtPointGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.derivativeAtPointGroupBox.Controls.Add(this.tableLayoutPanel3);
            this.derivativeAtPointGroupBox.Location = new System.Drawing.Point(265, 2);
            this.derivativeAtPointGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.derivativeAtPointGroupBox.Name = "derivativeAtPointGroupBox";
            this.derivativeAtPointGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.derivativeAtPointGroupBox.Size = new System.Drawing.Size(293, 81);
            this.derivativeAtPointGroupBox.TabIndex = 10;
            this.derivativeAtPointGroupBox.TabStop = false;
            this.derivativeAtPointGroupBox.Text = Localization.Views.NumericalCalculationsView.derivativeAtPointGroupBox_Text; ;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.nOrderDerivativeNumericUpDown, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label15, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.xDerivativePointNumericUpDown, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.label14, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(287, 62);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // nOrderDerivativeNumericUpDown
            // 
            this.nOrderDerivativeNumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nOrderDerivativeNumericUpDown.Font = new System.Drawing.Font("Cambria", 12F);
            this.nOrderDerivativeNumericUpDown.Location = new System.Drawing.Point(42, 0);
            this.nOrderDerivativeNumericUpDown.Margin = new System.Windows.Forms.Padding(0);
            this.nOrderDerivativeNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nOrderDerivativeNumericUpDown.Name = "nOrderDerivativeNumericUpDown";
            this.nOrderDerivativeNumericUpDown.Size = new System.Drawing.Size(245, 31);
            this.nOrderDerivativeNumericUpDown.TabIndex = 3;
            this.nOrderDerivativeNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nOrderDerivativeNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label15
            // 
            this.label15.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Cambria", 12F);
            this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label15.Location = new System.Drawing.Point(4, 35);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(35, 23);
            this.label15.TabIndex = 0;
            this.label15.Text = "x =";
            // 
            // xDerivativePointNumericUpDown
            // 
            this.xDerivativePointNumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xDerivativePointNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.xDerivativePointNumericUpDown.Font = new System.Drawing.Font("Cambria", 12F);
            this.xDerivativePointNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.xDerivativePointNumericUpDown.Location = new System.Drawing.Point(42, 31);
            this.xDerivativePointNumericUpDown.Margin = new System.Windows.Forms.Padding(0);
            this.xDerivativePointNumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.xDerivativePointNumericUpDown.Name = "xDerivativePointNumericUpDown";
            this.xDerivativePointNumericUpDown.Size = new System.Drawing.Size(245, 31);
            this.xDerivativePointNumericUpDown.TabIndex = 2;
            this.xDerivativePointNumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Adaptive;
            this.xDerivativePointNumericUpDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // label14
            // 
            this.label14.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Cambria", 12F);
            this.label14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label14.Location = new System.Drawing.Point(3, 4);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(36, 23);
            this.label14.TabIndex = 1;
            this.label14.Text = "n =";
            // 
            // maxErrorGroupBox
            // 
            this.maxErrorGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.maxErrorGroupBox.AutoSize = true;
            this.maxErrorGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.maxErrorGroupBox.Controls.Add(this.tableLayoutPanel5);
            this.maxErrorGroupBox.Location = new System.Drawing.Point(564, 2);
            this.maxErrorGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.maxErrorGroupBox.Name = "maxErrorGroupBox";
            this.maxErrorGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.maxErrorGroupBox.Size = new System.Drawing.Size(191, 50);
            this.maxErrorGroupBox.TabIndex = 11;
            this.maxErrorGroupBox.TabStop = false;
            this.maxErrorGroupBox.Text = Localization.Views.NumericalCalculationsView.maxErrorGroupBox_Text; ;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.AutoSize = true;
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.epsTextBox, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.label10, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(185, 31);
            this.tableLayoutPanel5.TabIndex = 12;
            // 
            // epsTextBox
            // 
            this.epsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.epsTextBox.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            1835008});
            this.epsTextBox.Font = new System.Drawing.Font("Cambria", 12F);
            this.epsTextBox.Location = new System.Drawing.Point(40, 0);
            this.epsTextBox.Margin = new System.Windows.Forms.Padding(0);
            this.epsTextBox.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.epsTextBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            1835008});
            this.epsTextBox.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.epsTextBox.Name = "epsTextBox";
            this.epsTextBox.Size = new System.Drawing.Size(145, 31);
            this.epsTextBox.TabIndex = 12;
            this.epsTextBox.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Multiplication;
            this.epsTextBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            524288});
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Cambria", 12F);
            this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label10.Location = new System.Drawing.Point(3, 4);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 23);
            this.label10.TabIndex = 11;
            this.label10.Text = "ε =";
            // 
            // stepsGroupBox
            // 
            this.stepsGroupBox.AutoSize = true;
            this.stepsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.stepsGroupBox.Controls.Add(this.tableLayoutPanel4);
            this.stepsGroupBox.Location = new System.Drawing.Point(3, 87);
            this.stepsGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.stepsGroupBox.Name = "stepsGroupBox";
            this.stepsGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.stepsGroupBox.Size = new System.Drawing.Size(195, 50);
            this.stepsGroupBox.TabIndex = 10;
            this.stepsGroupBox.TabStop = false;
            this.stepsGroupBox.Text = Localization.Views.NumericalCalculationsView.stepsGroupBox_Text; ;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.Controls.Add(this.nStepsNumericUpDown, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label19, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(189, 31);
            this.tableLayoutPanel4.TabIndex = 9;
            // 
            // nStepsNumericUpDown
            // 
            this.nStepsNumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nStepsNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nStepsNumericUpDown.Font = new System.Drawing.Font("Cambria", 12F);
            this.nStepsNumericUpDown.Location = new System.Drawing.Point(45, 0);
            this.nStepsNumericUpDown.Margin = new System.Windows.Forms.Padding(0);
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
            this.nStepsNumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nStepsNumericUpDown.Name = "nStepsNumericUpDown";
            this.nStepsNumericUpDown.Size = new System.Drawing.Size(144, 31);
            this.nStepsNumericUpDown.TabIndex = 9;
            this.nStepsNumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Multiplication;
            this.nStepsNumericUpDown.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // label19
            // 
            this.label19.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Cambria", 12F);
            this.label19.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label19.Location = new System.Drawing.Point(3, 4);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(39, 23);
            this.label19.TabIndex = 8;
            this.label19.Text = "N =";
            // 
            // NumericalCalculationsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "NumericalCalculationsView";
            this.Size = new System.Drawing.Size(782, 400);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).EndInit();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.intervalGroupBox.ResumeLayout(false);
            this.intervalGroupBox.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalNumericUpDown)).EndInit();
            this.derivativeAtPointGroupBox.ResumeLayout(false);
            this.derivativeAtPointGroupBox.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).EndInit();
            this.maxErrorGroupBox.ResumeLayout(false);
            this.maxErrorGroupBox.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.epsTextBox)).EndInit();
            this.stepsGroupBox.ResumeLayout(false);
            this.stepsGroupBox.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nStepsNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox resultNumericalCalculationsTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button numericalOperationButton;
        private System.Windows.Forms.ComboBox methodNumericalCalculationsComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox operationNumericalCalculationsComboBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private DataGridView numericalCalculationsDataGridView;
        private DataGridViewTextBoxColumn function;
        private DataGridViewTextBoxColumn operation;
        private DataGridViewTextBoxColumn method;
        private DataGridViewTextBoxColumn parameters;
        private DataGridViewTextBoxColumn result;
        private FlowLayoutPanel flowLayoutPanel3;
        private GroupBox intervalGroupBox;
        private TableLayoutPanel tableLayoutPanel2;
        private ScientificNumericUpDown aIntervalNumericUpDown;
        private Label label12;
        private ScientificNumericUpDown bIntervalNumericUpDown;
        private Label label13;
        private GroupBox derivativeAtPointGroupBox;
        private TableLayoutPanel tableLayoutPanel3;
        private NumericUpDown nOrderDerivativeNumericUpDown;
        private Label label15;
        private ScientificNumericUpDown xDerivativePointNumericUpDown;
        private Label label14;
        private GroupBox stepsGroupBox;
        private TableLayoutPanel tableLayoutPanel4;
        private ScientificNumericUpDown nStepsNumericUpDown;
        private Label label19;
        private GroupBox maxErrorGroupBox;
        private TableLayoutPanel tableLayoutPanel5;
        private ScientificNumericUpDown epsTextBox;
        private Label label10;
    }
}
