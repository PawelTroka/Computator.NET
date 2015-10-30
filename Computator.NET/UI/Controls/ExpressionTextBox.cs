using Enumerable = System.Linq.Enumerable;

namespace Computator.NET.UI.Controls
{
    internal class ExpressionTextBox : System.Windows.Forms.TextBox, System.ComponentModel.INotifyPropertyChanged
    {
        private AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;
        private bool _exponentMode;

        public ExpressionTextBox()
        {
            InitializeComponent();
            _exponentMode = false;
            GotFocus += ExpressionTextBox_GotFocus;
            MouseDoubleClick += Control_MouseDoubleClick;
            SetFont(Properties.Settings.Default.ExpressionFont);
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
            => Properties.Settings.Default.FunctionsOrder == DataTypes.SettingsTypes.FunctionsOrder.Alphabetical;

        public override string Text
        {
            get { return base.Text.Replace('*', DataTypes.SpecialSymbols.DotSymbol); }
            set { base.Text = value.Replace('*', DataTypes.SpecialSymbols.DotSymbol); }
        }

        public string Expression => base.Text.Replace(DataTypes.SpecialSymbols.DotSymbol, '*');

        public bool IsInDesignMode
        {
            get
            {
                var isInDesignMode = System.ComponentModel.LicenseManager.UsageMode ==
                                     System.ComponentModel.LicenseUsageMode.Designtime ||
                                     System.Diagnostics.Debugger.IsAttached;

                if (!isInDesignMode)
                {
                    using (var process = System.Diagnostics.Process.GetCurrentProcess())
                    {
                        return process.ProcessName.ToLowerInvariant().Contains("devenv");
                    }
                }

                return true;
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void Control_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ExponentMode = false;
        }

        private void ExpressionTextBox_GotFocus(object sender, System.EventArgs e)
        {
            _showCaret();
        }

        /// <summary>
        ///     test case:
        ///     tg(x)·H(x)+2.2312·root(z,2)+zᶜᵒˢ⁽ᶻ⁾+xʸ+MathieuMc(1,2,y,x)
        /// </summary>
        private void _showCaret()
        {
            var blob = System.Windows.Forms.TextRenderer.MeasureText("x", Font);
            if (ExponentMode)
                NativeMethods.CreateCaret(Handle, System.IntPtr.Zero, 2, blob.Height/2);
            else
                NativeMethods.CreateCaret(Handle, System.IntPtr.Zero, 2, blob.Height);
            NativeMethods.ShowCaret(Handle);
        }

        private void InitializeComponent()
        {
            KeyPress += ExpressionTextBox_KeyPress;
            _autocompleteMenu = new AutocompleteMenuNS.AutocompleteMenu();
            _autocompleteMenu.SetAutocompleteMenu(this, _autocompleteMenu);
        }

        public void SetFont(System.Drawing.Font font)
        {
            if (font.FontFamily.Name == "Cambria" && !IsInDesignMode)
            {
                Font = Config.MathCustomFonts.GetMathFont(font.Size);
                _autocompleteMenu.Font = Config.MathCustomFonts.GetMathFont(font.Size);
            }
            else
            {
                Font = font;
                _autocompleteMenu.Font = font;
            }
        }

        public void RefreshAutoComplete()
        {
            var array = Data.AutocompletionData.GetAutocompleteItemsForExpressions(true);
            if (Sort)
                System.Array.Sort(array, (a, b) => a.Text.CompareTo(b.Text));
            _autocompleteMenu.SetAutocompleteItems(array);
            RefreshSize();

            //this.autocompleteMenu.deserialize();
        }

        public void RefreshSize()
        {
            _autocompleteMenu.MaximumSize = new System.Drawing.Size(Size.Width, _autocompleteMenu.MaximumSize.Height);
        }

        private void ExpressionTextBox_KeyPress(object s, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (ExponentMode)
            {
                if (Enumerable.Contains(DataTypes.SpecialSymbols.AsciiForSuperscripts, e.KeyChar))
                {
                    e.KeyChar = DataTypes.SpecialSymbols.AsciiToSuperscript(e.KeyChar);
                }
            }

            if (IsOperator(e.KeyChar))
            {
                if (e.KeyChar == DataTypes.SpecialSymbols.ExponentModeSymbol)
                {
                    ExponentMode = !ExponentMode;
                    //_showCaret();
                    e.Handled = true;
                    //return;
                }

                if (e.KeyChar == '*')
                {
                    e.KeyChar = DataTypes.SpecialSymbols.DotSymbol;
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
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}