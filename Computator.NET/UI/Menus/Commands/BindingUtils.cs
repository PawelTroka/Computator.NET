using System;
using System.ComponentModel;

namespace Computator.NET.UI.Commands
{
    public class BindingUtils
    {

        public static void OnPropertyChanged(INotifyPropertyChanged dataSource, string dataSourcePropertyName, Action action)
        {
            dataSource.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == dataSourcePropertyName)
                    action();
            };
        }

        public static void OneWayBinding(object dataReceiver, string dataReceiverPropertyName, INotifyPropertyChanged dataSource, string dataSourcePropertyName)
        {
            var receiverProperty = dataReceiver.GetType().GetProperty(dataReceiverPropertyName);
            var sourceProperty = dataSource.GetType().GetProperty(dataSourcePropertyName);


            /*  dataReceiver.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName != dataReceiverPropertyName) return;
                var value1 = receiverProperty.GetValue(dataReceiver, null);
                sourceProperty.SetValue(dataSource, value1, null);
            };*/

            dataSource.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName != dataSourcePropertyName) return;
                var value2 = sourceProperty.GetValue(dataSource, null);
                receiverProperty.SetValue(dataReceiver, value2, null);
            };
        }

        public static void TwoWayBinding(INotifyPropertyChanged object1, string propertyName1, INotifyPropertyChanged object2, string propertyName2)
        {
            OneWayBinding(object1,propertyName1,object2,propertyName2);
            OneWayBinding( object2, propertyName2, object1, propertyName1);
            /*var property1 = object1.GetType().GetProperty(propertyName1);
            var property2 = object2.GetType().GetProperty(propertyName2);


            object1.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName != propertyName1) return;
                var value1 = property1.GetValue(object1, null);
                property2.SetValue(object2, value1, null);
            };

            object2.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName != propertyName2) return;
                var value2 = property2.GetValue(object2, null);
                property1.SetValue(object1, value2, null);
            };*/
        }
    }
}