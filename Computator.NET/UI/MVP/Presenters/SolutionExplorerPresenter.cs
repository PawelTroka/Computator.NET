using Computator.NET.Properties;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Views
{
    public class SolutionExplorerPresenter
    {
        private readonly ISolutionExplorerView _view;
        private readonly bool _isScripting;

        public SolutionExplorerPresenter(ISolutionExplorerView view, IDocumentsEditor documentsEditor, bool isScriptingOrCustomFunctions)
        {
            _view = view;
            _isScripting = isScriptingOrCustomFunctions;
            _view.DirectoryTree.CodeEditorWrapper = documentsEditor;
            _view.DirectoryTree.Path = _isScripting ? Settings.Default.ScriptingDirectory : Settings.Default.CustomFunctionsDirectory;
            _view.DirectoryChanged += _view_DirectoryChanged;
        }

        private void _view_DirectoryChanged(object sender, DirectorySelectedEventArgs e)
        {
            _view.DirectoryTree.Path = e.DirectoryName;
            if(_isScripting)
            Settings.Default.ScriptingDirectory = e.DirectoryName;
            else Settings.Default.CustomFunctionsDirectory = e.DirectoryName;
            Settings.Default.Save();
        }
    }
}