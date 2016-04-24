using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class PrintCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;

        private IMainForm mainFormView;

        public PrintCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, IMainForm mainFormView)
        {
            this.Icon = Resources.printToolStripButtonImage;
            this.Text = MenuStrings.printToolStripButton_Text;
            this.ToolTip = MenuStrings.printToolStripButton_Text;
            this.ShortcutKeyString = "Ctrl+P";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            switch ((int)SharedViewState.Instance.CurrentView)
            {
                case 0:
                    //if (_calculationsMode == CalculationsMode.Real)
                    mainFormView.ChartingView.Charts[SharedViewState.Instance.CalculationsMode].Print();
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