using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class SelectAllCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public SelectAllCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            this.Text = MenuStrings.selectAllToolStripMenuItem_Text;
            this.ToolTip = MenuStrings.selectAllToolStripMenuItem_Text;
            this.ShortcutKeyString = "Ctrl+A";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int)SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^A"); //expressionTextBox.SelectAll();
            }
            else if ((int)SharedViewState.Instance.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.SelectAll();
                else
                    mainFormView.SendStringAsKey("^A");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.SelectAll();
                //  else
                mainFormView.SendStringAsKey("^A");
            }

            mainFormView.SendStringAsKey("^A");
        }
    }
}