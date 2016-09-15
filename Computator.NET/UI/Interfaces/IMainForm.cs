using System;
using System.Windows.Forms;
using Computator.NET.UI.Views;

namespace Computator.NET.UI.Interfaces
{
    public interface IMainForm
    {
        string ModeText { get; set; }
        string StatusText { set; }
        int SelectedViewIndex { get; set; }
        FormBorderStyle FormBorderStyle { set; }
        FormWindowState WindowState { set; }
        
        event EventHandler ModeForcedToReal;
        event EventHandler ModeForcedToComplex;
        event EventHandler ModeForcedToFxy;
        

        event EventHandler Load;
        event EventHandler SelectedViewChanged;
    }
}