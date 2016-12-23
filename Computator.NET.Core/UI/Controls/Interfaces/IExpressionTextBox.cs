using System;
using System.Windows.Forms;
using Computator.NET.UI.Controls.CodeEditors;

namespace Computator.NET.UI.Controls
{
    public interface IExpressionTextBox : ITextProvider
    {
        event EventHandler TextChanged;
        event KeyPressEventHandler KeyPress;//TODO: make it platform independent
    }
}