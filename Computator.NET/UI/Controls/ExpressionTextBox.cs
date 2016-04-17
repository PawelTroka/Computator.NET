using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Controls
{
    public interface IExpressionView : ITextProvider
    {
        event EventHandler TextChanged;
    }

    internal class ExpressionTextBox : TextBox, INotifyPropertyChanged, IExpressionView
    {
        private AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;
        private bool _exponentMode;

        public ExpressionTextBox()
        {
            InitializeComponent();
            _exponentMode = false;
            GotFocus += ExpressionTextBox_GotFocus;
            MouseDoubleClick += Control_MouseDoubleClick;
            SetFont(Settings.Default.ExpressionFont);
            SizeChanged +=
                (o, e) =>
                {
                    _autocompleteMenu.MaximumSize = new Size(Size.Width, _autocompleteMenu.MaximumSize.Height);
                };

            Settings.Default.PropertyChanged += (o, e) =>
            {
                switch (e.PropertyName)
                {
                    case "FunctionsOrder":
                        this.RefreshAutoComplete();
                        break;

                    case "ExpressionFont":
                        this.SetFont(Settings.Default.ExpressionFont);
                        break;
                }
            };

            if(!DesignMode)
                RefreshAutoComplete();

            EventAggregator.Instance.Subscribe<ExponentModeChangedEvent>( emce => ExponentMode=emce.IsExponentMode);
        }

        public bool ExponentMode
        {
            get { return _exponentMode; }
            set
            {
                if (value != _exponentMode)
                {
                    _exponentMode = value;
                    _showCaret();
                    OnPropertyChanged(nameof(ExponentMode));
                    //Invalidate();
                }
            }
        }

        public bool Sort
            => Settings.Default.FunctionsOrder == FunctionsOrder.Alphabetical;

        public bool IsInDesignMode
        {
            get
            {
                var isInDesignMode = LicenseManager.UsageMode ==
                                     LicenseUsageMode.Designtime ||
                                     Debugger.IsAttached;

                if (!isInDesignMode)
                {
                    using (var process = Process.GetCurrentProcess())
                    {
                        return process.ProcessName.ToLowerInvariant().Contains("devenv");
                    }
                }

                return true;
            }
        }

        public override string Text
        {
            get { return base.Text.Replace('*', SpecialSymbols.DotSymbol); }
            set { base.Text = value.Replace('*', SpecialSymbols.DotSymbol); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Control_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ExponentMode = false;
        }

        private void ExpressionTextBox_GotFocus(object sender, EventArgs e)
        {
            _showCaret();
        }

        /// <summary>
        ///     test case:
        ///     tg(x)·H(x)+2.2312·root(z,2)+zᶜᵒˢ⁽ᶻ⁾+xʸ+MathieuMc(1,2,y,x)
        /// </summary>
        private void _showCaret()
        {
            var blob = TextRenderer.MeasureText("x", Font);
            if (ExponentMode)
                NativeMethods.CreateCaret(Handle, IntPtr.Zero, 2, blob.Height/2);
            else
                NativeMethods.CreateCaret(Handle, IntPtr.Zero, 2, blob.Height);
            NativeMethods.ShowCaret(Handle);
        }

        private void InitializeComponent()
        {
            KeyPress += ExpressionTextBox_KeyPress;
            _autocompleteMenu = new AutocompleteMenuNS.AutocompleteMenu();
            _autocompleteMenu.SetAutocompleteMenu(this, _autocompleteMenu);
        }

        public void SetFont(Font font)
        {
            if (font.FontFamily.Name == "Cambria" && !IsInDesignMode)
            {
                Font = CustomFonts.GetMathFont(font.Size);
                _autocompleteMenu.Font = CustomFonts.GetMathFont(font.Size);
            }
            else
            {
                Font = font;
                _autocompleteMenu.Font = font;
            }
        }

        private void RefreshAutoComplete()
        {
            var array = AutocompletionData.GetAutocompleteItemsForExpressions(true);
            if (Sort)
                Array.Sort(array, (a, b) => a.Text.CompareTo(b.Text));
            _autocompleteMenu.SetAutocompleteItems(array);
            //RefreshSize();

            //this.autocompleteMenu.deserialize();
        }

        public void RefreshSize()
        {
            _autocompleteMenu.MaximumSize = new Size(Size.Width, _autocompleteMenu.MaximumSize.Height);
        }

        private void ExpressionTextBox_KeyPress(object s, KeyPressEventArgs e)
        {
            if (ExponentMode)
            {
                if (SpecialSymbols.AsciiForSuperscripts.Contains(e.KeyChar))
                {
                    e.KeyChar = SpecialSymbols.AsciiToSuperscript(e.KeyChar);
                }
            }

            if (IsOperator(e.KeyChar))
            {
                if (e.KeyChar == SpecialSymbols.ExponentModeSymbol)
                {
                    ExponentMode = !ExponentMode;
                    //_showCaret();
                    e.Handled = true;
                    //return;
                }

                if (e.KeyChar == '*')
                {
                    e.KeyChar = SpecialSymbols.DotSymbol;
                    //for (int i = 0; i < this.AutoCompleteCustomSource.Count; i++)
                    // this.AutoCompleteCustomSource[i] += Text + e.KeyChar;
                }
            }
        }

        private static bool IsOperator(char c)
        {
            if (c == '*' || c == '/' || c == '+' || c == '-' || c == '(' || c == '^' || c == '!')
                return true;
            return false;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}