using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Computator.NET.Config
{
    public class MyMessageFilter : IMessageFilter
    {
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEWHEEL: // 0x020A
                case WM_MOUSEHWHEEL: // 0x020E
                    var hControlUnderMouse = WindowFromPoint(new Point((int) m.LParam));
                    if (hControlUnderMouse == m.HWnd)
                        return false; // already headed for the right control
                    // redirect the message to the control under the mouse
                    SendMessage(hControlUnderMouse, m.Msg, m.WParam, m.LParam);
                    return true;
                default:
                    return false;
            }
        }

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "WindowFromPoint", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr WindowFromPoint(Point pt);
    }
}