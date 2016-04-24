using System.Windows;
using System.Windows.Forms;
using Computator.NET.Data;
using Computator.NET.DataTypes;
using Computator.NET.UI.AutocompleteMenu;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class FunctionOrConstantCommand : CommandBase
    {
        private readonly ITextProvider _scriptingTextProvider;
        private readonly ITextProvider _customFunctionsTextProvider;
        private readonly ITextProvider _expressionTextProvider;
        public FunctionOrConstantCommand(string text, string toolTip,ITextProvider expressionTextProvider, ITextProvider scriptingTextProvider, ITextProvider customFunctionsTextProvider)
        {
            this.Text = text;
            this.ToolTip = toolTip;
            this._expressionTextProvider = expressionTextProvider;
            this._scriptingTextProvider = scriptingTextProvider;
            this._customFunctionsTextProvider = customFunctionsTextProvider;

        }


        public override void Execute()
        {
            if (SystemInformation.MouseButtonsSwapped || SystemParameters.SwapButtons)//TODO: somehow get which mouse button invoked this command
            {
                if (FunctionsDetails.Details.ContainsKey(this.Text))
                {
                    WebBrowserForm.Show((FunctionsDetails.Details[this.Text]));
                    // menuFunctionsToolTip.SetFunctionInfo(FunctionsDetails.Details[this.Text]);
                    //menuFunctionsToolTip.Show(this, menuItem.Width + 3, 0);
                    // menuFunctionsToolTip.Show();
                }
            }
            else
            {
                if ((int) SharedViewState.Instance.CurrentView < 4)

                    _expressionTextProvider.Text += (this.Text);
                else if ((int) SharedViewState.Instance.CurrentView == 4)
                {
                    _scriptingTextProvider.Text += (this.Text);
                }
                else if ((int) SharedViewState.Instance.CurrentView == 5)


                    _customFunctionsTextProvider.Text += (this.Text);
            }
        }
    }
}