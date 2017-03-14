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
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.nOrderDerivativeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.xDerivativePointNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label14 = new System.Windows.Forms.Label();
            this.stepsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.nStepsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label19 = new System.Windows.Forms.Label();
            this.intervalGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.aIntervalNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.bIntervalNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label13 = new System.Windows.Forms.Label();
            this.maxErrorGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.epsTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).BeginInit();
            this.derivativeAtPointGroupBox.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).BeginInit();
            this.stepsGroupBox.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nStepsNumericUpDown)).BeginInit();
            this.intervalGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalNumericUpDown)).BeginInit();
            this.maxErrorGroupBox.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
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
            this.numericalCalculationsDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.numericalCalculationsDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.numericalCalculationsDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
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
            resources.ApplyResources(this.operationNumericalCalculationsComboBox, "operationNumericalCalculationsComboBox");
            this.operationNumericalCalculationsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            resources.ApplyResources(this.methodNumericalCalculationsComboBox, "methodNumericalCalculationsComboBox");
            this.methodNumericalCalculationsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            this.derivativeAtPointGroupBox.Controls.Add(this.tableLayoutPanel3);
            this.derivativeAtPointGroupBox.Name = "derivativeAtPointGroupBox";
            this.derivativeAtPointGroupBox.TabStop = false;
            // 
            // tableLayoutPanel3
            // 
            resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
            this.tableLayoutPanel3.Controls.Add(this.nOrderDerivativeNumericUpDown, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label15, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.xDerivativePointNumericUpDown, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.label14, 0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
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
            this.label15.Name = "label15";
            // 
            // xDerivativePointNumericUpDown
            // 
            resources.ApplyResources(this.xDerivativePointNumericUpDown, "xDerivativePointNumericUpDown");
            this.xDerivativePointNumericUpDown.DecimalPlaces = 9;
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
            // stepsGroupBox
            // 
            resources.ApplyResources(this.stepsGroupBox, "stepsGroupBox");
            this.stepsGroupBox.Controls.Add(this.tableLayoutPanel4);
            this.stepsGroupBox.Name = "stepsGroupBox";
            this.stepsGroupBox.TabStop = false;
            // 
            // tableLayoutPanel4
            // 
            resources.ApplyResources(this.tableLayoutPanel4, "tableLayoutPanel4");
            this.tableLayoutPanel4.Controls.Add(this.nStepsNumericUpDown, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label19, 0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
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
            // intervalGroupBox
            // 
            resources.ApplyResources(this.intervalGroupBox, "intervalGroupBox");
            this.intervalGroupBox.Controls.Add(this.tableLayoutPanel2);
            this.intervalGroupBox.Name = "intervalGroupBox";
            this.intervalGroupBox.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.aIntervalNumericUpDown, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label12, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.bIntervalNumericUpDown, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label13, 0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // aIntervalNumericUpDown
            // 
            resources.ApplyResources(this.aIntervalNumericUpDown, "aIntervalNumericUpDown");
            this.aIntervalNumericUpDown.DecimalPlaces = 9;
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
            // bIntervalNumericUpDown
            // 
            resources.ApplyResources(this.bIntervalNumericUpDown, "bIntervalNumericUpDown");
            this.bIntervalNumericUpDown.DecimalPlaces = 9;
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
            this.label13.Name = "label13";
            // 
            // maxErrorGroupBox
            // 
            resources.ApplyResources(this.maxErrorGroupBox, "maxErrorGroupBox");
            this.maxErrorGroupBox.Controls.Add(this.tableLayoutPanel5);
            this.maxErrorGroupBox.Name = "maxErrorGroupBox";
            this.maxErrorGroupBox.TabStop = false;
            // 
            // tableLayoutPanel5
            // 
            resources.ApplyResources(this.tableLayoutPanel5, "tableLayoutPanel5");
            this.tableLayoutPanel5.Controls.Add(this.epsTextBox, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.label10, 0, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
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
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.numericalOperationButton, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.methodNumericalCalculationsComboBox, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label6, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.operationNumericalCalculationsComboBox, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label9, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.numericalCalculationsDataGridView, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.label8, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.resultNumericalCalculationsTextBox, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 2, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // tableLayoutPanel6
            // 
            resources.ApplyResources(this.tableLayoutPanel6, "tableLayoutPanel6");
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel6, 2);
            this.tableLayoutPanel6.Controls.Add(this.maxErrorGroupBox, 1, 1);
            this.tableLayoutPanel6.Controls.Add(this.stepsGroupBox, 0, 1);
            this.tableLayoutPanel6.Controls.Add(this.derivativeAtPointGroupBox, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.intervalGroupBox, 0, 0);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel1.SetRowSpan(this.tableLayoutPanel6, 2);
            // 
            // NumericalCalculationsView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "NumericalCalculationsView";
            ((System.ComponentModel.ISupportInitialize)(this.numericalCalculationsDataGridView)).EndInit();
            this.derivativeAtPointGroupBox.ResumeLayout(false);
            this.derivativeAtPointGroupBox.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nOrderDerivativeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xDerivativePointNumericUpDown)).EndInit();
            this.stepsGroupBox.ResumeLayout(false);
            this.stepsGroupBox.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nStepsNumericUpDown)).EndInit();
            this.intervalGroupBox.ResumeLayout(false);
            this.intervalGroupBox.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.aIntervalNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bIntervalNumericUpDown)).EndInit();
            this.maxErrorGroupBox.ResumeLayout(false);
            this.maxErrorGroupBox.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox resultNumericalCalculationsTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox maxErrorGroupBox;
        private System.Windows.Forms.TextBox epsTextBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox intervalGroupBox;
        private System.Windows.Forms.NumericUpDown bIntervalNumericUpDown;
        private System.Windows.Forms.NumericUpDown aIntervalNumericUpDown;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox stepsGroupBox;
        private System.Windows.Forms.NumericUpDown nStepsNumericUpDown;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.GroupBox derivativeAtPointGroupBox;
        private System.Windows.Forms.NumericUpDown nOrderDerivativeNumericUpDown;
        private System.Windows.Forms.NumericUpDown xDerivativePointNumericUpDown;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
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
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel6;
    }
}
