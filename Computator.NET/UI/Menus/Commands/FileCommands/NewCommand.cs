using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class NewCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;


        public NewCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            this.Icon = Resources.newToolStripButtonImage;
            this.Text = MenuStrings.newToolStripButton_Text;
            this.ToolTip = MenuStrings.newToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.ShortcutKeyString = "Ctrl+N";
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