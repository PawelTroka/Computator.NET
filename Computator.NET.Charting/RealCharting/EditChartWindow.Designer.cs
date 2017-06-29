namespace Computator.NET.Charting.RealCharting
{
    public partial class EditChartWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditChartWindow));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.xMinChartTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.xMaxChartTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.yMaxChartTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.yMinChartTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.yLabelChartTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.xLabelChartTextBox = new System.Windows.Forms.TextBox();
            this.tittleOfChartTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.pointsSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lineThicknessNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.xDeltaChartTextBox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.yDeltaChartTextBox = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.legendaEtykieta2 = new System.Windows.Forms.TextBox();
            this.legendaEtykieta1 = new System.Windows.Forms.TextBox();
            this.legendVisible = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.legendaEtykieta4 = new System.Windows.Forms.TextBox();
            this.legendaEtykieta3 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.titleFontDialog = new System.Windows.Forms.FontDialog();
            this.fontsGroupBox = new System.Windows.Forms.GroupBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.axisLabelsFontDialog = new System.Windows.Forms.FontDialog();
            this.legendFontDialog = new System.Windows.Forms.FontDialog();
            this.valuesFontDialog = new System.Windows.Forms.FontDialog();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pointsSizeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lineThicknessNumericUpDown)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.fontsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(16, 436);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(300, 73);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(324, 436);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(299, 73);
            this.button2.TabIndex = 1;
            this.button2.Text = Localization.Charting.EditChartWindow.button2_Text;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // xMinChartTextBox
            // 
            this.xMinChartTextBox.Location = new System.Drawing.Point(25, 21);
            this.xMinChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.xMinChartTextBox.Name = "xMinChartTextBox";
            this.xMinChartTextBox.Size = new System.Drawing.Size(117, 22);
            this.xMinChartTextBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = Localization.Charting.EditChartWindow.label1_Text;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(153, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = Localization.Charting.EditChartWindow.label2_Text;
            // 
            // xMaxChartTextBox
            // 
            this.xMaxChartTextBox.Location = new System.Drawing.Point(148, 21);
            this.xMaxChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.xMaxChartTextBox.Name = "xMaxChartTextBox";
            this.xMaxChartTextBox.Size = new System.Drawing.Size(117, 22);
            this.xMaxChartTextBox.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(153, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = Localization.Charting.EditChartWindow.label3_Text;
            // 
            // yMaxChartTextBox
            // 
            this.yMaxChartTextBox.Location = new System.Drawing.Point(148, 91);
            this.yMaxChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.yMaxChartTextBox.Name = "yMaxChartTextBox";
            this.yMaxChartTextBox.Size = new System.Drawing.Size(117, 22);
            this.yMaxChartTextBox.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = Localization.Charting.EditChartWindow.label4_Text;
            // 
            // yMinChartTextBox
            // 
            this.yMinChartTextBox.Location = new System.Drawing.Point(25, 91);
            this.yMinChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.yMinChartTextBox.Name = "yMinChartTextBox";
            this.yMinChartTextBox.Size = new System.Drawing.Size(117, 22);
            this.yMinChartTextBox.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(163, 98);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 17);
            this.label5.TabIndex = 13;
            this.label5.Text = Localization.Charting.EditChartWindow.label5_Text;
            // 
            // yLabelChartTextBox
            // 
            this.yLabelChartTextBox.Location = new System.Drawing.Point(157, 73);
            this.yLabelChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.yLabelChartTextBox.Name = "yLabelChartTextBox";
            this.yLabelChartTextBox.Size = new System.Drawing.Size(117, 22);
            this.yLabelChartTextBox.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(39, 98);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 17);
            this.label6.TabIndex = 11;
            this.label6.Text = Localization.Charting.EditChartWindow.label6_Text;
            // 
            // xLabelChartTextBox
            // 
            this.xLabelChartTextBox.Location = new System.Drawing.Point(35, 73);
            this.xLabelChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.xLabelChartTextBox.Name = "xLabelChartTextBox";
            this.xLabelChartTextBox.Size = new System.Drawing.Size(117, 22);
            this.xLabelChartTextBox.TabIndex = 10;
            // 
            // tittleOfChartTextBox
            // 
            this.tittleOfChartTextBox.Location = new System.Drawing.Point(35, 21);
            this.tittleOfChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tittleOfChartTextBox.Name = "tittleOfChartTextBox";
            this.tittleOfChartTextBox.Size = new System.Drawing.Size(232, 22);
            this.tittleOfChartTextBox.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(96, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 17);
            this.label7.TabIndex = 15;
            this.label7.Text = Localization.Charting.EditChartWindow.label7_Text;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.pointsSizeNumericUpDown);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.lineThicknessNumericUpDown);
            this.groupBox1.Controls.Add(this.xDeltaChartTextBox);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.yDeltaChartTextBox);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.xMaxChartTextBox);
            this.groupBox1.Controls.Add(this.xMinChartTextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.yMinChartTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.yMaxChartTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(16, 154);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(300, 265);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = Localization.Charting.EditChartWindow.groupBox1_Text;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label16.Location = new System.Drawing.Point(228, 212);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(22, 17);
            this.label16.TabIndex = 19;
            this.label16.Text = "px";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label17.Location = new System.Drawing.Point(145, 235);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(75, 17);
            this.label17.TabIndex = 18;
            this.label17.Text = Localization.Charting.EditChartWindow.label17_Text;
            // 
            // pointsSizeNumericUpDown
            // 
            this.pointsSizeNumericUpDown.Location = new System.Drawing.Point(148, 210);
            this.pointsSizeNumericUpDown.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pointsSizeNumericUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.pointsSizeNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.pointsSizeNumericUpDown.Name = "pointsSizeNumericUpDown";
            this.pointsSizeNumericUpDown.Size = new System.Drawing.Size(75, 22);
            this.pointsSizeNumericUpDown.TabIndex = 17;
            this.pointsSizeNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.pointsSizeNumericUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label15.Location = new System.Drawing.Point(105, 212);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(22, 17);
            this.label15.TabIndex = 16;
            this.label15.Text = "px";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(21, 235);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(98, 17);
            this.label14.TabIndex = 15;
            this.label14.Text = Localization.Charting.EditChartWindow.label14_Text;
            // 
            // lineThicknessNumericUpDown
            // 
            this.lineThicknessNumericUpDown.Location = new System.Drawing.Point(25, 210);
            this.lineThicknessNumericUpDown.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lineThicknessNumericUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.lineThicknessNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.lineThicknessNumericUpDown.Name = "lineThicknessNumericUpDown";
            this.lineThicknessNumericUpDown.Size = new System.Drawing.Size(75, 22);
            this.lineThicknessNumericUpDown.TabIndex = 14;
            this.lineThicknessNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.lineThicknessNumericUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // xDeltaChartTextBox
            // 
            this.xDeltaChartTextBox.Location = new System.Drawing.Point(25, 151);
            this.xDeltaChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.xDeltaChartTextBox.Name = "xDeltaChartTextBox";
            this.xDeltaChartTextBox.Size = new System.Drawing.Size(117, 22);
            this.xDeltaChartTextBox.TabIndex = 10;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(29, 176);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(45, 17);
            this.label12.TabIndex = 11;
            this.label12.Text = Localization.Charting.EditChartWindow.label12_Text;
            // 
            // yDeltaChartTextBox
            // 
            this.yDeltaChartTextBox.Location = new System.Drawing.Point(148, 151);
            this.yDeltaChartTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.yDeltaChartTextBox.Name = "yDeltaChartTextBox";
            this.yDeltaChartTextBox.Size = new System.Drawing.Size(117, 22);
            this.yDeltaChartTextBox.TabIndex = 12;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(153, 176);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(46, 17);
            this.label13.TabIndex = 13;
            this.label13.Text = Localization.Charting.EditChartWindow.label13_Text;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.tittleOfChartTextBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.yLabelChartTextBox);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.xLabelChartTextBox);
            this.groupBox2.Location = new System.Drawing.Point(16, 21);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Size = new System.Drawing.Size(300, 127);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = Localization.Charting.EditChartWindow.groupBox2_Text;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.legendaEtykieta2);
            this.groupBox3.Controls.Add(this.legendaEtykieta1);
            this.groupBox3.Controls.Add(this.legendVisible);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.legendaEtykieta4);
            this.groupBox3.Controls.Add(this.legendaEtykieta3);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Location = new System.Drawing.Point(324, 210);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Size = new System.Drawing.Size(299, 209);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = Localization.Charting.EditChartWindow.groupBox3_Text;
            // 
            // legendaEtykieta2
            // 
            this.legendaEtykieta2.Location = new System.Drawing.Point(141, 73);
            this.legendaEtykieta2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.legendaEtykieta2.Name = "legendaEtykieta2";
            this.legendaEtykieta2.Size = new System.Drawing.Size(117, 22);
            this.legendaEtykieta2.TabIndex = 12;
            // 
            // legendaEtykieta1
            // 
            this.legendaEtykieta1.Location = new System.Drawing.Point(19, 73);
            this.legendaEtykieta1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.legendaEtykieta1.Name = "legendaEtykieta1";
            this.legendaEtykieta1.Size = new System.Drawing.Size(117, 22);
            this.legendaEtykieta1.TabIndex = 10;
            // 
            // legendVisible
            // 
            this.legendVisible.AutoSize = true;
            this.legendVisible.Location = new System.Drawing.Point(19, 33);
            this.legendVisible.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.legendVisible.Name = "legendVisible";
            this.legendVisible.Size = new System.Drawing.Size(119, 21);
            this.legendVisible.TabIndex = 0;
            this.legendVisible.Text = Localization.Charting.EditChartWindow.legendVisible_Text;
            this.legendVisible.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(23, 98);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 17);
            this.label8.TabIndex = 11;
            this.label8.Text = Localization.Charting.EditChartWindow.label8_Text;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(147, 167);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(50, 17);
            this.label11.TabIndex = 17;
            this.label11.Text = Localization.Charting.EditChartWindow.label11_Text;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(147, 98);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(50, 17);
            this.label9.TabIndex = 13;
            this.label9.Text = Localization.Charting.EditChartWindow.label9_Text;
            // 
            // legendaEtykieta4
            // 
            this.legendaEtykieta4.Location = new System.Drawing.Point(141, 143);
            this.legendaEtykieta4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.legendaEtykieta4.Name = "legendaEtykieta4";
            this.legendaEtykieta4.Size = new System.Drawing.Size(117, 22);
            this.legendaEtykieta4.TabIndex = 16;
            // 
            // legendaEtykieta3
            // 
            this.legendaEtykieta3.Location = new System.Drawing.Point(19, 143);
            this.legendaEtykieta3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.legendaEtykieta3.Name = "legendaEtykieta3";
            this.legendaEtykieta3.Size = new System.Drawing.Size(117, 22);
            this.legendaEtykieta3.TabIndex = 14;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(23, 167);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(50, 17);
            this.label10.TabIndex = 15;
            this.label10.Text = Localization.Charting.EditChartWindow.label10_Text;
            // 
            // fontsGroupBox
            // 
            this.fontsGroupBox.Controls.Add(this.button6);
            this.fontsGroupBox.Controls.Add(this.button5);
            this.fontsGroupBox.Controls.Add(this.button4);
            this.fontsGroupBox.Controls.Add(this.button3);
            this.fontsGroupBox.Location = new System.Drawing.Point(324, 21);
            this.fontsGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fontsGroupBox.Name = "fontsGroupBox";
            this.fontsGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fontsGroupBox.Size = new System.Drawing.Size(299, 183);
            this.fontsGroupBox.TabIndex = 19;
            this.fontsGroupBox.TabStop = false;
            this.fontsGroupBox.Text = Localization.Charting.EditChartWindow.fontsGroupBox_Text;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(5, 142);
            this.button6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(285, 34);
            this.button6.TabIndex = 3;
            this.button6.Text = Localization.Charting.EditChartWindow.button6_Text;
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(5, 101);
            this.button5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(285, 34);
            this.button5.TabIndex = 2;
            this.button5.Text = Localization.Charting.EditChartWindow.button5_Text;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(5, 62);
            this.button4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(285, 34);
            this.button4.TabIndex = 1;
            this.button4.Text = Localization.Charting.EditChartWindow.button4_Text;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(5, 21);
            this.button3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(285, 34);
            this.button3.TabIndex = 0;
            this.button3.Text = Localization.Charting.EditChartWindow.button3_Text;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // EditChartWindow
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(635, 521);
            this.Controls.Add(this.fontsGroupBox);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditChartWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = Localization.Charting.EditChartWindow.this_Text;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pointsSizeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lineThicknessNumericUpDown)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.fontsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox xMinChartTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox xMaxChartTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox yMaxChartTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox yMinChartTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox yLabelChartTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox xLabelChartTextBox;
        private System.Windows.Forms.TextBox tittleOfChartTextBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox legendaEtykieta2;
        private System.Windows.Forms.TextBox legendaEtykieta1;
        private System.Windows.Forms.CheckBox legendVisible;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox legendaEtykieta4;
        private System.Windows.Forms.TextBox legendaEtykieta3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.FontDialog titleFontDialog;
        private System.Windows.Forms.GroupBox fontsGroupBox;
        private System.Windows.Forms.FontDialog axisLabelsFontDialog;
        private System.Windows.Forms.FontDialog legendFontDialog;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox xDeltaChartTextBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox yDeltaChartTextBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.FontDialog valuesFontDialog;
        private System.Windows.Forms.NumericUpDown lineThicknessNumericUpDown;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.NumericUpDown pointsSizeNumericUpDown;
    }
}