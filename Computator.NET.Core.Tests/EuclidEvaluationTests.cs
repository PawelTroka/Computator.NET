using System;
using System.Collections.Generic;
using System.Linq;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Properties;
using Computator.NET.DataTypes;
using NUnit.Framework;

namespace Computator.NET.Core.Tests
{
    [TestFixture(false)]
    [TestFixture(true)]
    [NonParallelizable]
    public class EuclidEvaluationTests
    {
        private readonly bool _legacy;
        public EuclidEvaluationTests(bool legacy) { _legacy = legacy; }

        private sealed class LegacyEvaluator : ExpressionsEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }
        private sealed class LegacyScriptEvaluator : ScriptEvaluator
        { protected override bool UsePrecompiledBuiltins => false; }

        [TestCase("Euclid.GreatestCommonDivisor(-84,30)", 6)]
        [TestCase("Euclid.LeastCommonMultiple(12,18)", 36)]
        [TestCase("Euclid.Modulus(-7,3)", 2)]
        [TestCase("Euclid.Remainder(-7,3)", -1)]
        [TestCase("Euclid.PowerOfTwo((integer)40)", 1099511627776)]
        [TestCase("Euclid.IsPerfectSquare(144) ? 1 : 0", 1)]
        [TestCase("((real)Euclid.GreatestCommonDivisor(System.Numerics.BigInteger.One<<80,System.Numerics.BigInteger.Zero))", 1.2089258196146292E+24)]
        public void DesktopExpressionsUseTheSameQualifiedLibrary(string source, double expected)
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyEvaluator() : new ExpressionsEvaluator();
            Func<double, double> function = evaluator.Evaluate(source, "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(0), Is.EqualTo(expected));
        }

        [Test]
        public void DesktopScriptRetainsExactIntegersAndOutArguments()
        {
            const string source = """
                integer u, v;
                integer divisor = Euclid.ExtendedGreatestCommonDivisor(240,46,out u,out v);
                write(divisor); write(":["); write(240*u+46*v); write("]:");
                write(Euclid.GreatestCommonDivisor(9007199254740993,0));
                """;
            ScriptEvaluator evaluator = _legacy ? new LegacyScriptEvaluator() : new ScriptEvaluator();
            var output = new List<string>();
            evaluator.Evaluate(source, "").Evaluate(output.Add);
            NUnit.Framework.Assert.That(string.Concat(output), Is.EqualTo("2:[2]:9007199254740993"));
        }

        [Test]
        public void ExistingUnqualifiedGcdAliasesKeepTheirLegacySurface()
        {
            ExpressionsEvaluator evaluator = _legacy ? new LegacyEvaluator() : new ExpressionsEvaluator();
            Func<double, double> function = evaluator.Evaluate("gcd(12,18)+nwd(12,18)+lcm(4,6)+nww(4,6)", "", CalculationsMode.Real);
            NUnit.Framework.Assert.That(function(0), Is.EqualTo(36));
        }
    }

    [TestFixture, NonParallelizable]
    public class EuclidCompletionTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void ExtendedGcdCompletionInsertsExecutableOutputArguments(bool arbitraryPrecision)
        {
            var settings = Settings.Default;
            var oldReturn = settings.ShowReturnTypeInScripting;
            var oldParameters = settings.ShowParametersTypeInScripting;
            try
            {
                settings.ShowReturnTypeInScripting = settings.ShowParametersTypeInScripting = true;
                var items = new AutocompleteReflectionSource().GetFunctionsNamesWithDescription(
                    typeof(MathNet.Numerics.Euclid), fullName: true);
                items.ForEach(item => item.IsScripting = true);
                var type = arbitraryPrecision ? "BigInteger" : "integer";
                var completion = items.Single(item => item.Text.StartsWith("Euclid.ExtendedGreatestCommonDivisor(")
                    && item.MenuText.StartsWith(type + " "));

                NUnit.Framework.Assert.That(completion.GetTextForReplace(),
                    Is.EqualTo("Euclid.ExtendedGreatestCommonDivisor(a,b,out x,out y)"));
                NUnit.Framework.Assert.That(completion.MenuText,
                    Is.EqualTo(type + " Euclid.ExtendedGreatestCommonDivisor(" + type + " a, " + type +
                        " b, out " + type + " x, out " + type + " y)"));

                var declarationType = arbitraryPrecision ? "System.Numerics.BigInteger" : "integer";
                var script = declarationType + " a=240,b=46,x,y; var d=" + completion.GetTextForReplace() +
                    "; write(d); write(\":\"); write(a*x+b*y);";
                var output = new List<string>();
                new ScriptEvaluator().Evaluate(script, "").Evaluate(output.Add);
                NUnit.Framework.Assert.That(string.Concat(output), Is.EqualTo("2:2"));
            }
            finally
            {
                settings.ShowReturnTypeInScripting = oldReturn;
                settings.ShowParametersTypeInScripting = oldParameters;
            }
        }

        [Test]
        public void EveryPinnedEuclidOverloadHasQualifiedCompletionAndAuthoredHelp()
        {
            var reference = new FunctionsDetailsFileSource();
            var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), reference);
            var items = provider.ScriptingAutocompleteItems.Where(item => item.Details.Type == "Euclid").ToArray();
            NUnit.Framework.Assert.That(items.Length, Is.EqualTo(37));
            NUnit.Framework.Assert.That(items.Select(item => item.Text.Split('(')[0]).Distinct().Count(), Is.EqualTo(12));
            foreach (var item in items)
            {
                NUnit.Framework.Assert.That(item.Text, Does.StartWith("Euclid."));
                NUnit.Framework.Assert.That(item.Details, Is.SameAs(reference.Details[item.Text]), item.Text);
                NUnit.Framework.Assert.That(item.Details.Category, Is.EqualTo("Integer arithmetic"), item.Text);
            }
            NUnit.Framework.Assert.That(provider.ExpressionAutocompleteItems.Select(item => item.Text),
                Does.Contain("Euclid.GreatestCommonDivisor(a,b)"));
        }
    }
}
