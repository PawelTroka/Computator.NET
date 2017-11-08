namespace Computator.NET.Desktop.Benchmarking
{
    partial class BenchmarkForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cancelMemoryTestButton = new System.Windows.Forms.Button();
            this.startMemoryTestButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.memoryTestRichTextBox = new System.Windows.Forms.RichTextBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.memoryTestProgressBar = new System.Windows.Forms.ProgressBar();
            this.cancelFunctionsTestButton = new System.Windows.Forms.Button();
            this.richTextBox4 = new System.Windows.Forms.RichTextBox();
            this.startFunctionsTestButton = new System.Windows.Forms.Button();
            this.functionsTestProgressBar = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.functionsTestRichTextBox = new System.Windows.Forms.RichTextBox();
            this.memoryTestBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.functionsTestBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.cancelMemoryTestButton);
            this.splitContainer1.Panel1.Controls.Add(this.startMemoryTestButton);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.memoryTestRichTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.richTextBox1);
            this.splitContainer1.Panel1.Controls.Add(this.memoryTestProgressBar);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cancelFunctionsTestButton);
            this.splitContainer1.Panel2.Controls.Add(this.richTextBox4);
            this.splitContainer1.Panel2.Controls.Add(this.startFunctionsTestButton);
            this.splitContainer1.Panel2.Controls.Add(this.functionsTestProgressBar);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.functionsTestRichTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(431, 362);
            this.splitContainer1.SplitterDistance = 214;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // cancelMemoryTestButton
            // 
            this.cancelMemoryTestButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F);
            this.cancelMemoryTestButton.Location = new System.Drawing.Point(9, 139);
            this.cancelMemoryTestButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelMemoryTestButton.Name = "cancelMemoryTestButton";
            this.cancelMemoryTestButton.Size = new System.Drawing.Size(196, 26);
            this.cancelMemoryTestButton.TabIndex = 5;
            this.cancelMemoryTestButton.Text = Localization.Dialogs.BenchmarkForm.cancelMemoryTestButton_Text;
            this.cancelMemoryTestButton.UseVisualStyleBackColor = true;
            this.cancelMemoryTestButton.Click += new System.EventHandler(this.cancelMemoryTestButton_Click);
            // 
            // startMemoryTestButton
            // 
            this.startMemoryTestButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F);
            this.startMemoryTestButton.Location = new System.Drawing.Point(9, 76);
            this.startMemoryTestButton.Margin = new System.Windows.Forms.Padding(2);
            this.startMemoryTestButton.Name = "startMemoryTestButton";
            this.startMemoryTestButton.Size = new System.Drawing.Size(196, 58);
            this.startMemoryTestButton.TabIndex = 4;
            this.startMemoryTestButton.Text = Localization.Dialogs.BenchmarkForm.startMemoryTestButton_Text;
            this.startMemoryTestButton.UseVisualStyleBackColor = true;
            this.startMemoryTestButton.Click += new System.EventHandler(this.startMemoryTestButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F);
            this.label1.Location = new System.Drawing.Point(9, 244);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = Localization.Dialogs.BenchmarkForm.label1_Text;
            // 
            // memoryTestRichTextBox
            // 
            this.memoryTestRichTextBox.BackColor = System.Drawing.Color.Lavender;
            this.memoryTestRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.memoryTestRichTextBox.Location = new System.Drawing.Point(9, 270);
            this.memoryTestRichTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.memoryTestRichTextBox.Name = "memoryTestRichTextBox";
            this.memoryTestRichTextBox.ReadOnly = true;
            this.memoryTestRichTextBox.Size = new System.Drawing.Size(196, 82);
            this.memoryTestRichTextBox.TabIndex = 2;
            this.memoryTestRichTextBox.Text = "";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F);
            this.richTextBox1.Location = new System.Drawing.Point(9, 10);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(196, 61);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = Localization.Dialogs.BenchmarkForm.richTextBox1_Text;
            // 
            // memoryTestProgressBar
            // 
            this.memoryTestProgressBar.Location = new System.Drawing.Point(9, 170);
            this.memoryTestProgressBar.Margin = new System.Windows.Forms.Padding(2);
            this.memoryTestProgressBar.Name = "memoryTestProgressBar";
            this.memoryTestProgressBar.Size = new System.Drawing.Size(196, 72);
            this.memoryTestProgressBar.Step = 1;
            this.memoryTestProgressBar.TabIndex = 0;
            // 
            // cancelFunctionsTestButton
            // 
            this.cancelFunctionsTestButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F);
            this.cancelFunctionsTestButton.Location = new System.Drawing.Point(10, 139);
            this.cancelFunctionsTestButton.Margin = new System.Windows.Forms.Padding(2);
            this.cancelFunctionsTestButton.Name = "cancelFunctionsTestButton";
            this.cancelFunctionsTestButton.Size = new System.Drawing.Size(196, 26);
            this.cancelFunctionsTestButton.TabIndex = 11;
            this.cancelFunctionsTestButton.Text = Localization.Dialogs.BenchmarkForm.cancelFunctionsTestButton_Text;
            this.cancelFunctionsTestButton.UseVisualStyleBackColor = true;
            this.cancelFunctionsTestButton.Click += new System.EventHandler(this.cancelFunctionsTestButton_Click);
            // 
            // richTextBox4
            // 
            this.richTextBox4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.richTextBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox4.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.richTextBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F);
            this.richTextBox4.Location = new System.Drawing.Point(10, 10);
            this.richTextBox4.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox4.Name = "richTextBox4";
            this.richTextBox4.ReadOnly = true;
            this.richTextBox4.Size = new System.Drawing.Size(196, 61);
            this.richTextBox4.TabIndex = 7;
            this.richTextBox4.Text = Localization.Dialogs.BenchmarkForm.richTextBox4_Text;
            // 
            // startFunctionsTestButton
            // 
            this.startFunctionsTestButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F);
            this.startFunctionsTestButton.Location = new System.Drawing.Point(10, 76);
            this.startFunctionsTestButton.Margin = new System.Windows.Forms.Padding(2);
            this.startFunctionsTestButton.Name = "startFunctionsTestButton";
            this.startFunctionsTestButton.Size = new System.Drawing.Size(196, 58);
            this.startFunctionsTestButton.TabIndex = 10;
            this.startFunctionsTestButton.Text = Localization.Dialogs.BenchmarkForm.startFunctionsTestButton_Text;
            this.startFunctionsTestButton.UseVisualStyleBackColor = true;
            this.startFunctionsTestButton.Click += new System.EventHandler(this.startFunctionsTestButton_Click);
            // 
            // functionsTestProgressBar
            // 
            this.functionsTestProgressBar.Location = new System.Drawing.Point(10, 170);
            this.functionsTestProgressBar.Margin = new System.Windows.Forms.Padding(2);
            this.functionsTestProgressBar.Name = "functionsTestProgressBar";
            this.functionsTestProgressBar.Size = new System.Drawing.Size(196, 72);
            this.functionsTestProgressBar.Step = 1;
            this.functionsTestProgressBar.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F);
            this.label2.Location = new System.Drawing.Point(10, 244);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 24);
            this.label2.TabIndex = 9;
            this.label2.Text = Localization.Dialogs.BenchmarkForm.label2_Text;
            // 
            // functionsTestRichTextBox
            // 
            this.functionsTestRichTextBox.BackColor = System.Drawing.Color.Lavender;
            this.functionsTestRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.functionsTestRichTextBox.Location = new System.Drawing.Point(10, 270);
            this.functionsTestRichTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.functionsTestRichTextBox.Name = "functionsTestRichTextBox";
            this.functionsTestRichTextBox.ReadOnly = true;
            this.functionsTestRichTextBox.Size = new System.Drawing.Size(196, 82);
            this.functionsTestRichTextBox.TabIndex = 8;
            this.functionsTestRichTextBox.Text = "";
            // 
            // memoryTestBackgroundWorker
            // 
            this.memoryTestBackgroundWorker.WorkerReportsProgress = true;
            this.memoryTestBackgroundWorker.WorkerSupportsCancellation = true;
            // 
            // functionsTestBackgroundWorker
            // 
            this.functionsTestBackgroundWorker.WorkerReportsProgress = true;
            this.functionsTestBackgroundWorker.WorkerSupportsCancellation = true;
            // 
            // BenchmarkForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 362);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BenchmarkForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Benchmark";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox memoryTestRichTextBox;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ProgressBar memoryTestProgressBar;
        private System.Windows.Forms.Button cancelMemoryTestButton;
        private System.Windows.Forms.Button startMemoryTestButton;
        private System.Windows.Forms.Button cancelFunctionsTestButton;
        private System.Windows.Forms.RichTextBox richTextBox4;
        private System.Windows.Forms.Button startFunctionsTestButton;
        private System.Windows.Forms.ProgressBar functionsTestProgressBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox functionsTestRichTextBox;
        private System.ComponentModel.BackgroundWorker memoryTestBackgroundWorker;
        private System.ComponentModel.BackgroundWorker functionsTestBackgroundWorker;
    }
}