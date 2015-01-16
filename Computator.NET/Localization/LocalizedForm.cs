using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace Computator.NET.Localization
{
    public class LocalizedForm : Form
    {
        protected CultureInfo culture;
        protected ComponentResourceManager resManager;

        public LocalizedForm()
        {
            resManager = new ComponentResourceManager(GetType());
            culture = CultureInfo.CurrentUICulture;
        }

        /// <summary>
        ///     Current culture of this form
        /// </summary>
        [Browsable(false)]
        [Description("Current culture of this form")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CultureInfo Culture
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
        [Browsable(true)]
        [Description("Occurs when current UI culture is changed")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [Category("Property Changed")]
        public event EventHandler CultureChanged;

        private void ApplyResources(Control parent, CultureInfo culture)
        {
            resManager.ApplyResources(parent, parent.Name, culture);
            //ScanNonControls();
            foreach (Control ctl in parent.Controls)
            {
                ApplyResources(ctl, culture);
            }
        }

        protected void OnCultureChanged()
        {
            EventHandler temp = CultureChanged;
            if (temp != null)
                temp(this, EventArgs.Empty);
        }

        protected void ScanNonControls()
        {
            FieldInfo[] fieldInfo = GetType().GetFields(BindingFlags.NonPublic
                                                        | BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < fieldInfo.Length; i++)
            {
                object obj = fieldInfo[i].GetValue(this);
                string fieldName = fieldInfo[i].Name;
                if (obj is ToolStripMenuItem)
                {
                    var menuItem = (ToolStripMenuItem) obj;
                    menuItem.Text = (string) (resManager.GetObject(fieldName +
                                                                   ".Text", culture));
                    // etc.
                }
            }
        }
    }
}