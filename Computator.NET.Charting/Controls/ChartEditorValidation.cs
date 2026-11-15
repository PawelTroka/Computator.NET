using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Computator.NET.Localization;

namespace Computator.NET.Charting.Controls
{
    // Keep parsing separate from chart setters: rejecting one field must not
    // leave the chart with a mixture of old and newly entered settings.
    internal sealed class ChartEditorValidation
    {
        private readonly Form _dialog;
        private readonly ErrorProvider _errors;
        private TextBox _firstInvalid;

        internal ChartEditorValidation(Form dialog, IContainer components)
        {
            _dialog = dialog;
            _errors = new ErrorProvider(components)
            {
                ContainerControl = dialog,
                BlinkStyle = ErrorBlinkStyle.NeverBlink
            };
        }

        internal void Clear()
        {
            _errors.Clear();
            _firstInvalid = null;
        }

        internal double ReadNumber(TextBox input, bool allowAutomatic)
        {
            double value;
            if (!double.TryParse(input.Text, NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.CurrentCulture, out value) || double.IsInfinity(value) ||
                (!allowAutomatic && double.IsNaN(value)))
                Reject(input, Message(allowAutomatic ? "ChartNumberOrAutomatic" : "ChartFiniteNumber"));
            return value;
        }

        internal double ReadInterval(TextBox input)
        {
            var value = ReadNumber(input, true);
            if (value < 0)
                Reject(input, Message("ChartNonNegativeInterval"));
            return value;
        }

        internal void ValidateRange(TextBox minimumInput, double minimum, TextBox maximumInput, double maximum)
        {
            // NaN is the real chart's existing automatic-axis sentinel.
            if (!double.IsNaN(minimum) && !double.IsNaN(maximum) && minimum >= maximum)
            {
                Reject(minimumInput, Strings.Min_value_needs_to_be_less_than_Max_value);
                Reject(maximumInput, Strings.Min_value_needs_to_be_less_than_Max_value);
            }
        }

        internal bool CanApply()
        {
            if (_firstInvalid == null) return true;
            // The Save buttons have DialogResult.OK. Reset it so invalid input
            // cannot also dismiss a modal editor after its Click handler ends.
            _dialog.DialogResult = DialogResult.None;
            _firstInvalid.Focus();
            _firstInvalid.SelectAll();
            return false;
        }

        private void Reject(TextBox input, string message)
        {
            if (_firstInvalid == null) _firstInvalid = input;
            if (string.IsNullOrEmpty(_errors.GetError(input))) _errors.SetError(input, message);
        }

        private static string Message(string key) => Strings.ResourceManager.GetString(key, Strings.Culture);
    }
}
