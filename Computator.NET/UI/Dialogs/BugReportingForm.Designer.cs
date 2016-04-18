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
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "BugReportingForm";


            _richtextbox = new RichTextBox();
            _richtextbox.Dock = DockStyle.Fill;
            _richtextbox.ReadOnly = true;

            Size = new Size(650, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;

                // Set the MaximizeBox to false to remove the maximize box.
            MaximizeBox = false;

                // Set the MinimizeBox to false to remove the minimize box.
            MinimizeBox = false;

                // Set the start position of the form to the center of the screen.
                //StartPosition = FormStartPosition.CenterScreen,
                Controls.Add(_richtextbox);
            Font = new Font(FontFamily.GenericSansSerif, 17.0F);

        }

        #endregion
    }
}