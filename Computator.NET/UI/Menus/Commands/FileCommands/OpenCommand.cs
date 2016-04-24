using System.Windows.Forms;
using Computator.NET.Config;
using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;
using Computator.NET.UI.Menus;
using Computator.NET.UI.MVP;

namespace Computator.NET.UI.Commands
{
    class OpenCommand : CommandBase
    {
        private ICanFileEdit scriptingCodeEditor;
        private ICanFileEdit customFunctionsCodeEditor;


        public OpenCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor)
        {
            this.Icon = Resources.openToolStripButtonImage;
            this.Text = MenuStrings.openToolStripButton_Text;
            this.ToolTip = MenuStrings.openToolStripButton_Text;
            this.ShortcutKeyString = "Ctrl+O";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            // this.mainFormView = mainFormView;
        }


        public override void Execute()
        {
            var ofd = new OpenFileDialog { Filter = GlobalConfig.tslFilesFIlter };//TODO: move this to mainView or something
            if (ofd.ShowDialog() != DialogResult.OK)
                return;



            switch ((int)SharedViewState.Instance.CurrentView)
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