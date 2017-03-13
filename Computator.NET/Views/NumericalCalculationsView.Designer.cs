using Computator.NET.Controls;
using System.Windows.Forms;

namespace Computator.NET.Views
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NumericalCalculationsView));
            this.resultNumericalCalculationsTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.numericalCalculationsDataGridView = new System.Windows.Forms.DataGridView();
            this.function = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.operation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.method = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.parameters = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label9 = new System.Windows.Forms.Label();
            this.operationNumericalCalculationsComboBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.methodNumericalCalculationsComboBox = new System.Windows.Forms.ComboBox();
            this.numericalOperationButton = new System.Windows.Forms.Button();
            this.derivativeAtPointGroupBox = new System.Windows.Forms.GroupBox();
            this.stepsGroupBox = new System.Windows.Forms.GroupBox();
            this.intervalGroupBox = new System.Windows.Forms.GroupBox();
            this.maxErrorGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label10 = new Computator.NET.Controls.Field();
            this.epsTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new Computator.NET.Controls.Field();
            this.bIntervalNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label13 = new Computator.NET.Controls.Field();
            this.aIntervalNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label19 = new Computator.NET.Controls.Field();
            this.nStepsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label14 = new Computator.NET.Controls.Field();
            this.nOrderDerivativeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label15 = new Computator.NET.Controls.Field();
            this.xDerivativePointNumericUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).BeginInit();
            this.derivativeAtPointGroupBox.SuspendLayout();
            this.stepsGroupBox.SuspendLayout();
            this.intervalGroupBox.SuspendLayout();
            this.maxErrorGroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.label10.SuspendLayout();
            this.label12.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalNumericUpDown)).BeginInit();
            this.label13.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalNumericUpDown)).BeginInit();
            this.label19.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nStepsNumericUpDown)).BeginInit();
            this.label14.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).BeginInit();
            this.label15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // resultNumericalCalculationsTextBox
            // 
            this.resultNumericalCalculationsTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel1.SetColumnSpan(this.resultNumericalCalculationsTextBox, 2);
            resources.ApplyResources(this.resultNumericalCalculationsTextBox, "resultNumericalCalculationsTextBox");
            this.resultNumericalCalculationsTextBox.Name = "resultNumericalCalculationsTextBox";
            this.resultNumericalCalculationsTextBox.ReadOnly = true;
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
            this.tableLayoutPanel1.SetColumnSpan(this.numericalCalculationsDataGridView, 4);
            resources.ApplyResources(this.numericalCalculationsDataGridView, "numericalCalculationsDataGridView");
            this.numericalCalculationsDataGridView.Name = "numericalCalculationsDataGridView";
            this.numericalCalculationsDataGridView.ReadOnly = true;
            this.numericalCalculationsDataGridView.RowTemplate.Height = 24;
            this.numericalCalculationsDataGridView.RowTemplate.ReadOnly = true;
            this.numericalCalculationsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.numericalCalculationsDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            this.numericalCalculationsDataGridView.AutoSize = true;

            // 
            // function
            // 
            this.function.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.function.DefaultCellStyle = dataGridViewCellStyle1;
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
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.parameters.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.parameters, "parameters");
            this.parameters.Name = "parameters";
            this.parameters.ReadOnly = true;
            // 
            // result
            // 
            this.result.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Cambria", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.result.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.result, "result");
            this.result.Name = "result";
            this.result.ReadOnly = true;
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // operationNumericalCalculationsComboBox
            // 
            this.operationNumericalCalculationsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.operationNumericalCalculationsComboBox, "operationNumericalCalculationsComboBox");
            this.operationNumericalCalculationsComboBox.FormattingEnabled = true;
            this.operationNumericalCalculationsComboBox.Name = "operationNumericalCalculationsComboBox";
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
            // numericalOperationButton
            // 
            resources.ApplyResources(this.numericalOperationButton, "numericalOperationButton");
            this.numericalOperationButton.Name = "numericalOperationButton";
            this.tableLayoutPanel1.SetRowSpan(this.numericalOperationButton, 2);
            this.numericalOperationButton.UseVisualStyleBackColor = true;
            // 
            // derivativeAtPointGroupBox
            // 
            resources.ApplyResources(this.derivativeAtPointGroupBox, "derivativeAtPointGroupBox");
            this.derivativeAtPointGroupBox.Controls.Add(this.label14);
            this.derivativeAtPointGroupBox.Controls.Add(this.label15);
            this.derivativeAtPointGroupBox.Name = "derivativeAtPointGroupBox";
            this.derivativeAtPointGroupBox.TabStop = false;
            // 
            // stepsGroupBox
            // 
            resources.ApplyResources(this.stepsGroupBox, "stepsGroupBox");
            this.stepsGroupBox.Controls.Add(this.label19);
            this.stepsGroupBox.Name = "stepsGroupBox";
            this.stepsGroupBox.TabStop = false;
            // 
            // intervalGroupBox
            // 
            resources.ApplyResources(this.intervalGroupBox, "intervalGroupBox");
            this.intervalGroupBox.Controls.Add(this.label12);
            this.intervalGroupBox.Controls.Add(this.label13);
            this.intervalGroupBox.Name = "intervalGroupBox";
            this.intervalGroupBox.TabStop = false;
            // 
            // maxErrorGroupBox
            // 
            resources.ApplyResources(this.maxErrorGroupBox, "maxErrorGroupBox");
            this.maxErrorGroupBox.Controls.Add(this.label10);
            this.maxErrorGroupBox.Name = "maxErrorGroupBox";
            this.maxErrorGroupBox.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.maxErrorGroupBox, 3, 6);
            this.tableLayoutPanel1.Controls.Add(this.intervalGroupBox, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.stepsGroupBox, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.derivativeAtPointGroupBox, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.numericalOperationButton, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.methodNumericalCalculationsComboBox, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label6, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.operationNumericalCalculationsComboBox, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.numericalCalculationsDataGridView, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.label8, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.resultNumericalCalculationsTextBox, 2, 7);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Controls.Add(this.epsTextBox);
            this.label10.Name = "label10";
            // 
            // epsTextBox
            // 
            resources.ApplyResources(this.epsTextBox, "epsTextBox");
            this.epsTextBox.Name = "epsTextBox";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Controls.Add(this.bIntervalNumericUpDown);
            this.label12.Name = "label12";
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
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Controls.Add(this.aIntervalNumericUpDown);
            this.label13.Name = "label13";
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
            // label19
            // 
            resources.ApplyResources(this.label19, "label19");
            this.label19.Controls.Add(this.nStepsNumericUpDown);
            this.label19.Name = "label19";
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
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Controls.Add(this.nOrderDerivativeNumericUpDown);
            this.label14.Name = "label14";
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
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Controls.Add(this.xDerivativePointNumericUpDown);
            this.label15.Name = "label15";
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
            // NumericalCalculationsView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "NumericalCalculationsView";
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).EndInit();
            this.derivativeAtPointGroupBox.ResumeLayout(false);
            this.derivativeAtPointGroupBox.PerformLayout();
            this.stepsGroupBox.ResumeLayout(false);
            this.stepsGroupBox.PerformLayout();
            this.intervalGroupBox.ResumeLayout(false);
            this.intervalGroupBox.PerformLayout();
            this.maxErrorGroupBox.ResumeLayout(false);
            this.maxErrorGroupBox.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.label10.ResumeLayout(false);
            this.label10.PerformLayout();
            this.label12.ResumeLayout(false);
            this.label12.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalNumericUpDown)).EndInit();
            this.label13.ResumeLayout(false);
            this.label13.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalNumericUpDown)).EndInit();
            this.label19.ResumeLayout(false);
            this.label19.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nStepsNumericUpDown)).EndInit();
            this.label14.ResumeLayout(false);
            this.label14.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).EndInit();
            this.label15.ResumeLayout(false);
            this.label15.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox resultNumericalCalculationsTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox maxErrorGroupBox;
        private System.Windows.Forms.TextBox epsTextBox;
        private Field label10;
        private System.Windows.Forms.GroupBox intervalGroupBox;
        private System.Windows.Forms.NumericUpDown bIntervalNumericUpDown;
        private System.Windows.Forms.NumericUpDown aIntervalNumericUpDown;
        private Field label12;
        private Field label13;
        private System.Windows.Forms.GroupBox stepsGroupBox;
        private System.Windows.Forms.NumericUpDown nStepsNumericUpDown;
        private Field label19;
        private System.Windows.Forms.GroupBox derivativeAtPointGroupBox;
        private System.Windows.Forms.NumericUpDown nOrderDerivativeNumericUpDown;
        private System.Windows.Forms.NumericUpDown xDerivativePointNumericUpDown;
        private Field label14;
        private Field label15;
        private System.Windows.Forms.Button numericalOperationButton;
        private System.Windows.Forms.ComboBox methodNumericalCalculationsComboBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox operationNumericalCalculationsComboBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.DataGridView numericalCalculationsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn function;
        private System.Windows.Forms.DataGridViewTextBoxColumn operation;
        private System.Windows.Forms.DataGridViewTextBoxColumn method;
        private System.Windows.Forms.DataGridViewTextBoxColumn parameters;
        private System.Windows.Forms.DataGridViewTextBoxColumn result;
        private System.Windows.Forms.Label label8;
    }
}
