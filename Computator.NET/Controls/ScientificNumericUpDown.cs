using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Helpers;
using Computator.NET.DataTypes;

// ReSharper disable LocalizableElement

namespace Computator.NET.Controls
{
    public sealed partial class ScientificNumericUpDown : NumericUpDown
    {
        private readonly int _multiplyFactor = 10;

        private const string Exponents = "⁰¹²³⁴⁵⁶⁷⁸⁹⁻";
        private const string ToReplace = "0123456789-";

        public ScientificNumericUpDown()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                Minimum = decimal.MinValue / 10;
                Maximum = decimal.MaxValue / 10;


                TextAlign = HorizontalAlignment.Center;
                Font = base.Font;

                ValueChanged += (o, e) =>
                {
                    if (!ExponentialMode)
                        Increment = Math.Max(Epsilon,
                            Math.Abs((0.3m * Value).RoundToSignificantDigits(1)));
                    //if (Increment == 0)
                    //Increment = 1;
                };
            }
        }


        public decimal Epsilon { get; set; } = 0.001m;

        public new decimal Value
        {
            get { return base.Value; }
            set { base.Value = Constrain(value); }
        }

        private new bool DesignMode
        {
            get
            {
                if (base.DesignMode)
                    return true;

                return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            }
        }

        public new Font Font
        {
            get { return base.Font; }
            set { base.Font = !DesignMode ? CustomFonts.GetMathFont(value.Size) : value; }
        }

        public bool ExponentialMode => ((double)Value).ToString(CultureInfo.InvariantCulture).Contains('E') ||
                                       ((double)Value).ToString(CultureInfo.InvariantCulture).Contains('e');


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int DecimalPlaces
        {
            get { return base.DecimalPlaces; }
            set { base.DecimalPlaces = value; }
        }


        private int CaretPosition => (Controls[1] as TextBoxBase).SelectionStart;

        private bool IsCaretInExponent()
        {
            for (var i = CaretPosition - 1; i >= 3; i--)
            {
                if (Exponents.Contains(Text[i]))
                    continue;
                return Text[i] == '0' &&
                       Text[i - 1] == '1' &&
                       Text[i - 2] == SpecialSymbols.DotSymbol;
            }

            return false;
        }

        protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
        {
            if (IsCaretInExponent())
            {
                var expKeyChar = ConvertToExponent(e.KeyChar);
                if (expKeyChar != ' ' && !(Text.Contains('⁻') && expKeyChar == '⁻'))
                    e.KeyChar = expKeyChar;
                else if (!char.IsControl(e.KeyChar))
                    e.Handled = true;
            }
            else if (e.KeyChar == '-' &&
                     !((IsCharOnLeftOfCaret('E') || IsCharOnLeftOfCaret('e')) && !IsCharOnRightOfCaret('-')) &&
                     !(CaretPosition == 0 && !Text.StartsWith("-")))
                e.Handled = true;

            else if (e.KeyChar == '*' && !Text.Contains(SpecialSymbols.DotSymbol))
                e.KeyChar = SpecialSymbols.DotSymbol;
            else if (!Text.Contains("e") && !Text.Contains("E") && (e.KeyChar == 'E' || e.KeyChar == 'e'))
            {
                // Text += dotSymbol + "10";
                // e.Handled = true;
            }
            else if (!Text.Contains(".") && (e.KeyChar == ',' || e.KeyChar == '.' ||
                     e.KeyChar == Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0] ||
                     e.KeyChar == Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator[0]))
            {
                e.KeyChar = '.';
                // //Text += '.';
                // //e.Handled = true;
            }
            else if (Text.Contains(".") && (e.KeyChar == ',' || e.KeyChar == '.' ||
                                             e.KeyChar ==
                                             Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0] ||
                                             e.KeyChar ==
                                             Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator[0
                                             ]))
            {
                e.Handled = true;
            }
            else
                base.OnTextBoxKeyPress(source, e);
        }

        private bool IsCharOnRightOfCaret(char v)
        {
            if (CaretPosition > Text.Length - 1)
                return false;

            return Text[CaretPosition] == v;
        }

        private bool IsCharOnLeftOfCaret(char c)
        {
            if (CaretPosition < 1 || Text.Length == 0)
                return false;

            return Text[CaretPosition - 1] == c;
        }

        protected override void ValidateEditText() //basically it sets Value after Text was edited by User
        {
            try
            {
                // VSWhidbey 173332: Verify that the user is not starting the string with a "-"
                // before attempting to set the Value property since a "-" is a valid character with
                // which to start a string representing a negative number.
                if (!string.IsNullOrEmpty(Text) &&
                    !(Text.Length == 1 && Text == "-"))
                {
                    Value = Constrain(Text.FromMathString());
                }
            }
            catch
            {
                // Leave value as it is
            }
            finally
            {
                UserEdit = false;
            }

            UpdateEditText();
        }



        private decimal Constrain(double value)
        {
            Debug.Assert(Minimum <= Maximum,
                         "minimum > maximum");

            if (value < (double)Minimum)
            {
                return Minimum;
            }

            if (value > (double)Maximum)
            {
                return Maximum;
            }

            return (decimal)value;
        }

        private decimal Constrain(decimal value)
        {

            Debug.Assert(Minimum <= Maximum,
                         "minimum > maximum");

            if (value < Minimum)
            {
                value = Minimum;
            }

            if (value > Maximum)
            {
                value = Maximum;
            }

            return value;
        }


        public override void UpButton()
        {
            if (!ExponentialMode)
            {
                base.UpButton();
                Value = Value.RoundToSignificantDigits(2);
                //beware it's kind of experimental, 1 instead of two would give generally better results but wight have stopped progres in some cases like 0.001
            }
            else
            {
                if (Value > 0)
                    Value = Constrain(Value * _multiplyFactor);
                else if (Value < 0)
                    Value = Constrain(Value / _multiplyFactor);

                //UpdateEditText();
            }
        }

        public override void DownButton()
        {
            if (!ExponentialMode)
            {
                base.DownButton();
                Value = Value.RoundToSignificantDigits(2);
                //beware it's kind of experimental, 1 instead of two would give generally better results but wight have stopped progres in some cases like 0.001
            }
            else
            {
                if (Value > 0)
                    Value = Constrain(Value / _multiplyFactor);
                else if (Value < 0)
                    Value = Constrain(Value * _multiplyFactor);
                //UpdateEditText();
            }
        }


        protected override void UpdateEditText() //basically it sets Text after Value was established
        {
            Text = Value.ToMathString();
        }


        private char ConvertToExponent(char v)
        {
            // var sb = new StringBuilder(v);

            //for (var i = 0; i < sb.Length; i++)
            for (var j = 0; j < Exponents.Length; j++)
                if (v == ToReplace[j])
                    return Exponents[j];
            return ' ';
        }
    }
}