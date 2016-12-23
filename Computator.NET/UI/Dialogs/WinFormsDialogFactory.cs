using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Computator.NET.UI.Dialogs
{
    public class WinFormsDialogFactory : IDialogFactory
    {
        private readonly Dictionary<string, Type> _dialogsTypes=new Dictionary<string, Type>()
        {
            {"about",  typeof(AboutBox1)},
            {"bugs",  typeof(BugReportingForm)},
            {"changelog",  typeof(ChangelogForm)},
            {"loading",  typeof(LoadingScreen)},
            {"read",  typeof(ReadForm)},
        };

        public bool ShowDialog(string name)
        {
            return (Activator.CreateInstance(_dialogsTypes[name]) as Form)?.ShowDialog() == DialogResult.OK;
        }
    }
}