using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Menus.Commands.FileCommands
{
    internal class PrintPreviewCommand : CommandBase
    {
        private ICanFileEdit customFunctionsCodeEditor;
        private ISharedViewState _sharedViewState;
        private readonly IMainForm mainFormView;
        private ICanFileEdit scriptingCodeEditor;

        public PrintPreviewCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor,
            IMainForm mainFormView, ISharedViewState sharedViewState)
        {
            Icon = Resources.printPreviewToolStripMenuItemImage;
            Text = MenuStrings.printPreviewToolStripMenuItem_Text;
            ToolTip = MenuStrings.printPreviewToolStripMenuItem_Text;

            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
            _sharedViewState = sharedViewState;
        }


        public override void Execute()
        {
            switch ((int) _sharedViewState.CurrentView)
            {
                case 0:
                    //if (_calculationsMode == CalculationsMode.Real)
                    mainFormView.ChartingView.Charts[_sharedViewState.CalculationsMode].PrintPreview();
                    // else
                    //  SendStringAsKey("^P");
                    break;

                case 4:
                    //scriptingCodeEditor();

                    break;

                case 5:
                    //this.customFunctionsCodeEditor
                    break;

                default:
                    mainFormView.SendStringAsKey("^P"); //this.chart2d.Printing.PrintPreview();
                    break;
            }
        }
    }
}