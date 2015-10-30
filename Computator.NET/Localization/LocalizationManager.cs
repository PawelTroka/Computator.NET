using Enumerable = System.Linq.Enumerable;

namespace Computator.NET.Localization
{
    internal class LocalizationManager
    {
        public static System.Globalization.CultureInfo GlobalUICulture
        {
            get { return System.Threading.Thread.CurrentThread.CurrentUICulture; }
            set
            {
                if (GlobalUICulture.Equals(value) == false)
                {
                    foreach (var form in Enumerable.OfType<LocalizedForm>(System.Windows.Forms.Application.OpenForms))
                    {
                        form.Culture = value;
                    }

                    System.Threading.Thread.CurrentThread.CurrentUICulture = value;
                    System.Threading.Thread.CurrentThread.CurrentCulture = value;
                }
            }
        }
    }
}