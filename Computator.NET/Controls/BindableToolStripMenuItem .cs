using System.ComponentModel;
using System.Windows.Forms;

namespace Computator.NET.Controls
{
    internal class BindableToolStripMenuItem : ToolStripMenuItem,
        IBindableComponent, INotifyPropertyChanged
    {
        public new bool Checked
        {
            get { return base.Checked; }
            set
            {
                if (value != base.Checked)
                {
                    base.Checked = value;
                    OnPropertyChanged(nameof(Checked));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region IBindableComponent Members

        private BindingContext bindingContext;
        private ControlBindingsCollection dataBindings;

        [Browsable(false)]
        public BindingContext BindingContext
        {
            get
            {
                if (bindingContext == null)
                {
                    bindingContext = new BindingContext();
                }
                return bindingContext;
            }
            set { bindingContext = value; }
        }

        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Content)]
        public ControlBindingsCollection DataBindings
        {
            get
            {
                if (dataBindings == null)
                {
                    dataBindings = new ControlBindingsCollection(this);
                }
                return dataBindings;
            }
        }

        #endregion
    }
}