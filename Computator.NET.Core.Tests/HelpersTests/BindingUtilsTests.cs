using System;
using System.ComponentModel;
using Computator.NET.Core.Helpers;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.HelpersTests
{
    [TestFixture]
    public class BindingUtilsTests
    {
        [Test]
        public void TwoWayBindingDoesNotRecursivelySetAnUnchangedValue()
        {
            var left = new AlwaysNotifyingValue();
            var right = new AlwaysNotifyingValue();
            BindingUtils.TwoWayBinding(left, nameof(left.Value), right, nameof(right.Value));

            left.Value = 42;
            NUnit.Framework.Assert.That(right.Value, Is.EqualTo(42));
            NUnit.Framework.Assert.That(left.SetCount, Is.EqualTo(1));
            NUnit.Framework.Assert.That(right.SetCount, Is.EqualTo(1));

            right.Value = 9;
            NUnit.Framework.Assert.That(left.Value, Is.EqualTo(9));
            NUnit.Framework.Assert.That(left.SetCount, Is.EqualTo(2));
            NUnit.Framework.Assert.That(right.SetCount, Is.EqualTo(2));
        }

        [TestCase(null)]
        [TestCase("")]
        public void OneWayBindingRecognizesAllPropertiesChanged(string propertyName)
        {
            var source = new AlwaysNotifyingValue { Value = 8 };
            var receiver = new AlwaysNotifyingValue();
            BindingUtils.OneWayBinding(receiver, nameof(receiver.Value), source, nameof(source.Value));

            source.Notify(propertyName);

            NUnit.Framework.Assert.That(receiver.Value, Is.EqualTo(8));
            NUnit.Framework.Assert.That(receiver.SetCount, Is.EqualTo(1));
        }

        [Test]
        public void OnPropertyChangedRecognizesAllPropertiesChangedButNotUnrelatedNames()
        {
            var source = new AlwaysNotifyingValue();
            var invocations = 0;
            BindingUtils.OnPropertyChanged(source, nameof(source.Value), () => invocations++);

            source.Notify(null);
            source.Notify("");
            source.Notify("AnotherProperty");
            source.Notify(nameof(source.Value));

            NUnit.Framework.Assert.That(invocations, Is.EqualTo(3));
        }

        private sealed class AlwaysNotifyingValue : INotifyPropertyChanged
        {
            private int _value;
            public event PropertyChangedEventHandler PropertyChanged;
            public int SetCount { get; private set; }

            public int Value
            {
                get => _value;
                set
                {
                    if (++SetCount > 10)
                        throw new InvalidOperationException("The binding recursively wrote the same value.");
                    _value = value;
                    Notify(nameof(Value)); // Fires even for an unchanged value.
                }
            }

            public void Notify(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
