using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Computator.NET.Desktop.Controls
{
    public sealed class IconEx : IDisposable
    {
        public enum SystemIcons
        {
            Application = 100, Asterisk = 104, Error = 103, Exclamation = 101,
            Hand = 103, Information = 104, Question = 102, Shield = 106,
            Warning = 101, WinLogo = 100
        }
        private Icon _icon;
        public Icon Icon => _icon;
        public IconEx(string path, Size size) =>
            Attach(LoadImage(IntPtr.Zero, path, ImageIcon, size.Width, size.Height, LoadFromFile));
        public IconEx(SystemIcons systemIcon, Size size) =>
            Attach(LoadImage(GetModuleHandle("user32"), (IntPtr)systemIcon, ImageIcon, size.Width, size.Height, 0));

        private void Attach(IntPtr handle)
        {
            if (handle == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            try
            {
                // Clone a borrowed HICON through public APIs, not private Framework
                // constructors. The clone owns its independent native handle.
                using (var borrowed = System.Drawing.Icon.FromHandle(handle)) _icon = (Icon)borrowed.Clone();
            }
            finally { DestroyIcon(handle); } // LoadImage did not use LR_SHARED.
        }
        public void Dispose() { _icon?.Dispose(); _icon = null; }
        private const int ImageIcon = 1;
        private const int LoadFromFile = 0x10;
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadImage(IntPtr module, string name, int type, int width, int height, int flags);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadImage(IntPtr module, IntPtr resourceId, int type, int width, int height, int flags);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr handle);
    }
}
