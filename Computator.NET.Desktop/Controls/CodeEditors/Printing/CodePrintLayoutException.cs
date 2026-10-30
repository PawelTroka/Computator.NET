using System;

namespace Computator.NET.Desktop.Controls.CodeEditors.Printing
{
    // Only deliberate pagination/layout guards are user-facing print errors.
    // An unrelated InvalidOperationException still identifies a programming error.
    internal sealed class CodePrintLayoutException : InvalidOperationException
    {
        internal CodePrintLayoutException(string message) : base(message) { }
    }
}
