using System;
using System.Collections.Generic;
using System.ComponentModel;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI
{
    public interface ISharedViewState : INotifyPropertyChanged
    {
        CalculationsMode CalculationsMode { get; set; }
        Action<object, EventArgs> CurrentAction { get; }
        ViewName CurrentView { get; set; }
        ISupportsExceptionHighliting CustomFunctionsEditor { get; }
        string CustomFunctionsText { get; }
        Dictionary<ViewName, Action<object, EventArgs>> DefaultActions { get; }
        string ExpressionText { get; }
        bool IsExponent { get; set; }
    }
}