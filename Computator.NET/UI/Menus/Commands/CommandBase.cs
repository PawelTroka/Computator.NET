using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Computator.NET.UI.Commands
{
    
    internal abstract class CommandBase : IToolbarCommand
    {
        private bool _visible;
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
            _visible = true;
        }

        public bool IsOption
        {
            get { return _isOption; }
            set {
                if (_isOption != value)
                {
                    _isOption = value;
                    OnPropertyChanged(nameof(IsOption));
                }
            }
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

        private string _shortcutKeyString;
        private bool _isOption;

        public string ShortcutKeyString
        {
            get { return _shortcutKeyString; }
            set
            {
                if (_shortcutKeyString != value)
                {
                    _shortcutKeyString = value;
                    OnPropertyChanged(nameof(ShortcutKeyString));
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
                    OnPropertyChanged(nameof(IsEnabled));
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
                    OnPropertyChanged(nameof(Icon));
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
                    OnPropertyChanged(nameof(ToolTip));
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

        public bool Visible
        {
            get { return _visible; }
            set {
                if (_visible != value)
                {
                    _visible = value;
                    OnPropertyChanged(nameof(Visible));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}