using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.DataTypes.Text;

namespace Computator.NET.Desktop.Controls
{
    public class ExpressionTextBox : TextBox, IExpressionTextBox
    {
        private ISharedViewState _sharedViewState;
        private IFunctionsDetails _functionsDetails;
        private AutocompleteMenu.AutocompleteMenu _autocompleteMenu;

        public ExpressionTextBox(ISharedViewState sharedViewState, IFunctionsDetails functionsDetails)
        {
            _sharedViewState = sharedViewState;
            _functionsDetails = functionsDetails;
            KeyPress += ExpressionTextBox_KeyPress;
            _autocompleteMenu = new AutocompleteMenu.AutocompleteMenu(_sharedViewState, _functionsDetails);
            _autocompleteMenu.SetAutocompleteMenu(this, _autocompleteMenu);

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
                    case nameof(Settings.Default.FunctionsOrder):
                        RefreshAutoComplete();
                        break;

                    case nameof(Settings.Default.ExpressionFont):
                        SetFont(Settings.Default.ExpressionFont);
                        break;
                }
            };

            if (!DesignMode)
            {
                RefreshAutoComplete();
                _sharedViewState.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == nameof(SharedViewState.IsExponent)) _showCaret();
                };
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


        private void Control_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            _sharedViewState.IsExponent = false;
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
            if (RuntimeInformation.IsUnix)
                return;//TODO: find a way to draw caret in Unix
            var blob = TextRenderer.MeasureText("x", Font);
            if (_sharedViewState.IsExponent)
                NativeMethods.CreateCaret(Handle, IntPtr.Zero, 2, blob.Height/2);
            else
                NativeMethods.CreateCaret(Handle, IntPtr.Zero, 2, blob.Height);
            NativeMethods.ShowCaret(Handle);
        }

        public void SetFont(Font font)
        {
            Font = _autocompleteMenu.Font = CustomFonts.GetFontFromFont(font);
        }

        private void RefreshAutoComplete()
        {
            var array = AutocompletionData.GetAutocompleteItemsForExpressions(_functionsDetails,true);
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
            if (_sharedViewState.IsExponent)
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
                    _sharedViewState.IsExponent = !_sharedViewState.IsExponent;
                    _showCaret();
                    // EventAggregator.Instance.Publish<ExponentModeChangedEvent>(new ExponentModeChangedEvent(_sharedViewState.IsExponent));
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