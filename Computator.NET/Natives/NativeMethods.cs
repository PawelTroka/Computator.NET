using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Computator.NET.Config;

namespace Computator.NET
{
    [SuppressUnmanagedCodeSecurity]
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
    //    [DllImport("kernel32", SetLastError = true,
      //      CharSet = CharSet.Unicode)]
        //public static extern IntPtr LoadLibrary(string lpFileName);



        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);


        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(
            IntPtr hdcDest, // handle to destination DC
            int nXDest, // x-coord of destination upper-left corner
            int nYDest, // y-coord of destination upper-left corner
            int nWidth, // width of destination rectangle
            int nHeight, // height of destination rectangle
            IntPtr hdcSrc, // handle to source DC
            int nXSrc, // x-coordinate of source upper-left corner
            int nYSrc, // y-coordinate of source upper-left corner
            int dwRop // raster operation code
            );

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam,
            IntPtr lParam);

        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC); //modified to include hWnd

        [DllImport("user32.dll", EntryPoint = "WindowFromPoint",
            CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr WindowFromPoint(Point pt);

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out,
             MarshalAs(UnmanagedType.LPWStr,
                 SizeParamIndex = 4)] StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        [DllImport("user32.dll")]
        public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport("user32.dll")]
        public static extern bool ShowCaret(IntPtr hWnd);

        [DllImport(GlobalConfig.gslDllName,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gsl_set_error_handler_off();

        [DllImport(GlobalConfig.gslDllName,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern gsl_error_handler_t gsl_set_error_handler(gsl_error_handler_t new_handler);
    }

    [StructLayout(LayoutKind.Sequential, Size = 16),
     Serializable]
    public struct gsl_sf_result
    {
        [MarshalAs(UnmanagedType.R8)] public readonly
            double val;

        [MarshalAs(UnmanagedType.R8)] public readonly
            double err;
    }

    public delegate void gsl_error_handler_t(
        [In,
         MarshalAs(UnmanagedType.LPStr)] string reason,
        [In,
         MarshalAs(UnmanagedType.LPStr)] string file,
        int line, int gsl_errno);

    //typedef void gsl_error_handler_t (const char * reason, const char * file,
    //int line, int gsl_errno);
}