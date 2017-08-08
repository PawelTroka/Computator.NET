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
        private readonly AutocompleteItem _autocompleteItem;
        private readonly IShowFunctionDetails _showFunctionDetails;

        public FunctionOrConstantCommand(AutocompleteItem autocompleteItem, ITextProvider expressionTextProvider,
            IScriptProvider scriptingTextProvider, IScriptProvider customFunctionsTextProvider, ISharedViewState sharedViewState, IClickedMouseButtonsProvider clickedMouseButtonsProvider, IShowFunctionDetails showFunctionDetails)
        {

            this._autocompleteItem = autocompleteItem;

            //TODO: do we really want to show signature as text and title as tooltip?
            Text = _autocompleteItem.Details.Signature;
            ToolTip = _autocompleteItem.Details.Title;

            _expressionTextProvider = expressionTextProvider;
            _scriptingTextProvider = scriptingTextProvider;
            _customFunctionsTextProvider = customFunctionsTextProvider;
            _sharedViewState = sharedViewState;
            _clickedMouseButtonsProvider = clickedMouseButtonsProvider;
            _showFunctionDetails = showFunctionDetails;
        }
        
        public override void Execute()
        {
            if (_clickedMouseButtonsProvider.ClickedMouseButtons == MouseButtons.Right)
            {
                if (!_autocompleteItem.Details.IsNullOrEmpty())
                {
                    _showFunctionDetails.Show(_autocompleteItem.Details);
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