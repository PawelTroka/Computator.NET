using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Models;
using Computator.NET.UI.Views;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    public class CutCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;
        private ISharedViewState _sharedViewState;
        private readonly IApplicationManager _applicationManager;

        public CutCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, ISharedViewState sharedViewState, IApplicationManager applicationManager)
        {
            Icon = Resources.cutToolStripButtonImage;
            Text = MenuStrings.cutToolStripButton_Text;
            ToolTip = MenuStrings.cutToolStripButton_Text;
            ShortcutKeyString = "Ctrl+X";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            _sharedViewState = sharedViewState;
            _applicationManager = applicationManager;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            switch ((int) _sharedViewState.CurrentView)
            {
                case 4:
                    if (scriptingCodeEditor.Focused)
                        scriptingCodeEditor.Cut();
                    else
                        _applicationManager.SendStringAsKey("^X");
                    break;

                case 5:
                    if (customFunctionsCodeEditor.Focused)
                        customFunctionsCodeEditor.Cut();
                    else
                        _applicationManager.SendStringAsKey("^X");
                    break;

                default: //if ((int)_sharedViewState.CurrentView < 4)
                    _applicationManager.SendStringAsKey("^X"); //expressionTextBox.Cut();
                    break;
            }
        }
    }
}