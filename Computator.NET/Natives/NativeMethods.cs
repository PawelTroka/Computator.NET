using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Computator.NET
{
    [SuppressUnmanagedCodeSecurity]
    internal class NativeMethods
    {
        private NativeMethods()
        {
            //all methods in this class would be static
        }

        [DllImport("MimeTex.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateGifFromEq(string expr, string fileName);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hLibModule);


        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);
    }
}