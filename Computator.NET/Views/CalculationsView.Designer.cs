using System.Windows.Forms;
using Computator.NET.Controls;

namespace Computator.NET.Views
{
    partial class CalculationsView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CalculationsView));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.valueForCalculationNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.calculationsImZnumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.calculationValueTextBox = new System.Windows.Forms.TextBox();
            this.calculateButton = new System.Windows.Forms.Button();
            this.calculationsHistoryDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.calculationsRealLabel = new System.Windows.Forms.Label();
            this.calculationsComplexLabel = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueForCalculationNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsImZnumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsHistoryDataGridView)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Controls.Add(this.tableLayoutPanel2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // valueForCalculationNumericUpDown
            // 
            resources.ApplyResources(this.valueForCalculationNumericUpDown, "valueForCalculationNumericUpDown");
            this.valueForCalculationNumericUpDown.DecimalPlaces = 5;
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
            // calculationsImZnumericUpDown
            // 
            resources.ApplyResources(this.calculationsImZnumericUpDown, "calculationsImZnumericUpDown");
            this.calculationsImZnumericUpDown.DecimalPlaces = 5;
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
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // calculationValueTextBox
            // 
            this.calculationValueTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel1.SetColumnSpan(this.calculationValueTextBox, 2);
            resources.ApplyResources(this.calculationValueTextBox, "calculationValueTextBox");
            this.calculationValueTextBox.Name = "calculationValueTextBox";
            this.calculationValueTextBox.ReadOnly = true;
            // 
            // calculateButton
            // 
            resources.ApplyResources(this.calculateButton, "calculateButton");
            this.calculateButton.Name = "calculateButton";
            this.calculateButton.UseVisualStyleBackColor = true;
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
            this.tableLayoutPanel1.SetColumnSpan(this.calculationsHistoryDataGridView, 4);
            resources.ApplyResources(this.calculationsHistoryDataGridView, "calculationsHistoryDataGridView");
            this.calculationsHistoryDataGridView.Name = "calculationsHistoryDataGridView";
            this.calculationsHistoryDataGridView.ReadOnly = true;
            this.calculationsHistoryDataGridView.RowTemplate.Height = 24;
            this.calculationsHistoryDataGridView.RowTemplate.ReadOnly = true;
            this.calculationsHistoryDataGridView.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
            this.calculationsHistoryDataGridView.AutoSizeRowsMode=DataGridViewAutoSizeRowsMode.AllCells;
            this.calculationsHistoryDataGridView.AutoSize = true;
            
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
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.label7, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.calculationsHistoryDataGridView, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.calculationValueTextBox, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.calculateButton, 1, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // tableLayoutPanel2
            // 
            resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
            this.tableLayoutPanel2.Controls.Add(this.calculationsRealLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.valueForCalculationNumericUpDown, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.calculationsImZnumericUpDown, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.calculationsComplexLabel, 0, 1);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            // 
            // calculationsRealLabel
            // 
            resources.ApplyResources(this.calculationsRealLabel, "calculationsRealLabel");
            this.calculationsRealLabel.Name = "calculationsRealLabel";
            // 
            // calculationsComplexLabel
            // 
            resources.ApplyResources(this.calculationsComplexLabel, "calculationsComplexLabel");
            this.calculationsComplexLabel.Name = "calculationsComplexLabel";
            // 
            // CalculationsView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.tableLayoutPanel1);
            resources.ApplyResources(this, "$this");
            this.Name = "CalculationsView";
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueForCalculationNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsImZnumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsHistoryDataGridView)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown calculationsImZnumericUpDown;
        private System.Windows.Forms.Label calculationsRealLabel;
        private System.Windows.Forms.NumericUpDown valueForCalculationNumericUpDown;
        private System.Windows.Forms.Label calculationsComplexLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox calculationValueTextBox;
        private System.Windows.Forms.Button calculateButton;
        private System.Windows.Forms.DataGridView calculationsHistoryDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}
