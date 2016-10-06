using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.FileCommands
{
    public class SaveAsCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;
        private ISharedViewState _sharedViewState;

        public SaveAsCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, ISharedViewState sharedViewState)
        {
            //  this.Icon = Resources.save;
            Text = MenuStrings.saveAsToolStripMenuItem_Text;
            ToolTip = MenuStrings.saveAsToolStripMenuItem_Text;

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