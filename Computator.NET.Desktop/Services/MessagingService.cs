using System.Windows.Forms;
using Computator.NET.Core.Abstract.Services;

namespace Computator.NET.Services
{
    class MessagingService : IMessagingService
    {
        public void Show(string message, string title)
        {
            MessageBox.Show(message, title);
        }
    }
}
