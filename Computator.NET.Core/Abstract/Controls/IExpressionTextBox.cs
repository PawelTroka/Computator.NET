using System;
using System.Windows.Forms;

namespace Computator.NET.UI.Controls
{
    public interface ITextProvider
    {
        string Text { get; set; }
    }

    public interface IExpressionTextBox : ITextProvider
    {
        event EventHandler TextChanged;
        event KeyPressEventHandler KeyPress;//TODO: make it platform independent
    }
}