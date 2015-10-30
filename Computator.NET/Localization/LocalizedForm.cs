namespace Computator.NET.Localization
{
    public class LocalizedForm : System.Windows.Forms.Form
    {
        protected System.Globalization.CultureInfo culture;
        protected System.ComponentModel.ComponentResourceManager resManager;

        public LocalizedForm()
        {
            resManager = new System.ComponentModel.ComponentResourceManager(GetType());
            culture = System.Globalization.CultureInfo.CurrentUICulture;
        }

        /// <summary>
        ///     Current culture of this form
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.Description("Current culture of this form")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public System.Globalization.CultureInfo Culture
        {
            get { return culture; }
            set
            {
                if (culture != value)
                {
                    ApplyResources(this, value);

                    culture = value;
                    OnCultureChanged();
                }
            }
        }

        /// <summary>
        ///     Occurs when current UI culture is changed
        /// </summary>
        [System.ComponentModel.Browsable(true)]
        [System.ComponentModel.Description("Occurs when current UI culture is changed")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ComponentModel.Category("Property Changed")]
        public event System.EventHandler CultureChanged;

        private void ApplyResources(System.Windows.Forms.Control parent, System.Globalization.CultureInfo culture)
        {
            resManager.ApplyResources(parent, parent.Name, culture);
            //ScanNonControls();
            foreach (System.Windows.Forms.Control ctl in parent.Controls)
            {
                ApplyResources(ctl, culture);
            }
        }

        protected void OnCultureChanged()
        {
            var temp = CultureChanged;
            if (temp != null)
                temp(this, System.EventArgs.Empty);
        }

        protected void ScanNonControls()
        {
            var fieldInfo = GetType().GetFields(System.Reflection.BindingFlags.NonPublic
                                                | System.Reflection.BindingFlags.Instance |
                                                System.Reflection.BindingFlags.Public);
            for (var i = 0; i < fieldInfo.Length; i++)
            {
                var obj = fieldInfo[i].GetValue(this);
                var fieldName = fieldInfo[i].Name;
                if (obj is System.Windows.Forms.ToolStripMenuItem)
                {
                    var menuItem = (System.Windows.Forms.ToolStripMenuItem) obj;
                    menuItem.Text = (string) (resManager.GetObject(fieldName +
                                                                   ".Text", culture));
                    // etc.
                }
            }
        }
    }
}