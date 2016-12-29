using System.Windows.Forms;
using Computator.NET.Core.Abstract.Services;

namespace Computator.NET.Services
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