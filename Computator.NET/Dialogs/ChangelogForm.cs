using System.IO;
using System.Windows.Forms;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Localization;
using Computator.NET.DataTypes.Properties;

namespace Computator.NET.Dialogs
{
    public sealed partial class ChangelogForm : Form
    {
        public ChangelogForm()
        {
            InitializeComponent();
            Text = Strings.Changelog;
            using (var sr = new StreamReader(PathUtility.GetFullPath("CHANGELOG")))
            {
                _richTextBox.Text = sr.ReadToEnd();
            }
            this.Icon = GraphicsResources.computator_net_icon;
        }
    }
}