using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Computator.NET.Core.Compilation.Builtins;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
{
    [TestFixture]
    public class PrecompiledBuiltinStateDiagnosticsTests
    {
        [TestCase(typeof(PrecompiledExpressionSurface))]
        [TestCase(typeof(PrecompiledScriptMathSurface))]
        public void SharedSurfacesContainNoUnreviewedStaticStorage(Type surface)
        {
            var rejected = RejectedFields(surface).ToArray();
            foreach (var field in rejected) TestContext.Progress.WriteLine("NONSHAREABLE: " + field);
            NUnit.Framework.Assert.That(rejected, Is.Empty, string.Join(Environment.NewLine, rejected));
        }

        private static IEnumerable<string> RejectedFields(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var field in type.GetFields(flags))
            {
                var t = field.FieldType;
                if (field.IsLiteral || field.IsInitOnly && ((t.IsPrimitive && t != typeof(IntPtr) && t != typeof(UIntPtr)) ||
                    t.IsEnum || t == typeof(decimal) || t == typeof(Complex) || t == typeof(string))) continue;
                yield return type.FullName + "." + field.Name + ": " + t.FullName + "; attributes=" + field.Attributes +
                    "; compilerGenerated=" + field.IsDefined(typeof(CompilerGeneratedAttribute), false);
            }
            foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(nested => !nested.IsDefined(typeof(CompilerGeneratedAttribute), false)))
                foreach (var rejected in RejectedFields(nested)) yield return rejected;
        }
    }
}
