using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class RedoCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public RedoCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            this.Text = MenuStrings.redoToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.redoToolStripMenuItem_Text;
            this.ShortcutKeyString = "Ctrl+Y";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int)SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^Y");
                //expressionTextBox.do()
            }
            else if ((int)SharedViewState.Instance.CurrentView == 4)
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