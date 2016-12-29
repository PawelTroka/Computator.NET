using System.IO;
using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;

namespace Computator.NET.UI.Dialogs
{
    public sealed partial class ChangelogForm : Form
    {
        public ChangelogForm()
        {
            InitializeComponent();
            Text = Strings.GUI_changelogToolStripMenuItem_Click_Changelog;
            using (var sr = new StreamReader(GlobalConfig.FullPath("CHANGELOG")))
            {
                _richTextBox.Text = sr.ReadToEnd();
            }
        }
    }
}