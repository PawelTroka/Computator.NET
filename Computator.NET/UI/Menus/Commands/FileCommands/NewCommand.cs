using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Menus.Commands.FileCommands
{
    internal class NewCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;


        public NewCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            Icon = Resources.newToolStripButtonImage;
            Text = MenuStrings.newToolStripButton_Text;
            ToolTip = MenuStrings.newToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            ShortcutKeyString = "Ctrl+N";
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
                    scriptingCodeEditor.NewDocument();
                    break;

                case 5:
                    customFunctionsCodeEditor.NewDocument();
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }

            //mainFormView.SendStringAsKey("^S");
        }
    }
}