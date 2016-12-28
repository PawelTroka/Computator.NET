using System;

namespace Computator.NET.UI.Controls
{
    public class DirectorySelectedEventArgs : EventArgs
    {
        public string DirectoryName;

        public DirectorySelectedEventArgs(string directoryName)
        {
            DirectoryName = directoryName;
        }
    }
}