using System;
using System.Windows.Forms;
using Computator.NET.Core.Properties;
using Computator.NET.Services;

namespace Computator.NET.Dialogs
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            //propertyGrid1.LargeButtons = true;
            //propertyGrid1.Font = new Font(propertyGrid1.Font.FontFamily, 14);
            propertyGrid1.SelectedObject = Settings.Default;
            //this.MakeSureItsNotBiggerThanScreen();
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            ///////////Settings.Default.Reload();
            // propertyGrid1.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ///////////Settings.Default.Reload();
            //propertyGrid1.Refresh();
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings.Default.Save();
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Settings.Default.Reset();
            propertyGrid1.Refresh();
        }
    }
}