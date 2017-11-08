using Computator.NET.Desktop.Controls;
using Computator.NET.DataTypes.Text;

namespace Computator.NET.Desktop.Views
{
    partial class ChartAreaValuesView
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
            this.label2 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.yNNumericUpDown = new ScientificNumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.x0NumericUpDown = new ScientificNumericUpDown();
            this.y0label = new System.Windows.Forms.Label();
            this.y0NumericUpDown = new ScientificNumericUpDown();
            this.yNlabel = new System.Windows.Forms.Label();
            this.xnNumericUpDown = new ScientificNumericUpDown();
            this.clearChartButton = new System.Windows.Forms.Button();
            this.addToChartButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yNNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.x0NumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.y0NumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xnNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(0, 308);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 10, 2, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 20);
            this.label2.TabIndex = 18;
            this.label2.Text = Localization.Views.ChartAreaValuesView.label2_Text;
            // 
            // trackBar1
            // 
            this.trackBar1.Dock = System.Windows.Forms.DockStyle.Top;
            this.trackBar1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.trackBar1.LargeChange = 20;
            this.trackBar1.Location = new System.Drawing.Point(0, 328);
            this.trackBar1.Margin = new System.Windows.Forms.Padding(2, 6, 2, 2);
            this.trackBar1.Maximum = 200;
            this.trackBar1.Minimum = 10;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(202, 56);
            this.trackBar1.SmallChange = 10;
            this.trackBar1.TabIndex = 24;
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar1.Value = 100;
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.tableLayoutPanel1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.8F);
            this.groupBox2.Location = new System.Drawing.Point(0, 141);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(202, 167);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = Localization.Views.ChartAreaValuesView.groupBox2_Text;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.Controls.Add(this.label4, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.yNNumericUpDown, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.label3, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.x0NumericUpDown, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.y0label, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.y0NumericUpDown, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.yNlabel, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.xnNumericUpDown, 2, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 23);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(198, 142);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Font = CustomFonts.GetMathFont(10.2F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(9, 42);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "xN =";
            // 
            // yNNumericUpDown
            // 
            this.yNNumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.yNNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.yNNumericUpDown.Font = CustomFonts.GetMathFont(9.75F);
            this.yNNumericUpDown.Increment = new decimal(new int[] {
            9,
            0,
            0,
            65536});
            this.yNNumericUpDown.Location = new System.Drawing.Point(55, 107);
            this.yNNumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.yNNumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.yNNumericUpDown.Name = "yNNumericUpDown";
            this.yNNumericUpDown.Size = new System.Drawing.Size(120, 27);
            this.yNNumericUpDown.TabIndex = 17;
            this.yNNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.yNNumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Adaptive;
            this.yNNumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Font = CustomFonts.GetMathFont(10.2F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(12, 11);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "x0 =";
            // 
            // x0NumericUpDown
            // 
            this.x0NumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.x0NumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.x0NumericUpDown.Font = CustomFonts.GetMathFont(9.75F);
            this.x0NumericUpDown.Increment = new decimal(new int[] {
            20,
            0,
            0,
            65536});
            this.x0NumericUpDown.Location = new System.Drawing.Point(55, 8);
            this.x0NumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.x0NumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.x0NumericUpDown.Name = "x0NumericUpDown";
            this.x0NumericUpDown.Size = new System.Drawing.Size(120, 27);
            this.x0NumericUpDown.TabIndex = 10;
            this.x0NumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.x0NumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Adaptive;
            this.x0NumericUpDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // y0label
            // 
            this.y0label.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.y0label.AutoSize = true;
            this.y0label.Font = CustomFonts.GetMathFont(10.2F);
            this.y0label.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.y0label.Location = new System.Drawing.Point(11, 79);
            this.y0label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.y0label.Name = "y0label";
            this.y0label.Size = new System.Drawing.Size(40, 20);
            this.y0label.TabIndex = 14;
            this.y0label.Text = "y0 =";
            // 
            // y0NumericUpDown
            // 
            this.y0NumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.y0NumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.y0NumericUpDown.Font = CustomFonts.GetMathFont(9.75F);
            this.y0NumericUpDown.Increment = new decimal(new int[] {
            9,
            0,
            0,
            65536});
            this.y0NumericUpDown.Location = new System.Drawing.Point(55, 76);
            this.y0NumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.y0NumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.y0NumericUpDown.Name = "y0NumericUpDown";
            this.y0NumericUpDown.Size = new System.Drawing.Size(120, 27);
            this.y0NumericUpDown.TabIndex = 16;
            this.y0NumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.y0NumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Adaptive;
            this.y0NumericUpDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // yNlabel
            // 
            this.yNlabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.yNlabel.AutoSize = true;
            this.yNlabel.Font = CustomFonts.GetMathFont(10.2F);
            this.yNlabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.yNlabel.Location = new System.Drawing.Point(8, 110);
            this.yNlabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.yNlabel.Name = "yNlabel";
            this.yNlabel.Size = new System.Drawing.Size(43, 20);
            this.yNlabel.TabIndex = 15;
            this.yNlabel.Text = "yN =";
            // 
            // xnNumericUpDown
            // 
            this.xnNumericUpDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xnNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.xnNumericUpDown.Font = CustomFonts.GetMathFont(9.75F);
            this.xnNumericUpDown.Increment = new decimal(new int[] {
            20,
            0,
            0,
            65536});
            this.xnNumericUpDown.Location = new System.Drawing.Point(55, 39);
            this.xnNumericUpDown.Margin = new System.Windows.Forms.Padding(2);
            this.xnNumericUpDown.MultiplyFactor = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.xnNumericUpDown.Name = "xnNumericUpDown";
            this.xnNumericUpDown.Size = new System.Drawing.Size(120, 27);
            this.xnNumericUpDown.TabIndex = 11;
            this.xnNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.xnNumericUpDown.UpDownMode = ScientificNumericUpDown.UpDownBehavior.Adaptive;
            this.xnNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // clearChartButton
            // 
            this.clearChartButton.AutoSize = true;
            this.clearChartButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.clearChartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F);
            this.clearChartButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.clearChartButton.Location = new System.Drawing.Point(0, 98);
            this.clearChartButton.Margin = new System.Windows.Forms.Padding(2);
            this.clearChartButton.Name = "clearChartButton";
            this.clearChartButton.Size = new System.Drawing.Size(202, 43);
            this.clearChartButton.TabIndex = 22;
            this.clearChartButton.Text = Localization.Views.ChartAreaValuesView.clearChartButton_Text;
            this.clearChartButton.UseVisualStyleBackColor = true;
            // 
            // addToChartButton
            // 
            this.addToChartButton.AutoSize = true;
            this.addToChartButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.addToChartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.addToChartButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.addToChartButton.Location = new System.Drawing.Point(0, 0);
            this.addToChartButton.Margin = new System.Windows.Forms.Padding(2);
            this.addToChartButton.Name = "addToChartButton";
            this.addToChartButton.Size = new System.Drawing.Size(202, 98);
            this.addToChartButton.TabIndex = 19;
            this.addToChartButton.Text = Localization.Views.ChartAreaValuesView.addToChartButton_Text;
            this.addToChartButton.UseVisualStyleBackColor = true;
            // 
            // ChartAreaValuesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.clearChartButton);
            this.Controls.Add(this.addToChartButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.2F);
            this.Name = "ChartAreaValuesView";
            this.Size = new System.Drawing.Size(202, 412);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yNNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.x0NumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.y0NumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xnNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackBar1;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
