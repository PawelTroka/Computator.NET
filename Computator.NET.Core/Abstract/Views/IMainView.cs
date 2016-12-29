using System;
using System.Windows.Forms;

namespace Computator.NET.Core.Abstract.Views
{
    public interface IMainView
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