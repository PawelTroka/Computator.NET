using System.Windows.Forms;
using Computator.NET.DataTypes;
using Computator.NET.Properties;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Models;

namespace Computator.NET.UI.Menus.Commands.FileCommands
{
    public interface IOpenFileDialog
    {
        bool Show();
        string Filter { get; set; }
        string FileName { get; set; }
    }

    public class OpenFileDialogWrapper : IOpenFileDialog
    {
        private readonly OpenFileDialog ofd = new OpenFileDialog {Filter = GlobalConfig.TslFilesFIlter};
    public bool Show()
    {
        return ofd.ShowDialog() == DialogResult.OK;
    }

        public string Filter { get {  return ofd.Filter; } set {ofd.Filter = value;} }

        public string FileName { get { return ofd.FileName; } set { ofd.FileName = value; } }
    }

    public class OpenCommand : CommandBase
    {
        private readonly ICanFileEdit customFunctionsCodeEditor;
        private readonly ICanFileEdit scriptingCodeEditor;
        private readonly ISharedViewState _sharedViewState;
        private readonly IOpenFileDialog _openFileDialog;

        public OpenCommand(ICanFileEdit scriptingCodeEditor, ICanFileEdit customFunctionsCodeEditor, ISharedViewState sharedViewState, IOpenFileDialog openFileDialog)
        {
            Icon = Resources.openToolStripButtonImage;
            Text = MenuStrings.openToolStripButton_Text;
            ToolTip = MenuStrings.openToolStripButton_Text;
            ShortcutKeyString = "Ctrl+O";
            this.scriptingCodeEditor = scriptingCodeEditor;
            this.customFunctionsCodeEditor = customFunctionsCodeEditor;
            _sharedViewState = sharedViewState;
            _openFileDialog = openFileDialog;
            _openFileDialog.Filter = GlobalConfig.TslFilesFIlter;
        }


        public override void Execute()
        {
            if (!_openFileDialog.Show())
                return;


            switch ((int) _sharedViewState.CurrentView)
            {
                case 0:

                    //mainFormView.SendStringAsKey("^S");
                    break;

                case 4:
                    scriptingCodeEditor.NewDocument(_openFileDialog.FileName);
                    break;

                case 5:
                    customFunctionsCodeEditor.NewDocument(_openFileDialog.FileName);
                    break;

                default:
                    //mainFormView.SendStringAsKey("^S");
                    break;
            }

            //mainFormView.SendStringAsKey("^S");
        }
    }
}