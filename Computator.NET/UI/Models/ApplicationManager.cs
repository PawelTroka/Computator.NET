using System.Windows.Forms;
using Computator.NET.UI.Views;

namespace Computator.NET.UI.Models
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