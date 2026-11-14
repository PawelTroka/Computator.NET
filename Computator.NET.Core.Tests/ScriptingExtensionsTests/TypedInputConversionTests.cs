using System;
using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Functions;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.ScriptingExtensionsTests
{
    [TestFixture(false)]
    [TestFixture(true)]
    [NonParallelizable]
    public class TypedInputConversionTests
    {
        private readonly bool _generated;
        private MethodInfo _convert;

        public TypedInputConversionTests(bool generated) { _generated = generated; }

        private sealed class GeneratedSourceScript : ScriptEvaluator
        {
            protected override bool UsePrecompiledBuiltins => false;
        }

        [OneTimeSetUp]
        public void SelectPublicOrActuallyEmittedConversion()
        {
            _convert = typeof(ScriptingFunctions).GetMethod("convert");
            if (!_generated) return;
            // The real evaluator compiles the complete ToCode prelude. Inspect
            // its program once so type and exception checks need no new compiler per case.
            var script = new GeneratedSourceScript().Evaluate(";", "");
            var callback = (Delegate)typeof(BaseFunction)
                .GetField("_function", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(script);
            _convert = callback.Method.DeclaringType.GetMethod("convert", BindingFlags.Public | BindingFlags.Static);
            NUnit.Framework.Assert.That(_convert, Is.Not.Null);
        }

        private object Convert(Type type, string text)
        {
            try { return _convert.MakeGenericMethod(type).Invoke(null, new object[] { text }); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private static IEnumerable ExactValues()
        {
            yield return new TestCaseData(typeof(sbyte), "-128", sbyte.MinValue);
            yield return new TestCaseData(typeof(byte), "255", byte.MaxValue);
            yield return new TestCaseData(typeof(short), "-32768", short.MinValue);
            yield return new TestCaseData(typeof(ushort), "65535", ushort.MaxValue);
            yield return new TestCaseData(typeof(int), "-2147483648", int.MinValue);
            yield return new TestCaseData(typeof(int), "2147483647", int.MaxValue);
            yield return new TestCaseData(typeof(uint), "4294967295", uint.MaxValue);
            yield return new TestCaseData(typeof(long), "-9223372036854775808", long.MinValue);
            yield return new TestCaseData(typeof(long), "9223372036854775807", long.MaxValue);
            yield return new TestCaseData(typeof(long), "9007199254740993", 9007199254740993L);
            yield return new TestCaseData(typeof(ulong), "18446744073709551615", ulong.MaxValue);
            yield return new TestCaseData(typeof(float), "1.25e2", 125f);
            yield return new TestCaseData(typeof(decimal), "1234567890123456789012345678.9", 1234567890123456789012345678.9m);
            yield return new TestCaseData(typeof(decimal), "79228162514264337593543950335", decimal.MaxValue);
            yield return new TestCaseData(typeof(decimal), "1.25e2", 125m);
            yield return new TestCaseData(typeof(double), "8.88024331", 8.88024331d);
            yield return new TestCaseData(typeof(Complex), "1+2i", new Complex(1, 2));
            yield return new TestCaseData(typeof(string), " 1,234 ", " 1,234 ");
        }

        [TestCaseSource(nameof(ExactValues))]
        public void ParsedValuesHaveTheRequestedTypeAndExactRepresentableValue(Type type, string text, object expected)
        {
            var actual = Convert(type, text);
            NUnit.Framework.Assert.That(actual.GetType(), Is.EqualTo(type));
            NUnit.Framework.Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("en-US")]
        [TestCase("pl-PL")]
        [TestCase("de-DE")]
        public void TypedInputUsesInvariantFormatsRegardlessOfUiCulture(string culture)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                NUnit.Framework.Assert.That(Convert(typeof(long), " \t+9007199254740993\r\n"), Is.EqualTo(9007199254740993L));
                NUnit.Framework.Assert.That(Convert(typeof(decimal), "1,234.5"), Is.EqualTo(1234.5m));
                NUnit.Framework.Assert.That(Convert(typeof(float), "1.25"), Is.EqualTo(1.25f));
                NUnit.Framework.Assert.That(Convert(typeof(double), "1,,2"), Is.EqualTo(12d));
                NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits((double)Convert(typeof(double), "8.88024331")),
                    Is.EqualTo(0x4021c2af4049f211L));
                NUnit.Framework.Assert.That(BitConverter.DoubleToInt64Bits((double)Convert(typeof(double), "-0")), Is.EqualTo(long.MinValue));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [TestCase("1.5")]
        [TestCase("1.0")]
        [TestCase("1e2")]
        [TestCase("1,234")]
        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("not a number")]
        public void IntegerInputRejectsFractionsExponentsAndGroupingInsteadOfRounding(string text)
        {
            NUnit.Framework.Assert.Throws<FormatException>(() => Convert(typeof(long), text));
            NUnit.Framework.Assert.Throws<FormatException>(() => Convert(typeof(ulong), text));
        }

        [TestCase(typeof(sbyte), "128")]
        [TestCase(typeof(byte), "256")]
        [TestCase(typeof(short), "32768")]
        [TestCase(typeof(ushort), "65536")]
        [TestCase(typeof(int), "2147483648")]
        [TestCase(typeof(uint), "4294967296")]
        [TestCase(typeof(long), "9223372036854775808")]
        [TestCase(typeof(long), "-9223372036854775809")]
        [TestCase(typeof(ulong), "18446744073709551616")]
        [TestCase(typeof(ulong), "-1")]
        [TestCase(typeof(decimal), "79228162514264337593543950336")]
        public void OutOfRangeInputReportsOverflowForItsRequestedType(Type type, string text)
        {
            NUnit.Framework.Assert.Throws<OverflowException>(() => Convert(type, text));
        }

        [TestCase(null)]
        [TestCase("")]
        public void EmptyOrCancelledInputRetainsTheEstablishedStringAndValueTypeContracts(string text)
        {
            NUnit.Framework.Assert.That(Convert(typeof(string), text), Is.EqualTo(string.Empty));
            foreach (var type in new[] { typeof(double), typeof(long), typeof(decimal), typeof(Complex) })
                NUnit.Framework.Assert.Throws<InvalidCastException>(() => Convert(type, text));
        }

        [TestCase(typeof(bool))]
        [TestCase(typeof(char))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(object))]
        public void UnsupportedTypesFailExplicitlyInsteadOfReturningADefaultValue(Type type)
        {
            NUnit.Framework.Assert.Throws<NotSupportedException>(() => Convert(type, "12"));
        }

        [Test]
        public void ScriptAliasesAndTypedFileReadsKeepExactSignedAndUnsignedIntegers()
        {
            var path = System.IO.Path.GetTempFileName();
            var previousDirectory = Environment.CurrentDirectory;
            try
            {
                System.IO.File.WriteAllText(path, "9007199254740993\n18446744073709551615");
                var source = "var f = new File(@\"" + path.Replace("\"", "\"\"") + "\"); " +
                    "try { integer signed = readln<integer>(f); natural unsigned; read<natural>(f,out unsigned); " +
                    "write(signed); write(\"|\"); write(unsigned); write(\"|\"); write(convert<int>(\"12\")); } finally { f.close(); }";
                var output = new StringBuilder();
                ScriptEvaluator evaluator = _generated ? new GeneratedSourceScript() : new ScriptEvaluator();
                evaluator.Evaluate(source, "").Evaluate(text => output.Append(text));
                NUnit.Framework.Assert.That(output.ToString(), Is.EqualTo("9007199254740993|18446744073709551615|12"));
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
                System.IO.File.Delete(path);
            }
        }
    }
}
