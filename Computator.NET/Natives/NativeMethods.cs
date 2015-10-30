namespace Computator.NET
{
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal class NativeMethods
    {
        // ReSharper disable InconsistentNaming
        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3
        }

        // ReSharper restore InconsistentNaming
        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true,
            CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern System.IntPtr LoadLibrary(string lpFileName);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool DeleteObject(System.IntPtr hObject);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool BitBlt(
            System.IntPtr hdcDest, // handle to destination DC
            int nXDest, // x-coord of destination upper-left corner
            int nYDest, // y-coord of destination upper-left corner
            int nWidth, // width of destination rectangle
            int nHeight, // height of destination rectangle
            System.IntPtr hdcSrc, // handle to source DC
            int nXSrc, // x-coordinate of source upper-left corner
            int nYSrc, // y-coordinate of source upper-left corner
            int dwRop // raster operation code
            );

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = false)]
        public static extern System.IntPtr SendMessage(System.IntPtr hWnd, int msg, System.IntPtr wParam,
            System.IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern System.IntPtr GetDC(System.IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern int ReleaseDC(System.IntPtr hWnd, System.IntPtr hDC); //modified to include hWnd

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "WindowFromPoint",
            CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        public static extern System.IntPtr WindowFromPoint(System.Drawing.Point pt);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [System.Runtime.InteropServices.Out,
             System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr,
                 SizeParamIndex = 4)] System.Text.StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool CreateCaret(System.IntPtr hWnd, System.IntPtr hBitmap, int nWidth, int nHeight);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ShowCaret(System.IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport(Config.GlobalConfig.gslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static extern System.IntPtr gsl_set_error_handler_off();

        [System.Runtime.InteropServices.DllImport(Config.GlobalConfig.gslDllName,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static extern gsl_error_handler_t gsl_set_error_handler(gsl_error_handler_t new_handler);
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Size = 16),
     System.Serializable]
    public struct gsl_sf_result
    {
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.R8)] public readonly
            double val;

        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.R8)] public readonly
            double err;
    }

    public delegate void gsl_error_handler_t(
        [System.Runtime.InteropServices.In,
         System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)] string reason,
        [System.Runtime.InteropServices.In,
         System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)] string file,
        int line, int gsl_errno);

    //typedef void gsl_error_handler_t (const char * reason, const char * file,
    //int line, int gsl_errno);
}