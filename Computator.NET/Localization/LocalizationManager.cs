using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Computator.NET.Localization
{
    internal class LocalizationManager
    {
        public static CultureInfo GlobalUICulture
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
            set
            {
                if (GlobalUICulture.Equals(value) == false)
                {
                    foreach (LocalizedForm form in Application.OpenForms.OfType<LocalizedForm>())
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