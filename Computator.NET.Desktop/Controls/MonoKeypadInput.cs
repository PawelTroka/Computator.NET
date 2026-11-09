#if __MonoCS__
using System;
using System.Windows.Forms;
using Computator.NET.DataTypes.Configuration;

namespace Computator.NET.Desktop.Controls
{
    // Mono 6.8's non-XIM XLookupString marshaller loses keypad characters
    // (mono/mono#18423). Keep the workaround local to the two text editors.
    internal static class MonoKeypadInput
    {
        private const int WmKeyDown = 0x0100;
        private const int WmChar = 0x0102;
        internal static bool ProcessKey(Control input, ref Message message, Keys keyData)
        {
            if (!RuntimeInformation.IsLinux || message.Msg != WmKeyDown || !input.Focused ||
                !input.IsHandleCreated || message.HWnd != input.Handle ||
                (keyData & (Keys.Control | Keys.Alt)) != Keys.None)
                return false;

            char character;
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Add: character = '+'; break;
                case Keys.Subtract: character = '-'; break;
                case Keys.Multiply: character = '*'; break;
                case Keys.Divide: character = '/'; break;
                default: return false;
            }

            var text = Message.Create(message.HWnd, WmChar, (IntPtr)character, message.LParam);
            // Retain KeyDown/SuppressKeyPress, then use the normal KeyPress and
            // editor insertion path (including superscripts, selection and undo).
            input.WindowTarget.OnMessage(ref message);
            if (!input.IsDisposed && input.IsHandleCreated && input.Handle == text.HWnd &&
                input.PreProcessControlMessage(ref text) != PreProcessControlState.MessageProcessed)
                input.WindowTarget.OnMessage(ref text);

            // ProcessCmdKey runs after menu capture. Consume the original key
            // before Mono translates it, avoiding duplicates even with working XIM.
            return true;
        }
    }
}
#endif
