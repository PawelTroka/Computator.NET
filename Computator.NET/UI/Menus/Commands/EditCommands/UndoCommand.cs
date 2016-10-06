using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Models;
using Computator.NET.UI.Views;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    public class UndoCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;
        private ISharedViewState _sharedViewState;
        private IApplicationManager _applicationManager;

        public UndoCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, ISharedViewState sharedViewState, IApplicationManager applicationManager)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            Text = MenuStrings.undoToolStripMenuItem_Text;
            ToolTip = MenuStrings.undoToolStripMenuItem_Text;
            ShortcutKeyString = "Ctrl+Z";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            _sharedViewState = sharedViewState;
            _applicationManager = applicationManager;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int) _sharedViewState.CurrentView < 4)
                _applicationManager.SendStringAsKey("^Z"); //expressionTextBox.Undo();
            else if ((int) _sharedViewState.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Undo();
                else
                    _applicationManager.SendStringAsKey("^Z");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Undo();
                else
                    _applicationManager.SendStringAsKey("^Z");
            }

            _applicationManager.SendStringAsKey("^Z");
        }
    }
}