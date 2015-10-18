using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AutocompleteMenuNS;
using Computator.NET.Config;
using Computator.NET.Data;
using Computator.NET.DataTypes.SettingsTypes;
using Settings = Computator.NET.Properties.Settings;

namespace Computator.NET.UI.Controls
{
    internal class ExpressionTextBox : TextBox
    {
        private bool _exponentMode;
        private AutocompleteMenuNS.AutocompleteMenu _autocompleteMenu;

        public ExpressionTextBox()
        {
            InitializeComponent();
            _exponentMode = false;
            GotFocus += ExpressionTextBox_GotFocus;
            MouseDoubleClick += Control_MouseDoubleClick;
            SetFont(Settings.Default.ExpressionFont);
        }

        public bool ExponentMode
        {
            get { return _exponentMode; }
            private set
            {
                if (value != _exponentMode)
                {
                    _exponentMode = value;
                    _showCaret();
                    //Invalidate();
                }
            }
        }

        public bool Sort => Settings.Default.FunctionsOrder == FunctionsOrder.Alphabetical;

        public override string Text
        {
            get { return base.Text.Replace('*', SpecialSymbols.DotSymbol); }
            set { base.Text = value.Replace('*', SpecialSymbols.DotSymbol); }
        }

        public string Expression => base.Text.Replace(SpecialSymbols.DotSymbol, '*');

        private void Control_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ExponentMode = false;
        }

        [DllImport("user32.dll")]
        private static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport("user32.dll")]
        private static extern bool ShowCaret(IntPtr hWnd);

        private void ExpressionTextBox_GotFocus(object sender, EventArgs e)
        {
            _showCaret();
        }
        /// <summary>
        /// test case:
        /// tg(x)·H(x)+2.2312·root(z,2)+zᶜᵒˢ⁽ᶻ⁾+xʸ+MathieuMc(1,2,y,x)
        /// </summary>

        private void _showCaret()
        {
            var blob = TextRenderer.MeasureText("x", Font);
            if (ExponentMode)
                CreateCaret(Handle, IntPtr.Zero, 2, blob.Height / 2);
            else
                CreateCaret(Handle, IntPtr.Zero, 2, blob.Height);
            ShowCaret(Handle);
        }

        private void InitializeComponent()
        {
            KeyPress += ExpressionTextBox_KeyPress;
            _autocompleteMenu = new AutocompleteMenuNS.AutocompleteMenu();
            _autocompleteMenu.SetAutocompleteMenu(this, _autocompleteMenu);
        }

        public bool IsInDesignMode
        {
            get
            {
                bool isInDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime || Debugger.IsAttached;

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

        public void SetFont(Font font)
        {
            if (font.FontFamily.Name == "Cambria" && !IsInDesignMode)
            {
                this.Font = MathCustomFonts.GetMathFont(font.Size);
                this._autocompleteMenu.Font = MathCustomFonts.GetMathFont(font.Size);
            }
            else
            {
                this.Font = font;
                this._autocompleteMenu.Font = font;
            }
        }

        public void RefreshAutoComplete()
        {
            var array = AutocompletionData.GetAutocompleteItemsForExpressions(true);
            if (Sort)
                Array.Sort(array, (a, b) => a.Text.CompareTo(b.Text));
            _autocompleteMenu.SetAutocompleteItems(array);
            RefreshSize();

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


    }
}