using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    internal class PasteCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly IMainForm mainFormView;
        private readonly ICanFileEdit scriptingCodeEditor;

        public PasteCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor,
            IMainForm mainFormView)
        {
            ShortcutKeyString = "Ctrl+V";
            Icon = Resources.pasteToolStripButtonImage;
            Text = MenuStrings.pasteToolStripButton_Text;
            ToolTip = MenuStrings.pasteToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int) SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^V"); //expressionTextBox.Paste();
            }
            else if ((int) SharedViewState.Instance.CurrentView == 4)
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