namespace Computator.NET.Benchmarking
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BenchmarkForm));
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
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
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
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
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
            this.splitContainer1.Panel2.Controls.Add(this.progressBar2);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.functionsTestRichTextBox);
            // 
            // cancelMemoryTestButton
            // 
            resources.ApplyResources(this.cancelMemoryTestButton, "cancelMemoryTestButton");
            this.cancelMemoryTestButton.Name = "cancelMemoryTestButton";
            this.cancelMemoryTestButton.UseVisualStyleBackColor = true;
            this.cancelMemoryTestButton.Click += new System.EventHandler(this.cancelMemoryTestButton_Click);
            // 
            // startMemoryTestButton
            // 
            resources.ApplyResources(this.startMemoryTestButton, "startMemoryTestButton");
            this.startMemoryTestButton.Name = "startMemoryTestButton";
            this.startMemoryTestButton.UseVisualStyleBackColor = true;
            this.startMemoryTestButton.Click += new System.EventHandler(this.startMemoryTestButton_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // memoryTestRichTextBox
            // 
            this.memoryTestRichTextBox.BackColor = System.Drawing.Color.Lavender;
            this.memoryTestRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.memoryTestRichTextBox, "memoryTestRichTextBox");
            this.memoryTestRichTextBox.Name = "memoryTestRichTextBox";
            this.memoryTestRichTextBox.ReadOnly = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Arrow;
            resources.ApplyResources(this.richTextBox1, "richTextBox1");
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            // 
            // memoryTestProgressBar
            // 
            resources.ApplyResources(this.memoryTestProgressBar, "memoryTestProgressBar");
            this.memoryTestProgressBar.Name = "memoryTestProgressBar";
            this.memoryTestProgressBar.Step = 1;
            // 
            // cancelFunctionsTestButton
            // 
            resources.ApplyResources(this.cancelFunctionsTestButton, "cancelFunctionsTestButton");
            this.cancelFunctionsTestButton.Name = "cancelFunctionsTestButton";
            this.cancelFunctionsTestButton.UseVisualStyleBackColor = true;
            this.cancelFunctionsTestButton.Click += new System.EventHandler(this.cancelFunctionsTestButton_Click);
            // 
            // richTextBox4
            // 
            this.richTextBox4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.richTextBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox4.Cursor = System.Windows.Forms.Cursors.Arrow;
            resources.ApplyResources(this.richTextBox4, "richTextBox4");
            this.richTextBox4.Name = "richTextBox4";
            this.richTextBox4.ReadOnly = true;
            // 
            // startFunctionsTestButton
            // 
            resources.ApplyResources(this.startFunctionsTestButton, "startFunctionsTestButton");
            this.startFunctionsTestButton.Name = "startFunctionsTestButton";
            this.startFunctionsTestButton.UseVisualStyleBackColor = true;
            this.startFunctionsTestButton.Click += new System.EventHandler(this.startFunctionsTestButton_Click);
            // 
            // progressBar2
            // 
            resources.ApplyResources(this.progressBar2, "progressBar2");
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Step = 1;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // functionsTestRichTextBox
            // 
            this.functionsTestRichTextBox.BackColor = System.Drawing.Color.Lavender;
            this.functionsTestRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.functionsTestRichTextBox, "functionsTestRichTextBox");
            this.functionsTestRichTextBox.Name = "functionsTestRichTextBox";
            this.functionsTestRichTextBox.ReadOnly = true;
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
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "BenchmarkForm";
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
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox functionsTestRichTextBox;
        private System.ComponentModel.BackgroundWorker memoryTestBackgroundWorker;
        private System.ComponentModel.BackgroundWorker functionsTestBackgroundWorker;
    }
}