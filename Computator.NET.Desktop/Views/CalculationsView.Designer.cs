using Computator.NET.DataTypes.Text;
using Computator.NET.Desktop.Controls;

namespace Computator.NET.Desktop.Views
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.calculationsRealLabel = new System.Windows.Forms.Label();
            this.valueForCalculationNumericUpDown = new ScientificNumericUpDown();
            this.calculationsImZnumericUpDown = new ScientificNumericUpDown();
            this.calculationsComplexLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.calculationValueTextBox = new System.Windows.Forms.TextBox();
            this.calculateButton = new System.Windows.Forms.Button();
            this.calculationsHistoryDataGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueForCalculationNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsImZnumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsHistoryDataGridView)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.AutoSize = true;
            this.groupBox3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox3.Controls.Add(this.tableLayoutPanel2);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.groupBox3.Location = new System.Drawing.Point(288, 22);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2, 2, 5, 5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2, 2, 5, 5);
            this.groupBox3.Size = new System.Drawing.Size(151, 100);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.calculationsRealLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.valueForCalculationNumericUpDown, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.calculationsImZnumericUpDown, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.calculationsComplexLabel, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(2, 25);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(144, 70);
            this.tableLayoutPanel2.TabIndex = 9;
            // 
            // calculationsRealLabel
            // 
            this.calculationsRealLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.calculationsRealLabel.AutoSize = true;
            this.calculationsRealLabel.Font = CustomFonts.GetMathFont(12);
            this.calculationsRealLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.calculationsRealLabel.Location = new System.Drawing.Point(12, 6);
            this.calculationsRealLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.calculationsRealLabel.Name = "calculationsRealLabel";
            this.calculationsRealLabel.Size = new System.Drawing.Size(63, 23);
            this.calculationsRealLabel.TabIndex = 5;
            this.calculationsRealLabel.Text = "       x =";
            // 
            // valueForCalculationNumericUpDown
            // 
            this.valueForCalculationNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.valueForCalculationNumericUpDown.AutoSize = true;
            this.valueForCalculationNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.valueForCalculationNumericUpDown.Font = CustomFonts.GetMathFont(12);
            this.valueForCalculationNumericUpDown.Location = new System.Drawing.Point(79, 2);
            this.valueForCalculationNumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.valueForCalculationNumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.valueForCalculationNumericUpDown.Name = "valueForCalculationNumericUpDown";
            this.valueForCalculationNumericUpDown.Size = new System.Drawing.Size(63, 31);
            this.valueForCalculationNumericUpDown.TabIndex = 6;
            this.valueForCalculationNumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Adaptive;
            this.valueForCalculationNumericUpDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // calculationsImZnumericUpDown
            // 
            this.calculationsImZnumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.calculationsImZnumericUpDown.AutoSize = true;
            this.calculationsImZnumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.calculationsImZnumericUpDown.Font = CustomFonts.GetMathFont(12);
            this.calculationsImZnumericUpDown.Location = new System.Drawing.Point(79, 37);
            this.calculationsImZnumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.calculationsImZnumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.calculationsImZnumericUpDown.Name = "calculationsImZnumericUpDown";
            this.calculationsImZnumericUpDown.Size = new System.Drawing.Size(63, 31);
            this.calculationsImZnumericUpDown.TabIndex = 9;
            this.calculationsImZnumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Adaptive;
            this.calculationsImZnumericUpDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.calculationsImZnumericUpDown.Visible = false;
            // 
            // calculationsComplexLabel
            // 
            this.calculationsComplexLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.calculationsComplexLabel.AutoSize = true;
            this.calculationsComplexLabel.Font = CustomFonts.GetMathFont(12);
            this.calculationsComplexLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.calculationsComplexLabel.Location = new System.Drawing.Point(2, 41);
            this.calculationsComplexLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.calculationsComplexLabel.Name = "calculationsComplexLabel";
            this.calculationsComplexLabel.Size = new System.Drawing.Size(73, 23);
            this.calculationsComplexLabel.TabIndex = 8;
            this.calculationsComplexLabel.Text = "Im(z) =";
            this.calculationsComplexLabel.Visible = false;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(182, 20);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(102, 107);
            this.label5.TabIndex = 10;
            this.label5.Text = Localization.Views.CalculationsView.label5_Text;
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // calculationValueTextBox
            // 
            this.calculationValueTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel1.SetColumnSpan(this.calculationValueTextBox, 2);
            this.calculationValueTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.calculationValueTextBox.Font = CustomFonts.GetMathFont(16.2F);
            this.calculationValueTextBox.Location = new System.Drawing.Point(184, 131);
            this.calculationValueTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 8, 4);
            this.calculationValueTextBox.Name = "calculationValueTextBox";
            this.calculationValueTextBox.ReadOnly = true;
            this.calculationValueTextBox.Size = new System.Drawing.Size(402, 39);
            this.calculationValueTextBox.TabIndex = 3;
            // 
            // calculateButton
            // 
            this.calculateButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.calculateButton.AutoSize = true;
            this.calculateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.calculateButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.calculateButton.Location = new System.Drawing.Point(32, 53);
            this.calculateButton.Margin = new System.Windows.Forms.Padding(2);
            this.calculateButton.Name = "calculateButton";
            this.calculateButton.Size = new System.Drawing.Size(146, 41);
            this.calculateButton.TabIndex = 2;
            this.calculateButton.Text = Localization.Views.CalculationsView.calculateButton_Text;
            this.calculateButton.UseVisualStyleBackColor = true;
            // 
            // calculationsHistoryDataGridView
            // 
            this.calculationsHistoryDataGridView.AllowUserToAddRows = false;
            this.calculationsHistoryDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.calculationsHistoryDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.calculationsHistoryDataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.calculationsHistoryDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.calculationsHistoryDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn4,
            this.dataGridViewTextBoxColumn5});
            this.tableLayoutPanel1.SetColumnSpan(this.calculationsHistoryDataGridView, 4);
            this.calculationsHistoryDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.calculationsHistoryDataGridView.Location = new System.Drawing.Point(2, 176);
            this.calculationsHistoryDataGridView.Margin = new System.Windows.Forms.Padding(2);
            this.calculationsHistoryDataGridView.Name = "calculationsHistoryDataGridView";
            this.calculationsHistoryDataGridView.ReadOnly = true;
            this.calculationsHistoryDataGridView.RowTemplate.Height = 24;
            this.calculationsHistoryDataGridView.RowTemplate.ReadOnly = true;
            this.calculationsHistoryDataGridView.Size = new System.Drawing.Size(590, 219);
            this.calculationsHistoryDataGridView.TabIndex = 7;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Font = CustomFonts.GetMathFont(10.2F);
            this.dataGridViewTextBoxColumn1.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewTextBoxColumn1.HeaderText = Localization.Views.CalculationsView.dataGridViewTextBoxColumn1_HeaderText;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Font = CustomFonts.GetMathFont(10.2F);
            this.dataGridViewTextBoxColumn4.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewTextBoxColumn4.HeaderText = Localization.Views.CalculationsView.dataGridViewTextBoxColumn4_HeaderText;
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Font = CustomFonts.GetMathFont(10.2F);
            this.dataGridViewTextBoxColumn5.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewTextBoxColumn5.HeaderText = Localization.Views.CalculationsView
                .dataGridViewTextBoxColumn5_HeaderText;
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.ReadOnly = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Controls.Add(this.label7, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.calculationsHistoryDataGridView, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.calculationValueTextBox, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.calculateButton, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 8);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(556, 397);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Right;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F);
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(74, 129);
            this.label7.Margin = new System.Windows.Forms.Padding(2);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(104, 43);
            this.label7.TabIndex = 4;
            this.label7.Text = Localization.Views.CalculationsView.label7_Text;
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CalculationsView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(8);
            this.Name = "CalculationsView";
            this.Size = new System.Drawing.Size(556, 397);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueForCalculationNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsImZnumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.calculationsHistoryDataGridView)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private ScientificNumericUpDown calculationsImZnumericUpDown;
        private System.Windows.Forms.Label calculationsRealLabel;
        private ScientificNumericUpDown valueForCalculationNumericUpDown;
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
