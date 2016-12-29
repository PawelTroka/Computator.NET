using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.Core.Properties;

namespace Computator.NET.Core.Menu.Commands.FileCommands
{
    public class SaveCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;
        private ISharedViewState _sharedViewState;

        public SaveCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, ISharedViewState sharedViewState)
        {
            ShortcutKeyString = "Ctrl+S";
            Icon = Resources.saveToolStripButtonImage;
            Text = MenuStrings.saveToolStripButton_Text;
            ToolTip = MenuStrings.saveToolStripButton_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            _sharedViewState = sharedViewState;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            switch ((int) _sharedViewState.CurrentView)
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