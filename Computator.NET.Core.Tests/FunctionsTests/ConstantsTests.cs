using System;
using System.Diagnostics;
using System.Reflection;
using Computator.NET.Core.Constants;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
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
        public void PhysicsConstants_RetainTheirCoreTypeIdentity()
        {
            var core = typeof(Computator.NET.Core.Evaluation.ExpressionsEvaluator).Assembly;
            Assert.AreEqual(typeof(PhysicalConstants), core.GetType(typeof(PhysicalConstants).FullName, true));
            Assert.AreEqual(typeof(Computator.NET.Engine.PortableEngine).Assembly, typeof(PhysicalConstants).Assembly);
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
