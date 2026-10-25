using System;
using System.IO;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.Events;
using Computator.NET.Localization.Menu;

namespace Computator.NET.Core.Menu.Commands.FileCommands
{
    public interface IOpenFileDialog
    {
        bool Show();
        string Filter { get; set; }
        string FileName { get; set; }
    }

    public class OpenFileDialogWrapper : IOpenFileDialog
    {
        private readonly OpenFileDialog ofd = new OpenFileDialog {Filter = HandledFilesInformation.TslFilesFIlter};
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
            _openFileDialog.Filter = HandledFilesInformation.TslFilesFIlter;
        }


        public override void Execute()
        {
            if (!_openFileDialog.Show())
                return;

            // Match shell activation: the selected document determines its editor,
            // regardless of the tool that was active when the dialog opened.
            var filename = _openFileDialog.FileName;
            // The chooser normally rejects missing files, but a selected file
            // may disappear before its editor opens. Do not create a blank tab
            // masquerading as the document that the user selected.
            if (!File.Exists(filename))
                throw new FileNotFoundException("The selected document is no longer available.", filename);

            var isCustomFunctions = string.Equals(Path.GetExtension(filename), ".tslf",
                StringComparison.OrdinalIgnoreCase);
            var editor = isCustomFunctions ? customFunctionsCodeEditor : scriptingCodeEditor;
            editor.NewDocument(filename);

            // A failed open must leave the previous tool active.
            _sharedViewState.CurrentView = isCustomFunctions ? ViewName.CustomFunctions : ViewName.Scripting;
        }
    }
}