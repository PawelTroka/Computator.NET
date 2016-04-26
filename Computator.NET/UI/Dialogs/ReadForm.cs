// ReSharper disable RedundantNameQualifier
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation



namespace Computator.NET
{
    internal class ReadForm : System.Windows.Forms.Form
    {
        public const string ToCode =
            @"
    class ReadForm : System.Windows.Forms.Form
    {
        public ReadForm(string str)
        {
            InitializeComponent();
            //this.Text = str;
            this.readQuestion.Text = str;
            this.readQuestion.AutoSize = true;
            this.readQuestion.Invalidate();
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.AutoSize = true;
            this.Invalidate();
        }

        private System.Windows.Forms.Label readQuestion;
    
        public string Result
        {
            get { return this.textBox1.Text; }
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name=""disposing"">true if managed resources should be disposed; otherwise, false.</param>
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.readQuestion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font(""Microsoft Sans Serif"", 18F);
            this.textBox1.Location = new System.Drawing.Point(0, 29);
            this.textBox1.Margin = new System.Windows.Forms.Padding(5);
            this.textBox1.Name = ""textBox1"";
            this.textBox1.Size = new System.Drawing.Size(391, 41);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(0, 78);
            this.button1.Name = ""button1"";
            this.button1.Size = new System.Drawing.Size(391, 50);
            this.button1.TabIndex = 1;
            this.button1.Text = ""OK"";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // readQuestion
            // 
            this.readQuestion.AutoSize = true;
            this.readQuestion.Dock = System.Windows.Forms.DockStyle.Top;
            this.readQuestion.Location = new System.Drawing.Point(0, 0);
            this.readQuestion.Name = ""readQuestion"";
            this.readQuestion.Size = new System.Drawing.Size(68, 29);
            this.readQuestion.TabIndex = 2;
            this.readQuestion.Text = ""read:"";
            // 
            // ReadForm
            // 
            AcceptButton = button1;
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            ShowInTaskbar = false;
            StartPosition= System.Windows.Forms.FormStartPosition.CenterParent;
            ShowIcon = false;
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 128);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.readQuestion);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font(""Microsoft Sans Serif"", 13.8F);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = ""ReadForm"";
            this.Text = ""read"";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
}
";

        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label readQuestion;
        private System.Windows.Forms.TextBox textBox1;

        public ReadForm(string str)
        {
            InitializeComponent();
            //this.Text = str;
            readQuestion.Text = str;
            readQuestion.AutoSize = true;
            readQuestion.Invalidate();
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            AutoSize = true;
            Invalidate();
        }

        public string Result
        {
            get { return textBox1.Text; }
        }

        /// <summary>
        ///     Clean up any resources being used.
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
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.readQuestion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (238)));
            this.textBox1.Location = new System.Drawing.Point(0, 29);
            this.textBox1.Margin = new System.Windows.Forms.Padding(5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(391, 41);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(0, 78);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(391, 50);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // readQuestion
            // 
            this.readQuestion.AutoSize = true;
            this.readQuestion.Dock = System.Windows.Forms.DockStyle.Top;
            this.readQuestion.Location = new System.Drawing.Point(0, 0);
            this.readQuestion.Name = "readQuestion";
            this.readQuestion.Size = new System.Drawing.Size(68, 29);
            this.readQuestion.TabIndex = 2;
            this.readQuestion.Text = "read:";
            // 
            // ReadForm
            // 

            AcceptButton = button1;
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ShowIcon = false;

            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 128);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.readQuestion);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular,
                System.Drawing.GraphicsUnit.Point, ((byte) (238)));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ReadForm";
            this.Text = "read";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}