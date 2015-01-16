using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Computator.NET.UI
{
    public partial class ScientificNumericUpDown : NumericUpDown
    {
        private char dotSymbol = '·'; //'⋅'
        private string exponents = "⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻⁼⁽⁾";
        private string toReplace = "0123456789+-=()";


        public ScientificNumericUpDown()
        {
            InitializeComponent();
            Minimum = decimal.MinValue;
            Maximum = decimal.MaxValue;
            Value = (decimal) 1e-9;
            ExponentialMode = true;


            Font = new Font("Cambria", 16.2F, GraphicsUnit.Point); //GlobalConfig.mathFont;

            TextAlign = HorizontalAlignment.Center;
            //this.KeyPress += Control_KeyPress;
        }

        public bool ExponentialMode { get; set; }

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

        protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
        {
            if (e.KeyChar == '*' && !Text.Contains(dotSymbol))
                e.KeyChar = dotSymbol;
            else
                base.OnTextBoxKeyPress(source, e);
        }

        protected override void ValidateEditText()
        {
            try
            {
                string[] parts1 = Text.Split(dotSymbol);
                if (parts1.Length == 2)
                {
                    if (parts1[1].Any(c => exponents.Contains(c)))
                        Value = (decimal) (double.Parse(parts1[0])*covertFromScientificToValue(parts1[1]));
                    else
                        Value = (decimal) (double.Parse(parts1[0])*double.Parse(parts1[1]));
                }
                else if (parts1.Length == 1 && parts1[0].Any(c => exponents.Contains(c)))
                {
                    double convertedValue = covertFromScientificToValue(parts1[0]);
                    Value = (decimal) (convertedValue);
                }
            }
            catch (Exception ex)
            {
                base.ValidateEditText();
            }
            base.ValidateEditText();
        }

        public override void UpButton()
        {
            if (!ExponentialMode)
                base.UpButton();
            else
            {
                if (Value*10 <= Maximum)
                {
                    Value = Value*10;
                    UpdateEditText();
                }
            }
        }

        public override void DownButton()
        {
            if (!ExponentialMode)
                base.DownButton();
            else
            {
                if (Value/10 >= Minimum)
                {
                    Value = Value/10;
                    UpdateEditText();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        protected override void UpdateEditText()
        {
            if ((Math.Abs(Value) < 10000 && (double) Math.Abs(Value) >= 0.001) || Value == 0)
                Text = Value.ToString();
            else
            {
                string[] parts = Value.ToString("E").Split('E');

                double significand = double.Parse(parts[0]);
                double exponent = double.Parse(parts[1]);

                if (significand != 1.0)
                    Text = significand.ToString() + dotSymbol + "10" + covertToExponent(exponent.ToString());
                else
                    Text = "10" + covertToExponent(exponent.ToString());
            }
        }

        private string covertToExponent(string v)
        {
            var sb = new StringBuilder(v);

            for (int i = 0; i < sb.Length; i++)
                for (int j = 0; j < exponents.Length; j++)
                    if (sb[i] == toReplace[j])
                        sb[i] = exponents[j];
            return sb.ToString();
        }


        private double covertFromScientificToValue(string v)
        {
            var sb = new StringBuilder(v);

            if (sb[0] == '1' && sb[1] == '0')
            {
                sb[1] = 'e';
            }

            for (int i = 0; i < sb.Length; i++)
                for (int j = 0; j < exponents.Length; j++)
                    if (sb[i] == exponents[j])
                        sb[i] = toReplace[j];

            return double.Parse(sb.ToString());
        }
    }
}