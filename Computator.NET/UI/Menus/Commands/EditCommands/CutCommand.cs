using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    internal class CutCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly IMainForm mainFormView;
        private readonly ICanFileEdit scriptingCodeEditor;

        public CutCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor,
            IMainForm mainFormView)
        {
            Icon = Resources.cutToolStripButtonImage;
            Text = MenuStrings.cutToolStripButton_Text;
            ToolTip = MenuStrings.cutToolStripButton_Text;
            ShortcutKeyString = "Ctrl+X";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            switch ((int) SharedViewState.Instance.CurrentView)
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