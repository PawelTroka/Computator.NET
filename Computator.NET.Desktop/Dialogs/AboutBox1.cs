using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Computator.NET.DataTypes.Properties;
using Computator.NET.Localization;

namespace Computator.NET.Desktop.Dialogs
{
    internal partial class AboutBox1 : Form
    {
        public AboutBox1()
        {
            InitializeComponent();
            Text = string.Format("About {0}", AssemblyTitle);
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = string.Format("Version {0}", AssemblyVersion);
            labelCopyright.Text = AssemblyCopyright;
            labelCompanyName.Text = AssemblyCompany;
            textBoxDescription.Text =
                $"{Strings.GlobalConfig_assemblyDescription_Computator_NET_is_a_special_kind_of_numerical_software_that_is_fast_and_easy_to_use_but_not_worse_than_others_feature_wise_}{Environment.NewLine}{Environment.NewLine}{Strings.ItSFeaturesInclude}{Environment.NewLine}{Strings.featuresInclude}";
            this.Icon = GraphicsResources.computator_net_icon;
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                var attributes =
                    Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute) attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return
                    Path.GetFileNameWithoutExtension(
                        Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public string AssemblyDescription
        {
            get
            {
                var attributes =
                    Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute) attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                var attributes =
                    Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                var attributes =
                    Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                var attributes =
                    Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute) attributes[0]).Company;
            }
        }

        #endregion
    }
}