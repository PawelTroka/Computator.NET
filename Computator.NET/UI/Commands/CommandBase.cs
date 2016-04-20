using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.UI.Commands
{
    public interface IToolbarCommand : INotifyPropertyChanged
    {
        bool IsEnabled { get; set; }
        Image Icon { get; set; }
        string ToolTip { get; set; }
        Keys ShortcutKey { get; set; }
        bool Checked { get; set; }
        bool CheckOnClick { get; set; }

        IEnumerable<IToolbarCommand> ChildrenCommands { get; set; }
        void Execute();
    }

    internal abstract class CommandBase : IToolbarCommand
    {
        private bool _checked;
        private bool _checkOnClick;

        private ToolStripItemImageScaling _imageScaling;
        private ToolStripItemDisplayStyle _displayStyle;

        private string _text;
        private Image icon;
        private bool isEnabled;
        private string toolTip;

        protected CommandBase()
        {
            isEnabled = true;
            _checked = _checkOnClick = false;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract void Execute();

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public Image Icon
        {
            get { return icon; }
            set
            {
                if (icon != value)
                {
                    icon = value;
                    OnPropertyChanged("Icon");
                }
            }
        }

        public string ToolTip
        {
            get { return toolTip; }
            set
            {
                if (toolTip != value)
                {
                    toolTip = value;
                    OnPropertyChanged("ToolTip");
                }
            }
        }

        public Keys ShortcutKey { get; set; }

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    OnPropertyChanged(nameof(Checked));
                }
            }
        }

        public bool CheckOnClick
        {
            get { return _checkOnClick; }
            set
            {
                if (_checkOnClick != value)
                {
                    _checkOnClick = value;
                    OnPropertyChanged(nameof(CheckOnClick));
                }
            }
        }

        public IEnumerable<IToolbarCommand> ChildrenCommands { get; set; }

        public ToolStripItemImageScaling ImageScaling
        {
            get { return _imageScaling; }
            set
            {
                if (_imageScaling != value)
                {
                    _imageScaling = value;
                    OnPropertyChanged(nameof(ImageScaling));
                }
            }
        }

        public ToolStripItemDisplayStyle DisplayStyle
        {
            get { return _displayStyle; }
            set
            {
                if (_displayStyle != value)
                {
                    _displayStyle = value;
                    OnPropertyChanged(nameof(DisplayStyle));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}