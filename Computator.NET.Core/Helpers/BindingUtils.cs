using System;
using System.ComponentModel;

namespace Computator.NET.Core.Helpers
{
    public class BindingUtils
    {
        public static void OnPropertyChanged(INotifyPropertyChanged dataSource, string dataSourcePropertyName,
            Action action)
        {
            dataSource.PropertyChanged += (o, e) =>
            {
                // INotifyPropertyChanged permits null/empty to report that all
                // properties changed, not just a particular named property.
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == dataSourcePropertyName)
                    action();
            };
        }

        public static void OneWayBinding(object dataReceiver, string dataReceiverPropertyName,
            INotifyPropertyChanged dataSource, string dataSourcePropertyName)
        {
            var receiverProperty = dataReceiver.GetType().GetProperty(dataReceiverPropertyName);
            var sourceProperty = dataSource.GetType().GetProperty(dataSourcePropertyName);

            dataSource.PropertyChanged += (o, e) =>
            {
                if (!string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != dataSourcePropertyName)
                    return;

                var value = sourceProperty.GetValue(dataSource, null);

                // A setter may raise PropertyChanged even if its value does not
                // change. Writing the same value back would make TwoWayBinding
                // recursively notify both objects until the stack overflows.
                if (!Equals(receiverProperty.GetValue(dataReceiver, null), value))
                    receiverProperty.SetValue(dataReceiver, value, null);
            };
        }

        public static void TwoWayBinding(INotifyPropertyChanged object1, string propertyName1,
            INotifyPropertyChanged object2, string propertyName2)
        {
            OneWayBinding(object1, propertyName1, object2, propertyName2);
            OneWayBinding(object2, propertyName2, object1, propertyName1);
        }
    }
}
