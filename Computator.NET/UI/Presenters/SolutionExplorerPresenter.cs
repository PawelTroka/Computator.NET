using Computator.NET.Properties;
using Computator.NET.UI.Controls;
using Computator.NET.UI.Controls.CodeEditors;
using Computator.NET.UI.Interfaces;

namespace Computator.NET.UI.Presenters
{
    public class SolutionExplorerPresenter
    {
        private readonly bool _isScripting;
        private readonly ISolutionExplorerView _view;

        public SolutionExplorerPresenter(ISolutionExplorerView view, IDocumentsEditor documentsEditor,
            bool isScriptingOrCustomFunctions)
        {
            _view = view;
            _isScripting = isScriptingOrCustomFunctions;
            _view.DirectoryTree.CodeEditorWrapper = documentsEditor;
            _view.DirectoryTree.Path = _isScripting
                ? Settings.Default.ScriptingDirectory
                : Settings.Default.CustomFunctionsDirectory;
            _view.DirectoryChanged += _view_DirectoryChanged;
        }

        private void _view_DirectoryChanged(object sender, DirectorySelectedEventArgs e)
        {
            _view.DirectoryTree.Path = e.DirectoryName;
            if (_isScripting)
                Settings.Default.ScriptingDirectory = e.DirectoryName;
            else Settings.Default.CustomFunctionsDirectory = e.DirectoryName;
            Settings.Default.Save();
        }
    }
}