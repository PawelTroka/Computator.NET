using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class CutCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public CutCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            this.Icon = Resources.cutToolStripButtonImage;
            this.Text = MenuStrings.cutToolStripButton_Text;
            this.ToolTip = MenuStrings.cutToolStripButton_Text;
            this.ShortcutKeyString = "Ctrl+X";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 4:
                    if (scriptingCodeEditor.Focused)
                        scriptingCodeEditor.Cut();
                    else
                        mainFormView.SendStringAsKey("^X");
                    break;

                case 5:
                    if (customFunctionsCodeEditor.Focused)
                        customFunctionsCodeEditor.Cut();
                    else
                        mainFormView.SendStringAsKey("^X");
                    break;

                default: //if ((int)SharedViewState.Instance.CurrentView < 4)
                    mainFormView.SendStringAsKey("^X"); //expressionTextBox.Cut();
                    break;
            }
        }
    }
}