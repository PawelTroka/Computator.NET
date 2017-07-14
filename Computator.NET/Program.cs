using System;
using Computator.NET.Desktop;

namespace Computator.NET
{
    internal static class Program
    {

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Startup.Start();
        }
    }
}