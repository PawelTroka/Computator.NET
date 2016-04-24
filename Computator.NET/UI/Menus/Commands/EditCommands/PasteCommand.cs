using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class PasteCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public PasteCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            this.ShortcutKeyString = "Ctrl+V";
            this.Icon = Resources.pasteToolStripButtonImage;
            this.Text = MenuStrings.pasteToolStripButton_Text;
            this.ToolTip = MenuStrings.pasteToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            if ((int)SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^V"); //expressionTextBox.Paste();
            }
            else if ((int)SharedViewState.Instance.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Paste();
                else
                    mainFormView.SendStringAsKey("^V");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Paste();
                else
                    mainFormView.SendStringAsKey("^V");
            }
        }
    }
}