using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class UndoCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public UndoCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            this.Text = MenuStrings.undoToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.undoToolStripMenuItem_Text;
            this.ShortcutKeyString = "Ctrl+Z";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int)SharedViewState.Instance.CurrentView < 4)
                mainFormView.SendStringAsKey("^Z"); //expressionTextBox.Undo();
            else if ((int)SharedViewState.Instance.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Undo();
                else
                    mainFormView.SendStringAsKey("^Z");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Undo();
                else
                    mainFormView.SendStringAsKey("^Z");
            }

            mainFormView.SendStringAsKey("^Z");
        }
    }
}