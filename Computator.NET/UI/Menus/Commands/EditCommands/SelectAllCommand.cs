using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    internal class SelectAllCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly IMainForm mainFormView;
        private readonly ICanFileEdit scriptingCodeEditor;
        private ISharedViewState _sharedViewState;

        public SelectAllCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor,
            IMainForm mainFormView, ISharedViewState sharedViewState)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            Text = MenuStrings.selectAllToolStripMenuItem_Text;
            ToolTip = MenuStrings.selectAllToolStripMenuItem_Text;
            ShortcutKeyString = "Ctrl+A";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            _sharedViewState = sharedViewState;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int) _sharedViewState.CurrentView < 4)
            {
                mainFormView.SendStringAsKey("^A"); //expressionTextBox.SelectAll();
            }
            else if ((int) _sharedViewState.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.SelectAll();
                else
                    mainFormView.SendStringAsKey("^A");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.SelectAll();
                //  else
                mainFormView.SendStringAsKey("^A");
            }

            mainFormView.SendStringAsKey("^A");
        }
    }
}