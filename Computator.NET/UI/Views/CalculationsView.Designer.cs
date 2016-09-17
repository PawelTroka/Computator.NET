namespace Computator.NET.UI.Views
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsImZnumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueForCalculationNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsHistoryDataGridView)).BeginInit();
            this.SuspendLayout();
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
            // CalculationsView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer4);
            this.Name = "CalculationsView";
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown calculationsImZnumericUpDown;
        private System.Windows.Forms.Label calculationsRealLabel;
        private System.Windows.Forms.NumericUpDown valueForCalculationNumericUpDown;
        private System.Windows.Forms.Label calculationsComplexLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox calculationValueTextBox;
        private System.Windows.Forms.Button calculateButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridView calculationsHistoryDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
    }
}
