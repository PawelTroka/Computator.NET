using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Menus.Commands.FileCommands
{
    internal class SaveCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;


        public SaveCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            ShortcutKeyString = "Ctrl+S";
            Icon = Resources.saveToolStripButtonImage;
            Text = MenuStrings.saveToolStripButton_Text;
            ToolTip = MenuStrings.saveToolStripButton_Text;

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
                    scriptingCodeEditor.Save();
                    break;

                case 5:
                    customFunctionsCodeEditor.Save();
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }

            // mainFormView.SendStringAsKey("^S");
        }
    }
}