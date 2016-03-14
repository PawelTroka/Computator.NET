using System.Configuration;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Config
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            //propertyGrid1.LargeButtons = true;
            //propertyGrid1.Font = new Font(propertyGrid1.Font.FontFamily, 14);
            propertyGrid1.SelectedObject = Properties.Settings.Default;
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Reload();
           // propertyGrid1.Refresh();
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.Reload();
            //propertyGrid1.Refresh();
            Close();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.Save();
            Close();
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.Reset();
            propertyGrid1.Refresh();
        }
    }
}