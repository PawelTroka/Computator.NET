using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Computator.NET.Localization
{
    public class LocalizationManager
    {
        public static CultureInfo GlobalUICulture
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
            set
            {
                if (GlobalUICulture.Equals(value) == false)
                {
                    foreach (var form in Enumerable.OfType<LocalizedForm>(Application.OpenForms))
                    {
                        form.Culture = value;
                    }

                    Thread.CurrentThread.CurrentUICulture = value;
                    Thread.CurrentThread.CurrentCulture = value;
                }
            }
        }
    }
}