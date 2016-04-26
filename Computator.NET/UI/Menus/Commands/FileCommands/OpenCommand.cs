using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Menus.Commands.FileCommands
{
    internal class OpenCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;


        public OpenCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            Icon = Resources.openToolStripButtonImage;
            Text = MenuStrings.openToolStripButton_Text;
            ToolTip = MenuStrings.openToolStripButton_Text;
            ShortcutKeyString = "Ctrl+O";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            var ofd = new OpenFileDialog {Filter = GlobalConfig.tslFilesFIlter};
                //TODO: move this to mainView or something
            if (ofd.ShowDialog() != DialogResult.OK)
                return;


            switch ((int) SharedViewState.Instance.CurrentView)
            {
                case 0:

                    //mainFormView.SendStringAsKey("^S");
                    break;

                case 4:
                    scriptingCodeEditor.NewDocument(ofd.FileName);
                    break;

                case 5:
                    customFunctionsCodeEditor.NewDocument(ofd.FileName);
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }

            //mainFormView.SendStringAsKey("^S");
        }
    }
}