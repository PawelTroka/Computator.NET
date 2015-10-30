namespace Computator.NET.Config
{
    public partial class Settings : System.Windows.Forms.Form
    {
        public Settings()
        {
            InitializeComponent();
            propertyGrid1.LargeButtons = true;
            propertyGrid1.Font = new System.Drawing.Font(propertyGrid1.Font.FontFamily, 14);
            propertyGrid1.SelectedObject = Properties.Settings.Default;
        }

        private void Settings_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}