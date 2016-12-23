using System.Windows;
using System.Windows.Forms;
using Computator.NET.Data;
using Computator.NET.UI.Controls.AutocompleteMenu;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands
{
    public class MouseButtonsProvider : IClickedMouseButtonsProvider
    {
        public MouseButtons ClickedMouseButtons => System.Windows.Forms.Control.MouseButtons;
    }
    public interface IClickedMouseButtonsProvider
    {
        MouseButtons ClickedMouseButtons { get; }
    }
    internal class FunctionOrConstantCommand : CommandBase
    {
        private readonly IScriptProvider _customFunctionsTextProvider;
        private readonly ITextProvider _expressionTextProvider;
        private readonly IScriptProvider _scriptingTextProvider;
        private readonly ISharedViewState _sharedViewState;
        private readonly IClickedMouseButtonsProvider _clickedMouseButtonsProvider;
        private IShowFuncrionDetails _showFuncrionDetails;
        public FunctionOrConstantCommand(string text, string toolTip, ITextProvider expressionTextProvider,
            IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails, IClickedMouseButtonsProvider clickedMouseButtonsProvider, IShowFuncrionDetails showFuncrionDetails)
        {
            Text = text;
            ToolTip = toolTip;
            _expressionTextProvider = expressionTextProvider;
            _scriptingTextProvider = scriptingTextProvider;
            _customFunctionsTextProvider = customFunctionsTextProvider;
            _sharedViewState = sharedViewState;
            this._functionsDetails = functionsDetails;
            _clickedMouseButtonsProvider = clickedMouseButtonsProvider;
            _showFuncrionDetails = showFuncrionDetails;
        }

        private readonly IFunctionsDetails _functionsDetails;
        public override void Execute()
        {
            if (_clickedMouseButtonsProvider.ClickedMouseButtons == MouseButtons.Right)
            {
                if (_functionsDetails.ContainsKey(Text))
                {
                    _showFuncrionDetails.Show(_functionsDetails[Text]);
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