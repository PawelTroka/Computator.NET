using Computator.NET.Core.UI.Menus;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.EditCommands
{
    public class SelectAllCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;
        private ISharedViewState _sharedViewState;
        private IApplicationManager _applicationManager;

        public SelectAllCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, ISharedViewState sharedViewState, IApplicationManager applicationManager)
        {
            //this.Icon = Resources.copyToolStripButtonImage;
            Text = MenuStrings.selectAllToolStripMenuItem_Text;
            ToolTip = MenuStrings.selectAllToolStripMenuItem_Text;
            ShortcutKeyString = "Ctrl+A";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            _sharedViewState = sharedViewState;
            _applicationManager = applicationManager;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            if ((int) _sharedViewState.CurrentView < 4)
            {
                _applicationManager.SendStringAsKey("^A"); //expressionTextBox.SelectAll();
            }
            else if ((int) _sharedViewState.CurrentView == 4)
            {
                if (scriptingCodeEditor.Focused)
                    scriptingCodeEditor.SelectAll();
                else
                    _applicationManager.SendStringAsKey("^A");
            }
            else
            {
                if (customFunctionsCodeEditor.Focused)
                    customFunctionsCodeEditor.SelectAll();
                //  else
                _applicationManager.SendStringAsKey("^A");
            }

            _applicationManager.SendStringAsKey("^A");
        }
    }
}