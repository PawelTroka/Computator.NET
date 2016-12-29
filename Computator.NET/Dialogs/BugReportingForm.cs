using System;
using System.Diagnostics;
using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.Dialogs
{
    public partial class BugReportingForm : Form
    {
        public BugReportingForm()
        {
            InitializeComponent();
            _richtextbox.Text =
                $@"{Environment.NewLine}{Strings.PleaseReportAnyBugsToPawełTrokaPtrokaFizykaDk}{Environment.NewLine
                    }{Environment.NewLine}{
                    Strings
                        .GUI_bugReportingToolStripMenuItem_Click_Or_even_better_report_them_on_project_site__using_link_below
                    }{Environment.NewLine}{GlobalConfig.IssuesUrl}";
            _richtextbox.LinkClicked += (ooo, eee) => Process.Start(GlobalConfig.IssuesUrl);

            Text = Strings.BugReporting;
        }
    }
}