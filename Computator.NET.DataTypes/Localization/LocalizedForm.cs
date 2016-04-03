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
                    //ScanNonControls();
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

                var menuStr = ctl as MenuStrip;
                if (menuStr == null) continue;

                var menuStrip = menuStr;

                foreach (ToolStripItem item in menuStrip.Items)
                {
                    ApplyResources(item, culture);
                }
            }
        }

        private void ApplyResources(ToolStripItem item, CultureInfo culture)
        {
            resManager.ApplyResources(item, item.Name, culture);

            var downItem = item as ToolStripDropDownItem;
            if (downItem == null) return;

            foreach (ToolStripItem it in downItem.DropDownItems)
            {
                ApplyResources(it, culture);
            }
        }


        protected void OnCultureChanged()
        {
            var temp = CultureChanged;
            if (temp != null)
                temp(this, EventArgs.Empty);
        }

        protected void ScanNonControls()
        {
            LocalizeType(GetType(), this, 1);
        }

        private void LocalizeType(Type type, object owner, int howDeep)
        {
            if (owner == null|| howDeep==0)
                return;


                var memberInfos = type.GetMembers(BindingFlags.NonPublic
                              | BindingFlags.Instance |
                              BindingFlags.Public);

                foreach (MemberInfo memberInfo in memberInfos)
                {
                    LocalizeMember(memberInfo, owner);

                    var fieldInfo = memberInfo as FieldInfo;
                    if (fieldInfo != null && !fieldInfo.FieldType.IsValueType)
                        LocalizeType(fieldInfo.FieldType,fieldInfo.GetValue(owner),howDeep-1);

                    var propertyInfo = memberInfo as PropertyInfo;
                    if (propertyInfo != null && propertyInfo.CanRead && !propertyInfo.PropertyType.IsValueType && propertyInfo.GetIndexParameters().Length == 0)
                        try { LocalizeType(propertyInfo.PropertyType, propertyInfo.GetValue(owner,null), howDeep - 1);}
                        catch(Exception ex)
                        {
                        }
                }

        }

        private void LocalizeMember(MemberInfo member, object owner)
        {
            object obj = null;
            if(member is FieldInfo)
             obj= ((FieldInfo)member).GetValue(owner);
            else if(member is PropertyInfo && ((PropertyInfo)member).CanRead && ((PropertyInfo)member).GetIndexParameters().Length==0)
               try { obj = ((PropertyInfo)member).GetValue(owner,null);}
               catch
               {
               }
            var fieldName = member.Name;
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