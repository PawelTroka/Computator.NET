using System;
using System.Text;
using System.Threading;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes.SettingsTypes;
using NUnit.Framework;

namespace Computator.NET.Desktop.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class Net10ScriptOutputTests
    {
        [TestCase(NumericalOutputNotationType.MathematicalNotation, "1.232223·10⁻²¹")]
        [TestCase(NumericalOutputNotationType.EngineeringNotation, "1.232223E-21")]
        public void GeneratedScriptAndHostAgreeOnExactDecimalFormatting(NumericalOutputNotationType notation, string expected)
        {
            NUnit.Framework.Assert.That(Environment.Version.Major, Is.EqualTo(10));
            GSLInitializer.Initialize();
            var settings = Computator.NET.Core.Properties.Settings.Default;
            var previous = settings.NumericalOutputNotation;
            try
            {
                settings.NumericalOutputNotation = notation;
                var evaluator = new ScriptEvaluator();
                var function = evaluator.Evaluate("write(System.Decimal.Parse(\"0.000000000000000000001232223\", System.Globalization.CultureInfo.InvariantCulture).ToMathString());", "");
                var result = new StringBuilder();
                function.Evaluate(text => result.Append(text));
                NUnit.Framework.Assert.That(result.ToString(), Is.EqualTo(expected));
                NUnit.Framework.Assert.That((1.232223e-21m).ToMathString(), Is.EqualTo(expected));
            }
            finally { settings.NumericalOutputNotation = previous; }
        }
    }
}
