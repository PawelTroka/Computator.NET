using System;

namespace Computator.NET.Core.Model
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