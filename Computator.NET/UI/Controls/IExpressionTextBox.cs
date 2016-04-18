using System;
using System.Windows.Forms;
using Computator.NET.UI.CodeEditors;

namespace Computator.NET.UI.Controls
{
    public interface IExpressionTextBox : ITextProvider
    {
        event EventHandler TextChanged;
        event KeyPressEventHandler KeyPress;
    }
}