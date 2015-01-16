using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AutocompleteMenuNS;
using Computator.NET.Config;
using Computator.NET.Evaluation;

namespace Computator.NET
{
    internal class ExpressionTextBox : TextBox
    {
        private const char dotSymbol = '·'; //'⋅'
        private const char exponentModeSymbol = '^';
        public static bool toolTipTipOrForm = true; //TODO: move to settings or something 
        private bool _exponentMode;
        private int autcompleteMode = 2;
        private AutocompleteMenu autocompleteMenu;

        public ExpressionTextBox()
        {
            InitializeComponent();
            Sort = false;
            _exponentMode = false;
            GotFocus += ExpressionTextBox_GotFocus;
            MouseDoubleClick += Control_MouseDoubleClick;
        }

        private bool ExponentMode
        {
            get { return _exponentMode; }
            set
            {
                if (value != _exponentMode)
                {
                    _exponentMode = value;
                    _showCaret();
                }
            }
        }


        public bool Sort { get; set; }

        public override string Text
        {
            get { return base.Text.Replace('*', dotSymbol); }
            set { base.Text = value.Replace('*', dotSymbol); }
        }

        public string Expression
        {
            get { return base.Text.Replace(dotSymbol, '*'); }
        }

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

        private void _showCaret()
        {
            Size blob = TextRenderer.MeasureText("x", Font);
            if (ExponentMode)
                CreateCaret(Handle, IntPtr.Zero, 2, blob.Height/2);
            else
                CreateCaret(Handle, IntPtr.Zero, 2, blob.Height);
            ShowCaret(Handle);
        }

        private void InitializeComponent()
        {
            KeyPress += ExpressionTextBox_KeyPress;
            //this.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            //this.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            Name = "expressionTextBox";

            autocompleteMenu = new AutocompleteMenu();
            autocompleteMenu.SetAutocompleteMenu(this, autocompleteMenu);
        }

        public void changeAutocompleteFontToMathFont()
        {
            autocompleteMenu.Font = new Font(GlobalConfig.mathFontFamily, autocompleteMenu.Font.Size,
                autocompleteMenu.Font.Unit); //GlobalConfig.mathFont;
        }

        public void changeAutocompleteFontToMathFont(float fontSize)
        {
            autocompleteMenu.Font = new Font(GlobalConfig.mathFontFamily, fontSize, autocompleteMenu.Font.Unit);
                //GlobalConfig.mathFont;
        }

        public void refreshAutoComplete()
        {
            if (autcompleteMode == 1)
            {
                string[] strings = Evaluator.getAutocompleteStrings();
                var autocompleteItems = new List<SubstringAutocompleteItem>();
                foreach (string s in strings)
                    autocompleteItems.Add(new SubstringAutocompleteItem(s));

                autocompleteMenu.SetAutocompleteItems(autocompleteItems);
            }
            else if (autcompleteMode == 0)
            {
                autocompleteMenu.Items = Evaluator.getAutocompleteStrings();
            }
            else if (autcompleteMode == 2)
            {
                AutocompleteItem[] array = Evaluator.getAutocompleteItems();
                AutocompleteItem[] array2 = array.Distinct(new AutocompleteItemEqualityComparer()).ToArray();
                if (Sort)
                    Array.Sort(array2, (a, b) => a.Text.CompareTo(b.Text));
                autocompleteMenu.SetAutocompleteItems(array2);
            }
            refreshSize();

            //this.autocompleteMenu.deserialize();
        }

        public void refreshSize()
        {
            autocompleteMenu.MaximumSize = new Size(Size.Width, autocompleteMenu.MaximumSize.Height);
        }

        private void ExpressionTextBox_KeyPress(object s, KeyPressEventArgs e)
        {
            if (ExponentMode)
            {
                if (GlobalConfig.AsciiForSuperscripts.Contains(e.KeyChar))
                {
                    e.KeyChar = GlobalConfig.AsciiToSuperscript(e.KeyChar);
                }
            }

            if (isOperator(e.KeyChar))
            {
                if (e.KeyChar == exponentModeSymbol)
                {
                    if (ExponentMode)
                    {
                        ExponentMode = false;
                    }
                    else
                    {
                        ExponentMode = true;
                    }
                    e.Handled = true;
                    return;
                }

                if (e.KeyChar == '*')
                {
                    e.KeyChar = dotSymbol;
                    //for (int i = 0; i < this.AutoCompleteCustomSource.Count; i++)
                    // this.AutoCompleteCustomSource[i] += Text + e.KeyChar;
                }
            }
        }

        private static bool isOperator(char c)
        {
            if (c == '*' || c == '/' || c == '+' || c == '-' || c == '(' || c == '^' || c == '!')
                return true;
            return false;
        }

        public class AutocompleteItemEqualityComparer : IEqualityComparer<AutocompleteItem>
        {
            public bool Equals(AutocompleteItem x, AutocompleteItem y)
            {
                return x.Text == y.Text;
            }

            public int GetHashCode(AutocompleteItem obj)
            {
                return obj.Text.GetHashCode();
            }
        }
    }
}