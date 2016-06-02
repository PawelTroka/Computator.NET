using System;
using System.Drawing;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.UI.Dialogs
{
    partial class BugReportingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private RichTextBox _richtextbox;

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
            this._richtextbox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // _richtextbox
            // 
            this._richtextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._richtextbox.Location = new System.Drawing.Point(0, 0);
            this._richtextbox.Name = "_richtextbox";
            this._richtextbox.ReadOnly = true;
            this._richtextbox.Size = new System.Drawing.Size(434, 261);
            this._richtextbox.TabIndex = 0;
            this._richtextbox.Text = "";
            // 
            // BugReportingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 261);
            this.Controls.Add(this._richtextbox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 17F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BugReportingForm";
            this.Text = "BugReportingForm";
            this.ResumeLayout(false);

        }

        #endregion
    }
}