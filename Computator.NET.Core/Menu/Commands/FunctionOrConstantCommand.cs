using System.Windows.Forms;
using Computator.NET.Core.Abstract;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Model;

namespace Computator.NET.Core.Menu.Commands
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
        private IShowFunctionDetails _showFunctionDetails;
        public FunctionOrConstantCommand(string text, string toolTip, ITextProvider expressionTextProvider,
            IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IFunctionsDetails functionsDetails, IClickedMouseButtonsProvider clickedMouseButtonsProvider, IShowFunctionDetails showFunctionDetails)
        {
            Text = text;
            ToolTip = toolTip;
            _expressionTextProvider = expressionTextProvider;
            _scriptingTextProvider = scriptingTextProvider;
            _customFunctionsTextProvider = customFunctionsTextProvider;
            _sharedViewState = sharedViewState;
            this._functionsDetails = functionsDetails;
            _clickedMouseButtonsProvider = clickedMouseButtonsProvider;
            _showFunctionDetails = showFunctionDetails;
        }

        private readonly IFunctionsDetails _functionsDetails;
        public override void Execute()
        {
            if (_clickedMouseButtonsProvider.ClickedMouseButtons == MouseButtons.Right)
            {
                if (_functionsDetails.ContainsKey(Text))
                {
                    _showFunctionDetails.Show(_functionsDetails[Text]);
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