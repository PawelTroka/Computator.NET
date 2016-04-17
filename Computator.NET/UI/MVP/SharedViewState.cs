using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Computator.NET.DataTypes;
using Computator.NET.Evaluation;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.MVP
{
    class SharedViewState
    {
        public static void Initialize(ITextProvider expression, ICodeEditorView customFunctions)
        {
            if(_instance==null)
            Instance = new SharedViewState(expression, customFunctions);
        }

        public static SharedViewState Instance
        {
            get { if(_instance==null) throw new Exception($"Call {nameof(Initialize)} method first!"); return _instance; }
            private set { _instance = value; }
        }
        private static SharedViewState _instance;

        private SharedViewState(ITextProvider expression, ICodeEditorView customFunctions)
        {
            this._expression = expression;
            this._customFunctions = customFunctions;
        }

        private ITextProvider _expression;
        private ICodeEditorView _customFunctions;
  

        public ViewName CurrentView { get; private set; }
        public bool IsExponent { get; private set; }
        public CalculationsMode CalculationsMode { get; private set; }

        public string ExpressionText => _expression.Text;

        public ISupportsExceptionHighliting CustomFunctionsEditor => _customFunctions;
        public string CustomFunctionsText => _customFunctions.Text;
    }
}
