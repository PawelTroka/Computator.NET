using System.Windows.Forms;

namespace Computator.NET.NumericalCalculations
{
    internal class NumericalCalculation
    {
        public static void setupOperations(ComboBox operationsComboBox)
        {
            operationsComboBox.Items.Clear();
            operationsComboBox.Items.Add("Integral");
            operationsComboBox.Items.Add("Derivative");
            operationsComboBox.Items.Add("Function root");

            operationsComboBox.SelectedItem = "Integral";
        }

        public static void setupMethods(ComboBox methodsComboBox, ComboBox operationsComboBox)
        {
            methodsComboBox.Items.Clear();

            var operation = operationsComboBox.SelectedItem.ToString();

            switch (operation)
            {
                case "Integral":
                    methodsComboBox.Items.Add("trapezoidal method");
                    methodsComboBox.Items.Add("rectangle method");
                    methodsComboBox.Items.Add("Simpson's method");
                    methodsComboBox.Items.Add("double exponential transformation");
                    methodsComboBox.Items.Add("non-adaptive Gauss–Kronrod method");
                    methodsComboBox.Items.Add("infinity-adaptive Gauss–Kronrod method");
                    methodsComboBox.Items.Add("Monte Carlo method");
                    methodsComboBox.Items.Add("Romberg's method");
                    break;

                case "Derivative":
                    methodsComboBox.Items.Add("finite difference formula");
                    methodsComboBox.Items.Add("two-point finite difference formula");
                    methodsComboBox.Items.Add("stable finite difference formula");
                    methodsComboBox.Items.Add("centered five-point method");
                    break;

                case "Function root":
                    methodsComboBox.Items.Add("bisection method");
                    methodsComboBox.Items.Add("secant method");
                    methodsComboBox.Items.Add("Brent's method");
                    methodsComboBox.Items.Add("Broyden's method");
                    break;
            }
            methodsComboBox.SelectedItem = methodsComboBox.Items[methodsComboBox.Items.Count - 1];
        }

        public static void setupGroupBoxes(ComboBox operationsComboBox, GroupBox derrivative, GroupBox root,
            GroupBox integral)
        {
            var operation = operationsComboBox.SelectedItem.ToString();

            integral.Visible = root.Visible = derrivative.Visible = false;
            switch (operation)
            {
                case "Integral":
                    integral.Visible = true;
                    break;

                case "Derivative":
                    derrivative.Visible = true;
                    break;

                case "Function root":
                    root.Visible = true;
                    break;
            }
        }
    }
}