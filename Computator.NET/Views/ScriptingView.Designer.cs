namespace Computator.NET.Views
{
    partial class ScriptingView
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.consoleOutputTextBox = new System.Windows.Forms.RichTextBox();
            this.processButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
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
            this.splitContainer1.Panel1.AutoScroll = true;
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.processButton);
            this.splitContainer1.Size = new System.Drawing.Size(556, 397);
            this.splitContainer1.SplitterDistance = 417;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.AutoScroll = true;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AutoScroll = true;
            this.splitContainer2.Panel2.Controls.Add(this.consoleOutputTextBox);
            this.splitContainer2.Size = new System.Drawing.Size(417, 397);
            this.splitContainer2.SplitterDistance = 293;
            this.splitContainer2.SplitterWidth = 2;
            this.splitContainer2.TabIndex = 0;
            // 
            // consoleOutputTextBox
            // 
            this.consoleOutputTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.consoleOutputTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.consoleOutputTextBox.DetectUrls = false;
            this.consoleOutputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consoleOutputTextBox.Font = new System.Drawing.Font("Consolas", 12F);
            this.consoleOutputTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.consoleOutputTextBox.Location = new System.Drawing.Point(0, 0);
            this.consoleOutputTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.consoleOutputTextBox.Name = "consoleOutputTextBox";
            this.consoleOutputTextBox.ReadOnly = true;
            this.consoleOutputTextBox.Size = new System.Drawing.Size(417, 102);
            this.consoleOutputTextBox.TabIndex = 0;
            this.consoleOutputTextBox.Text = Localization.Views.ScriptingView.consoleOutputTextBox_Text;
            // 
            // processButton
            // 
            this.processButton.AutoSize = true;
            this.processButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.processButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.processButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.processButton.Location = new System.Drawing.Point(0, 0);
            this.processButton.Margin = new System.Windows.Forms.Padding(2);
            this.processButton.Name = "processButton";
            this.processButton.Size = new System.Drawing.Size(137, 76);
            this.processButton.TabIndex = 0;
            this.processButton.Text = Localization.Views.ScriptingView.processButton_Text;
            this.processButton.UseVisualStyleBackColor = true;
            // 
            // ScriptingView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ScriptingView";
            this.Size = new System.Drawing.Size(556, 397);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox consoleOutputTextBox;
        private System.Windows.Forms.Button processButton;
    }
}
