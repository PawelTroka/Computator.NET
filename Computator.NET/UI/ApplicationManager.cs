using System.Windows.Forms;

namespace Computator.NET.UI.Views
{
    public class ApplicationManager : IApplicationManager
    {
        public void SendStringAsKey(string key)
        {
            SendKeys.Send(key);
        }
        public void Restart()
        {
            Application.Restart();
        }
    }
}