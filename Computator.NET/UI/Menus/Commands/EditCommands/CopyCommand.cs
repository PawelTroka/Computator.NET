using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class CopyCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;
        private IMainForm mainFormView;

        public CopyCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            this.Icon = Resources.copyToolStripButtonImage;
            this.Text = MenuStrings.copyToolStripButton_Text;
            this.ToolTip = MenuStrings.copyToolStripButton_Text;
            this.ShortcutKeyString = "Ctrl+C";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            if ((int)SharedViewState.Instance.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^C"); //expressionTextBox.Copy();
            }
            else if ((int)SharedViewState.Instance.CurrentView == 4)
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