using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    internal class CopyCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly IMainForm mainFormView;
        private readonly ICanFileEdit scriptingCodeEditor;

        public CopyCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor,
            IMainForm mainFormView)
        {
            Icon = Resources.copyToolStripButtonImage;
            Text = MenuStrings.copyToolStripButton_Text;
            ToolTip = MenuStrings.copyToolStripButton_Text;
            ShortcutKeyString = "Ctrl+C";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int) SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^C"); //expressionTextBox.Copy();
            }
            else if ((int) SharedViewState.Instance.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.Copy();
                else
                    mainFormView.SendStringAsKey("^C");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.Copy();
                else
                    mainFormView.SendStringAsKey("^C");
            }
        }
    }
}