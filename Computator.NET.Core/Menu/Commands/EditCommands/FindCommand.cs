using System;
using System.ComponentModel;
using System.Windows.Forms;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes.Events;
using Computator.NET.Localization;

namespace Computator.NET.Core.Menu.Commands.EditCommands
{
    public class FindCommand : CommandBase, IDisposable
    {
        private readonly ICodeDocumentsEditor _scripting;
        private readonly ICodeDocumentsEditor _customFunctions;
        private readonly ISharedViewState _state;
        private readonly bool _replace;

        public FindCommand(ICodeDocumentsEditor scripting, ICodeDocumentsEditor customFunctions,
            ISharedViewState sharedViewState) : this(scripting, customFunctions, sharedViewState, false) { }

        protected FindCommand(ICodeDocumentsEditor scripting, ICodeDocumentsEditor customFunctions,
            ISharedViewState sharedViewState, bool replace)
        {
            _scripting = scripting;
            _customFunctions = customFunctions;
            _state = sharedViewState;
            _replace = replace;
            Text = EditorSearchStrings.Get(replace ? "ReplaceMenu" : "FindMenu");
            ToolTip = EditorSearchStrings.Get(replace ? "ReplaceTitle" : "FindTitle");
            ShortcutKeyString = replace ? "Ctrl+H" : "Ctrl+F";
            ShortcutKey = Keys.Control | (replace ? Keys.H : Keys.F);
            _state.PropertyChanged += StateChanged;
            UpdateEnabled();
        }

        private void StateChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ISharedViewState.CurrentView)) UpdateEnabled();
        }

        private void UpdateEnabled() => IsEnabled =
            _state.CurrentView == ViewName.Scripting || _state.CurrentView == ViewName.CustomFunctions;

        public override void Execute()
        {
            if (_state.CurrentView == ViewName.Scripting) _scripting.ShowFindReplace(_replace);
            else if (_state.CurrentView == ViewName.CustomFunctions) _customFunctions.ShowFindReplace(_replace);
        }

        public void Dispose() => _state.PropertyChanged -= StateChanged;
    }

    public sealed class ReplaceCommand : FindCommand
    {
        public ReplaceCommand(ICodeDocumentsEditor scripting, ICodeDocumentsEditor customFunctions,
            ISharedViewState sharedViewState) : base(scripting, customFunctions, sharedViewState, true) { }
    }
}
