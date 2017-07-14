using System.Windows.Forms;
using Computator.NET.Core.Abstract.Services;

namespace Computator.NET.Desktop.Services
{
    class MessagingService : IMessagingService
    {
        public void Show(string message, string title)
        {
            MessageBox.Show(message, title);
        }
    }
}
