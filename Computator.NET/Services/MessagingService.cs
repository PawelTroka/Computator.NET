using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Computator.NET.UI.ErrorHandling;

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
