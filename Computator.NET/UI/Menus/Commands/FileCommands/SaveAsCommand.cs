using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Menus.Commands.FileCommands
{
    internal class SaveAsCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;


        public SaveAsCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            //  this.Icon = Resources.save;
            Text = MenuStrings.saveAsToolStripMenuItem_Text;
            ToolTip = MenuStrings.saveAsToolStripMenuItem_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            switch ((int) SharedViewState.Instance.CurrentView)
            {
                case 0:

                    //mainFormView.SendStringAsKey("^S");
                    break;

                case 4:
                    scriptingCodeEditor.SaveAs();
                    break;

                case 5:
                    customFunctionsCodeEditor.SaveAs();
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }
        }
    }
}