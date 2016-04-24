using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class SaveCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;


        public SaveCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            this.ShortcutKeyString = "Ctrl+S";
            this.Icon = Resources.saveToolStripButtonImage;
            this.Text = MenuStrings.saveToolStripButton_Text;
            this.ToolTip = MenuStrings.saveToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {

            switch ((int)SharedViewState.Instance.CurrentView)
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