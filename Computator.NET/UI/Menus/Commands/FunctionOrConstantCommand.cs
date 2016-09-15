using System.Windows;
using System.Windows.Forms;
using Computator.NET.Data;
using Computator.NET.UI.Controls.AutocompleteMenu;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands
{
    internal class FunctionOrConstantCommand : CommandBase
    {
        private readonly IScriptProvider _customFunctionsTextProvider;
        private readonly ITextProvider _expressionTextProvider;
        private readonly IScriptProvider _scriptingTextProvider;
        private readonly ISharedViewState _sharedViewState;
        public FunctionOrConstantCommand(string text, string toolTip, ITextProvider expressionTextProvider,
            IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails)
        {
            Text = text;
            ToolTip = toolTip;
            _expressionTextProvider = expressionTextProvider;
            _scriptingTextProvider = scriptingTextProvider;
            _customFunctionsTextProvider = customFunctionsTextProvider;
            _sharedViewState = sharedViewState;
            this._functionsDetails = functionsDetails;
        }

        private readonly IFunctionsDetails _functionsDetails;
        public override void Execute()
        {
            if (SystemInformation.MouseButtonsSwapped || SystemParameters.SwapButtons)
                //TODO: somehow get which mouse button invoked this command
            {
                if (_functionsDetails.ContainsKey(Text))
                {
                    WebBrowserForm.Show(_functionsDetails[Text]);
                    // menuFunctionsToolTip.SetFunctionInfo(FunctionsDetails.Details[this.Text]);
                    //menuFunctionsToolTip.Show(this, menuItem.Width + 3, 0);
                    // menuFunctionsToolTip.Show();
                }
            }
            else
            {
                if ((int) _sharedViewState.CurrentView < 4)

                    _expressionTextProvider.Text += Text;
                else if ((int) _sharedViewState.CurrentView == 4)
                {
                    _scriptingTextProvider.Text += Text;
                }
                else if ((int) _sharedViewState.CurrentView == 5)


                    _customFunctionsTextProvider.Text += Text;
            }
        }
    }
}