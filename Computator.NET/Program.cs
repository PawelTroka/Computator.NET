using System;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Config;
using Settings = Computator.NET.Properties.Settings;

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
            Thread.CurrentThread.CurrentCulture = Settings.Default.Language;
            Thread.CurrentThread.CurrentUICulture = Settings.Default.Language;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.AddMessageFilter(new MyMessageFilter());
            Application.Run(new GUI());
        }
    }
}