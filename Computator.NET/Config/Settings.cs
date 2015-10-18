using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.Config
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            propertyGrid1.LargeButtons = true;
            propertyGrid1.Font = new Font(propertyGrid1.Font.FontFamily, 14);
            propertyGrid1.SelectedObject = Properties.Settings.Default;
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}