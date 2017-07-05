using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Computator.NET.Core.Constants;
using NUnit.Framework;

namespace Computator.NET.Tests
{
    [TestFixture]
    class ConstantsTests
    {
        [Test]
        public void PhysicsConstants_AreAccessible()
        {
            AreAccessible(typeof(PhysicalConstants));
        }

        [Test]
        public void MathematicalConstants_AreAccessible()
        {
            AreAccessible(typeof(MathematicalConstants));
        }

        private static void AreAccessible(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var fieldInfo in fields)
            {
                Debug.WriteLine($"Processing field {fieldInfo.Name}");
                CanGetValue(() => fieldInfo.GetValue(null));
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (var propertyInfo in properties)
            {
                Debug.WriteLine($"Processing property {propertyInfo.Name}");
                CanGetValue(() => propertyInfo.GetValue(null,null));
            }
        }

        private static void CanGetValue(Func<object> valueGetter)
        {
            object obj = null;

            Assert.DoesNotThrow(() => { obj = valueGetter(); });

            Assert.IsNotNull(obj);
        }
    }
}
