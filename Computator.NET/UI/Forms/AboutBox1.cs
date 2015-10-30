namespace Computator.NET
{
    partial class AboutBox1 : System.Windows.Forms.Form
    {
        public AboutBox1()
        {
            InitializeComponent();
            Text = string.Format("About {0}", AssemblyTitle);
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = string.Format("Version {0}", AssemblyVersion);
            labelCopyright.Text = AssemblyCopyright;
            labelCompanyName.Text = AssemblyCompany;
            textBoxDescription.Text = AssemblyDescription + Localization.Strings.ItSFeaturesInclude +
                                      Config.GlobalConfig.features.Replace(" - ", "\r\n - ");
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                var attributes =
                    System.Reflection.Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof (System.Reflection.AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (System.Reflection.AssemblyTitleAttribute) attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return
                    System.IO.Path.GetFileNameWithoutExtension(
                        System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public string AssemblyDescription
        {
            get
            {
                var attributes =
                    System.Reflection.Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof (System.Reflection.AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyDescriptionAttribute) attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                var attributes =
                    System.Reflection.Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof (System.Reflection.AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                var attributes =
                    System.Reflection.Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof (System.Reflection.AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                var attributes =
                    System.Reflection.Assembly.GetExecutingAssembly()
                        .GetCustomAttributes(typeof (System.Reflection.AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((System.Reflection.AssemblyCompanyAttribute) attributes[0]).Company;
            }
        }

        #endregion
    }
}