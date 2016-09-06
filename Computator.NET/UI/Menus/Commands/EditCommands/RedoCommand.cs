using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    internal class RedoCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly IMainForm mainFormView;
        private readonly ICanFileEdit scriptingCodeEditor;
        private ISharedViewState _sharedViewState;

        public RedoCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor,
            IMainForm mainFormView, ISharedViewState sharedViewState)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            Text = MenuStrings.redoToolStripMenuItem_Text;
            ToolTip = MenuStrings.redoToolStripMenuItem_Text;
            ShortcutKeyString = "Ctrl+Y";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            _sharedViewState = sharedViewState;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int) _sharedViewState.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^Y");
                //expressionTextBox.do()
            }
            else if ((int) _sharedViewState.CurrentView == 4)
                //scriptingCodeEditor.Focus();
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Redo();
                else
                    mainFormView.SendStringAsKey("^Y");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Redo();
                else
                    mainFormView.SendStringAsKey("^Y");
            }

            mainFormView.SendStringAsKey("^Y");
        }
    }
}