using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;
using Computator.NET.UI.Models;
using Computator.NET.UI.Views;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    public class CopyCommand : CommandBase
    {
        private readonly ICanFileEdit _customFunctionsCodeEditor;
        private readonly ICanFileEdit _scriptingCodeEditor;
        private readonly ISharedViewState _sharedViewState;
        private readonly IApplicationManager _applicationManager;
        public CopyCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor,
             ISharedViewState sharedViewState, IApplicationManager applicationManager)
        {
            Icon = Resources.copyToolStripButtonImage;
            Text = MenuStrings.copyToolStripButton_Text;
            ToolTip = MenuStrings.copyToolStripButton_Text;
            ShortcutKeyString = "Ctrl+C";
            this._scriptingCodeEditor = scriptingCodeEditor;
            this._customFunctionsCodeEditor = customFunctionsCodeEditor;
            _sharedViewState = sharedViewState;
            _applicationManager = applicationManager;
        }


        public override void Execute()
        {
            if ((int) _sharedViewState.CurrentView < 4)
            {
                _applicationManager.SendStringAsKey("^C"); //expressionTextBox.Copy();
            }
            else if ((int) _sharedViewState.CurrentView == 4)
            {
                if (_scriptingCodeEditor.Focused)
                    _scriptingCodeEditor.Copy();
                else
                    _applicationManager.SendStringAsKey("^C");
            }
            else
            {
                if (_customFunctionsCodeEditor.Focused)
                    _customFunctionsCodeEditor.Copy();
                else
                    _applicationManager.SendStringAsKey("^C");
            }
        }
    }
}