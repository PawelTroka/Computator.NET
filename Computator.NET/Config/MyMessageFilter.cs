namespace Computator.NET.Config
{
    public class MyMessageFilter : System.Windows.Forms.IMessageFilter
    {
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case WM_MOUSEWHEEL: // 0x020A
                case WM_MOUSEHWHEEL: // 0x020E
                    var hControlUnderMouse = NativeMethods.WindowFromPoint(new System.Drawing.Point((int) m.LParam));
                    if (hControlUnderMouse == m.HWnd)
                        return false; // already headed for the right control
                    // redirect the message to the control under the mouse
                    NativeMethods.SendMessage(hControlUnderMouse, m.Msg, m.WParam, m.LParam);
                    return true;
                default:
                    return false;
            }
        }
    }
}