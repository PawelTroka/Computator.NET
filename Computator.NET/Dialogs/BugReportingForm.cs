using System;
using System.Diagnostics;
using System.Windows.Forms;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.DataTypes.Properties;

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
                        .Or_even_better_report_them_on_project_site__using_link_below
                    }{Environment.NewLine}{AppRelatedUrls.IssuesUrl}";
            _richtextbox.LinkClicked += (ooo, eee) => Process.Start(AppRelatedUrls.IssuesUrl);

            Text = Strings.BugReporting;
            this.Icon = GraphicsResources.computator_net_icon;
        }
    }
}