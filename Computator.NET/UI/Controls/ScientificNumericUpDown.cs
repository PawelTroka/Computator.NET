using Enumerable = System.Linq.Enumerable;

namespace Computator.NET.UI
{
    public partial class ScientificNumericUpDown : System.Windows.Forms.NumericUpDown
    {
        private readonly char dotSymbol = '·'; //'⋅'
        private readonly string exponents = "⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻⁼⁽⁾";
        private readonly string toReplace = "0123456789+-=()";

        public ScientificNumericUpDown()
        {
            InitializeComponent();
            Minimum = decimal.MinValue;
            Maximum = decimal.MaxValue;
            Value = (decimal) 1e-9;
            ExponentialMode = true;


            Font = new System.Drawing.Font("Cambria", 16.2F, System.Drawing.GraphicsUnit.Point);
                //GlobalConfig.mathFont;

            TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
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

        protected override void OnTextBoxKeyPress(object source, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == '*' && !Enumerable.Contains(Text, dotSymbol))
                e.KeyChar = dotSymbol;
            else
                base.OnTextBoxKeyPress(source, e);
        }

        protected override void ValidateEditText()
        {
            try
            {
                var parts1 = Text.Split(dotSymbol);
                if (parts1.Length == 2)
                {
                    if (Enumerable.Any(parts1[1], c => Enumerable.Contains(exponents, c)))
                        Value = (decimal) (double.Parse(parts1[0])*covertFromScientificToValue(parts1[1]));
                    else
                        Value = (decimal) (double.Parse(parts1[0])*double.Parse(parts1[1]));
                }
                else if (parts1.Length == 1 && Enumerable.Any(parts1[0], c => Enumerable.Contains(exponents, c)))
                {
                    var convertedValue = covertFromScientificToValue(parts1[0]);
                    Value = (decimal) (convertedValue);
                }
            }
            catch (System.Exception ex)
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

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        protected override void UpdateEditText()
        {
            if ((System.Math.Abs(Value) < 10000 && (double) System.Math.Abs(Value) >= 0.001) || Value == 0)
                Text = Value.ToString();
            else
            {
                var parts = Value.ToString("E").Split('E');

                var significand = double.Parse(parts[0]);
                var exponent = double.Parse(parts[1]);

                if (significand != 1.0)
                    Text = significand.ToString() + dotSymbol + "10" + covertToExponent(exponent.ToString());
                else
                    Text = "10" + covertToExponent(exponent.ToString());
            }
        }

        private string covertToExponent(string v)
        {
            var sb = new System.Text.StringBuilder(v);

            for (var i = 0; i < sb.Length; i++)
                for (var j = 0; j < exponents.Length; j++)
                    if (sb[i] == toReplace[j])
                        sb[i] = exponents[j];
            return sb.ToString();
        }

        private double covertFromScientificToValue(string v)
        {
            var sb = new System.Text.StringBuilder(v);

            if (sb[0] == '1' && sb[1] == '0')
            {
                sb[1] = 'e';
            }

            for (var i = 0; i < sb.Length; i++)
                for (var j = 0; j < exponents.Length; j++)
                    if (sb[i] == exponents[j])
                        sb[i] = toReplace[j];

            return double.Parse(sb.ToString());
        }
    }
}