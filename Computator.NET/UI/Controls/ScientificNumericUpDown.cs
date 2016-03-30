using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;

// ReSharper disable LocalizableElement

namespace Computator.NET.UI
{
    public static class DoubleExtensions
    {
        public static decimal RoundToSignificantDigits(this decimal d, int digits)
        {
            if (d == 0)
                return 0;

            var scale = (decimal)Math.Pow(10, (double) (Math.Floor( (decimal)Math.Log10(Math.Abs((double)d)) ) + 1.0m));
            return scale * Math.Round(d / scale, digits);
        }
    }

    public sealed partial class ScientificNumericUpDown : NumericUpDown
    {
     //   private readonly char dotSymbol = '·'; //'⋅'
        private readonly string exponents = "⁰¹²³⁴⁵⁶⁷⁸⁹⁻";
        private readonly string toReplace = "0123456789-";
        private int _multiplyFactor = 10;

        public ScientificNumericUpDown()
        {
            InitializeComponent();

            
            Minimum = decimal.MinValue/10;
            Maximum = decimal.MaxValue/10;
          

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


        public decimal Epsilon { get; set; } = 0.001m;

        private decimal ToInsideRange(double value)
        {
            if (value > (double)Maximum)
                return Maximum;
            if (value < (double)Minimum)
                return Minimum;
            return (decimal)value;
        }

        public new decimal Value
        {
            get { return base.Value; }
            set
            {
               base.Value = ToInsideRange((double)value);
            }
        }

        public new Font Font
        {
            get { return base.Font; }
            set { base.Font = CustomFonts.GetMathFont(value.Size); }
        }

        public bool ExponentialMode => ((double) (Value)).ToString(CultureInfo.InvariantCulture).Contains('E') ||
                                       ((double) (Value)).ToString(CultureInfo.InvariantCulture).Contains('e');

        /*  private void Control_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '*')
            {
                e.KeyChar = dotSymbol;


                NumericUpDown numericUpDownsender = (sender as NumericUpDown);

                TextBoxBase txtBase = numericUpDownsender.Controls[1] as TextBoxBase;
                int currentCaretPosition = txtBase.SelectionStart;
              //  numericUpDownsender.DataBindings[0].WriteValue();

                //numericUpDownsender.Text.Insert(currentCaretPosition, new string(dotSymbol,1));
                txtBase.Text.Insert(currentCaretPosition, new string(dotSymbol, 1));
                txtBase.
                txtBase.SelectionStart = currentCaretPosition+1;
            }
        }*/


        private bool IsCaretInExponent()
        {
            for (int i = CaretPosition - 1; i >=3; i--)
            {
                if (exponents.Contains(Text[i]))
                    continue;
                return Text[i] == '0' &&
                       Text[i - 1] == '1' &&
                       Text[i - 2] == SpecialSymbols.DotSymbol;
            }

            return false;
        }


        private int CaretPosition => (Controls[1] as TextBoxBase).SelectionStart;



        //     private Regex properExpressionRegex = new Regex(@"\-?(\d+\.?\d*)[eE]\d+");

        protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
        {
            if (IsCaretInExponent())
            {
                var expKeyChar = CovertToExponent(e.KeyChar);
                if (expKeyChar != ' ' && !(Text.Contains('⁻') && expKeyChar== '⁻'))
                    e.KeyChar = expKeyChar;
                else if(!char.IsControl(e.KeyChar))
                    e.Handled = true;
            }
            else if (e.KeyChar == '-' && ! ((IsCharOnLeftOfCaret('E')|| IsCharOnLeftOfCaret('e')) && !IsCharOnRightOfCaret('-')) &&!(CaretPosition==0 && !Text.StartsWith("-")))
                e.Handled = true;

            else if (e.KeyChar == '*' && !Text.Contains(SpecialSymbols.DotSymbol))
                e.KeyChar = SpecialSymbols.DotSymbol;
            else if (e.KeyChar == 'E' || e.KeyChar == 'e')
            {
               // Text += dotSymbol + "10";
               // e.Handled = true;
            }
           else if (e.KeyChar == ',' || e.KeyChar == '.' || e.KeyChar== Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0] || e.KeyChar == Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator[0])
            {
                e.KeyChar = '.';
               // //Text += '.';
               // //e.Handled = true;
            }
            else
                base.OnTextBoxKeyPress(source, e);
        }

        private bool IsCharOnRightOfCaret(char v)
        {
            if (CaretPosition > Text.Length-1)
                return false;

            return Text[CaretPosition] == v;
        }

        private bool IsCharOnLeftOfCaret(char c)
        {
            if (CaretPosition < 1|| Text.Length==0)
                return false;

            return Text[CaretPosition-1]==c;
        }

        protected override void ValidateEditText()//basically it sets Value after Text was edited
        {
           // try
            {
                if (Text.Contains('E') || Text.Contains('e'))
                {
                    Value = CovertFromEngineeringToValue(Text);
                }
                else if (Text.Contains(SpecialSymbols.DotSymbol))
                {
                    var parts1 = Text.Split(SpecialSymbols.DotSymbol);
                    if (parts1.Length == 2)
                    {
                        if (parts1[1].Any(c => exponents.Contains(c)))
                            Value = decimal.Parse(parts1[0], CultureInfo.InvariantCulture)*
                                    CovertFromScientificToValue(parts1[1]);
                        else
                            Value = decimal.Parse(parts1[0], CultureInfo.InvariantCulture)*
                                    decimal.Parse(parts1[1], CultureInfo.InvariantCulture);
                    }
                    else if (parts1.Length == 1 && parts1[0].Any(c => exponents.Contains(c)))
                    {
                        Value = CovertFromScientificToValue(parts1[0]);
                    }
                }
                else
                {
                    Value = decimal.Parse(Text, CultureInfo.InvariantCulture);
                }
            }
          //  catch (Exception ex)
            {
         //       base.ValidateEditText();
            }
            base.ValidateEditText();
        }

        public override void UpButton()
        {
            if (!ExponentialMode)
            {
                base.UpButton();
                Value = Value.RoundToSignificantDigits(2);//beware it's kind of experimental, 1 instead of two would give generally better results but wight have stopped progres in some cases like 0.001
            }
            else
            {
                    if(Value>0)
                    Value = ToInsideRange((double)Value * _multiplyFactor);
                    else if (Value < 0)
                        Value = ToInsideRange((double)Value / _multiplyFactor);

                    //UpdateEditText();
            }
        }

        public override void DownButton()
        {
            if (!ExponentialMode)
            {
                base.DownButton();
                Value = Value.RoundToSignificantDigits(2);//beware it's kind of experimental, 1 instead of two would give generally better results but wight have stopped progres in some cases like 0.001
            }
            else
            {
                    if (Value > 0)
                        Value = ToInsideRange((double)Value /_multiplyFactor);
                    else if (Value < 0)
                        Value = ToInsideRange((double)Value * _multiplyFactor);
                    //UpdateEditText();
            }
        }


        protected override void UpdateEditText()//basically it sets Text after Value was established
        {
            Text = ((double)Value).ToMathString();
            //var str = ((double) Value).ToString(CultureInfo.InvariantCulture);
            /*if (!ExponentialMode)
                Text = Value.ToString(CultureInfo.InvariantCulture);
            else
            {
                var parts = Value.ToString("E",CultureInfo.InvariantCulture).Split('E');

                var significand = decimal.Parse(parts[0], CultureInfo.InvariantCulture);
                var exponent = decimal.Parse(parts[1], CultureInfo.InvariantCulture);

                if (significand != 1.0m)
                    Text = significand.ToString(CultureInfo.InvariantCulture) + dotSymbol + "10" + CovertToExponent(exponent.ToString(CultureInfo.InvariantCulture));
                else
                    Text = "10" + CovertToExponent(exponent.ToString(CultureInfo.InvariantCulture));
            }*/
        }

        private string CovertToExponent(string v)
        {
            var sb = new StringBuilder(v);

            for (var i = 0; i < sb.Length; i++)
                for (var j = 0; j < exponents.Length; j++)
                    if (sb[i] == toReplace[j])
                        sb[i] = exponents[j];
            return sb.ToString();
        }

        private char CovertToExponent(char v)
        {
           // var sb = new StringBuilder(v);

            //for (var i = 0; i < sb.Length; i++)
                for (var j = 0; j < exponents.Length; j++)
                    if (v == toReplace[j])
                        return exponents[j];
            return ' ';
        }

        private decimal CovertFromScientificToValue(string v)
        {
            var sb = new StringBuilder(v);

            if (sb[0] == '1' && sb[1] == '0')
            {
                sb[1] = 'E';
            }

            for (var i = 0; i < sb.Length; i++)
                for (var j = 0; j < exponents.Length; j++)
                    if (sb[i] == exponents[j])
                        sb[i] = toReplace[j];

            return (decimal)double.Parse(sb.ToString(), CultureInfo.InvariantCulture);//maybe decimal should parse this
        }


        private decimal CovertFromEngineeringToValue(string v)
        {
            var val = double.Parse(v, CultureInfo.InvariantCulture);

            

            return ToInsideRange(val);//maybe decimal should parse this
        }
    }
}