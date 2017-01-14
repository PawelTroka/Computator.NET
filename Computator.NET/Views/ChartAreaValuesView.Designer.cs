using Computator.NET.Controls;

namespace Computator.NET.Views
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChartAreaValuesView));
            this.label2 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.clearChartButton = new System.Windows.Forms.Button();
            this.addToChartButton = new System.Windows.Forms.Button();
            this.yNlabel = new Computator.NET.Controls.Field();
            this.yNNumericUpDown = new Computator.NET.Controls.ScientificNumericUpDown();
            this.y0label = new Computator.NET.Controls.Field();
            this.y0NumericUpDown = new Computator.NET.Controls.ScientificNumericUpDown();
            this.label4 = new Computator.NET.Controls.Field();
            this.xnNumericUpDown = new Computator.NET.Controls.ScientificNumericUpDown();
            this.label3 = new Computator.NET.Controls.Field();
            this.x0NumericUpDown = new Computator.NET.Controls.ScientificNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.yNlabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yNNumericUpDown)).BeginInit();
            this.y0label.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.y0NumericUpDown)).BeginInit();
            this.label4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xnNumericUpDown)).BeginInit();
            this.label3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.x0NumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // trackBar1
            // 
            resources.ApplyResources(this.trackBar1, "trackBar1");
            this.trackBar1.LargeChange = 20;
            this.trackBar1.Maximum = 200;
            this.trackBar1.Minimum = 10;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.SmallChange = 10;
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar1.Value = 100;
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.yNlabel);
            this.groupBox2.Controls.Add(this.y0label);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // clearChartButton
            // 
            resources.ApplyResources(this.clearChartButton, "clearChartButton");
            this.clearChartButton.Name = "clearChartButton";
            this.clearChartButton.UseVisualStyleBackColor = true;
            // 
            // addToChartButton
            // 
            resources.ApplyResources(this.addToChartButton, "addToChartButton");
            this.addToChartButton.Name = "addToChartButton";
            this.addToChartButton.UseVisualStyleBackColor = true;
            // 
            // yNlabel
            // 
            resources.ApplyResources(this.yNlabel, "yNlabel");
            this.yNlabel.Controls.Add(this.yNNumericUpDown);
            this.yNlabel.Name = "yNlabel";
            // 
            // yNNumericUpDown
            // 
            resources.ApplyResources(this.yNNumericUpDown, "yNNumericUpDown");
            this.yNNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.yNNumericUpDown.Increment = new decimal(new int[] {
            9,
            0,
            0,
            65536});
            this.yNNumericUpDown.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            65536});
            this.yNNumericUpDown.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147418112});
            this.yNNumericUpDown.Name = "yNNumericUpDown";
            this.yNNumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // y0label
            // 
            resources.ApplyResources(this.y0label, "y0label");
            this.y0label.Controls.Add(this.y0NumericUpDown);
            this.y0label.Name = "y0label";
            // 
            // y0NumericUpDown
            // 
            resources.ApplyResources(this.y0NumericUpDown, "y0NumericUpDown");
            this.y0NumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.y0NumericUpDown.Increment = new decimal(new int[] {
            9,
            0,
            0,
            65536});
            this.y0NumericUpDown.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            65536});
            this.y0NumericUpDown.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147418112});
            this.y0NumericUpDown.Name = "y0NumericUpDown";
            this.y0NumericUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            -2147483648});
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Controls.Add(this.xnNumericUpDown);
            this.label4.Name = "label4";
            // 
            // xnNumericUpDown
            // 
            resources.ApplyResources(this.xnNumericUpDown, "xnNumericUpDown");
            this.xnNumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.xnNumericUpDown.Increment = new decimal(new int[] {
            20,
            0,
            0,
            65536});
            this.xnNumericUpDown.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            65536});
            this.xnNumericUpDown.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147418112});
            this.xnNumericUpDown.Name = "xnNumericUpDown";
            this.xnNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Controls.Add(this.x0NumericUpDown);
            this.label3.Name = "label3";
            // 
            // x0NumericUpDown
            // 
            resources.ApplyResources(this.x0NumericUpDown, "x0NumericUpDown");
            this.x0NumericUpDown.Epsilon = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.x0NumericUpDown.Increment = new decimal(new int[] {
            20,
            0,
            0,
            65536});
            this.x0NumericUpDown.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            65536});
            this.x0NumericUpDown.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147418112});
            this.x0NumericUpDown.Name = "x0NumericUpDown";
            this.x0NumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            -2147483648});
            // 
            // ChartAreaValuesView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.clearChartButton);
            this.Controls.Add(this.addToChartButton);
            resources.ApplyResources(this, "$this");
            this.Name = "ChartAreaValuesView";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.yNlabel.ResumeLayout(false);
            this.yNlabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yNNumericUpDown)).EndInit();
            this.y0label.ResumeLayout(false);
            this.y0label.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.y0NumericUpDown)).EndInit();
            this.label4.ResumeLayout(false);
            this.label4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xnNumericUpDown)).EndInit();
            this.label3.ResumeLayout(false);
            this.label3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.x0NumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.GroupBox groupBox2;
        private ScientificNumericUpDown yNNumericUpDown;
        private ScientificNumericUpDown y0NumericUpDown;
        private Field yNlabel;
        private Field y0label;
        private ScientificNumericUpDown xnNumericUpDown;
        private ScientificNumericUpDown x0NumericUpDown;
        private Field label4;
        private Field label3;
        private System.Windows.Forms.Button clearChartButton;
        private System.Windows.Forms.Button addToChartButton;
    }
}
