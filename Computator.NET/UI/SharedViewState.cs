using System;
using System.Collections.Generic;
using System.ComponentModel;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI
{

    public class SharedViewState : ISharedViewState
    {
        private CalculationsMode _calculationsMode;
        private ViewName _currentView;
        private readonly ICodeEditorView _customFunctions;

        private readonly ITextProvider _expression;
        private bool _isExponent;


        public SharedViewState(ITextProvider expression, ICodeEditorView customFunctions)
        {
            this._expression = expression;
            this._customFunctions = customFunctions;

            EventAggregator.Instance.Subscribe<CalculationsModeChangedEvent>(
                cmce => { CalculationsMode = cmce.CalculationsMode; });
        }

        public Dictionary<ViewName, Action<object, EventArgs>> DefaultActions { get; } =
            new Dictionary<ViewName, Action<object, EventArgs>>();

        public Action<object, EventArgs> CurrentAction
            => DefaultActions.ContainsKey(CurrentView) ? DefaultActions[CurrentView] : null;


        public ViewName CurrentView
        {
            get { return _currentView; }
            set
            {
                if (_currentView == value) return;
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public bool IsExponent
        {
            get { return _isExponent; }
            set
            {
                if (_isExponent == value) return;
                _isExponent = value;
                OnPropertyChanged(nameof(IsExponent));
            }
        }

        public CalculationsMode CalculationsMode
        {
            get { return _calculationsMode; }
            set
            {
                if (_calculationsMode == value) return;
                _calculationsMode = value;
                OnPropertyChanged(nameof(CalculationsMode));
            }
        }

        public string ExpressionText => _expression.Text;

        public ISupportsExceptionHighliting CustomFunctionsEditor => _customFunctions;
        public string CustomFunctionsText => _customFunctions.Text;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}