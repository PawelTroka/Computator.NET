using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
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
        private readonly string exponents = "⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻⁼⁽⁾";
        private readonly string toReplace = "0123456789+-=()";
        private int _multiplyFactor = 10;

        public ScientificNumericUpDown()
        {
            InitializeComponent();

            
            Minimum = decimal.MinValue;
            Maximum = decimal.MaxValue;
          //  Value = (decimal) 1e-9;
            //ExponentialMode = true;


          //  Font = new Font("Cambria", 16.2F, GraphicsUnit.Point);

      //      Font = MathCustomFonts.GetMathFont(Font.Size);
            //GlobalConfig.mathFont;

            TextAlign = HorizontalAlignment.Center;
            //this.KeyPress += Control_KeyPress;
            ValueChanged += (o, e) =>
            {
                if (!ExponentialMode)
                    Increment = Math.Abs((0.3m * Value).RoundToSignificantDigits(1));
                if (Increment == 0)
                    Increment = 1;
            };
          //  Culture
            Font = base.Font;
        }


        public new decimal Value
        {
            get { return base.Value; }
            set
            {
                if (value > decimal.MaxValue/10)
                    base.Value = decimal.MaxValue/10;
                else if (value < decimal.MinValue/10)
                    base.Value = decimal.MinValue/10;
                else
                    base.Value = value;
            }
        }

        public new Font Font
        {
            get { return base.Font; }
            set { base.Font = MathCustomFonts.GetMathFont(value.Size); }
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
            TextBoxBase txtBase = this.Controls[1] as TextBoxBase;
            int currentCaretPosition = txtBase.SelectionStart;

           // var section = 0;

            for (int i = currentCaretPosition-1; i >=3; i--)
            {
                if (exponents.Contains(Text[i]))
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
                var expKeyChar = CovertToExponent(e.KeyChar);
                if (expKeyChar != ' ')
                    e.KeyChar = expKeyChar;
                else if(!char.IsControl(e.KeyChar))
                    e.Handled = true;
            }

            else if (e.KeyChar == '*' && !Enumerable.Contains(Text, SpecialSymbols.DotSymbol))
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
                        if (Enumerable.Any(parts1[1], c => Enumerable.Contains(exponents, c)))
                            Value = (decimal.Parse(parts1[0], CultureInfo.InvariantCulture)*
                                     CovertFromScientificToValue(parts1[1]));
                        else
                            Value = (decimal.Parse(parts1[0], CultureInfo.InvariantCulture)*
                                     decimal.Parse(parts1[1], CultureInfo.InvariantCulture));
                    }
                    else if (parts1.Length == 1 && Enumerable.Any(parts1[0], c => Enumerable.Contains(exponents, c)))
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
                if (Value*_multiplyFactor <= Maximum)
                {
                    if(Value>0)
                    Value = Value*_multiplyFactor;
                    else if (Value < 0)
                        Value = Value/_multiplyFactor;

                    UpdateEditText();
                }
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
                if (Value/_multiplyFactor >= Minimum)
                {
                    if (Value > 0)
                        Value = Value/_multiplyFactor;
                    else if (Value < 0)
                        Value = Value * _multiplyFactor;
                    UpdateEditText();
                }
            }
        }

       /* protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }*/

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
            return (decimal)double.Parse(v, CultureInfo.InvariantCulture);//maybe decimal should parse this
        }
    }
}