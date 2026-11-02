using System;
using System.Linq;
using System.Xml.Linq;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Properties;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture, NonParallelizable]
    public class PolynomialAutocompleteTests
    {
        private static AutocompleteProvider Provider() =>
            new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource());

        [Test]
        public void ScriptingExposesTheCallablePolynomialCatalog()
        {
            var items = Provider().ScriptingAutocompleteItems.Where(item => item.Details.Type == "Polynomial").ToArray();
            var expected = new[] {
                "new Polynomial(coefficients)", "Polynomial.Zero", "Polynomial.Fit(x,y,order,method)",
                "Polynomial.Evaluate(z,coefficients1,coefficients2, ...)", "Polynomial.Add(a,b)",
                "Polynomial.Subtract(a,b)", "Polynomial.Subtract(b,a)", "Polynomial.Negate(a)",
                "Polynomial.Multiply(a,b)", "Polynomial.Multiply(a,k)", "Polynomial.Divide(a,k)",
                "Polynomial.DivideRemainder(a,b)", "Polynomial.PointwiseDivide(a,b)", "Polynomial.PointwiseMultiply(a,b)"
            };
            NUnit.Framework.Assert.That(items, Has.Length.EqualTo(18));
            NUnit.Framework.Assert.That(items.Select(item => item.Text).Distinct(), Is.EquivalentTo(expected));
            NUnit.Framework.Assert.That(items.All(item => item.IsScripting), Is.True);
            NUnit.Framework.Assert.That(items.Any(item => item.Text.Contains(".op_") || item.Text.Contains(".get_")), Is.False);
        }

        [Test]
        public void ExpressionCompletionRetainsOnlyNumericEvaluationOverloads()
        {
            var provider = Provider();
            var items = provider.ExpressionAutocompleteItems.Where(item => item.Details.Type == "Polynomial").ToArray();
            NUnit.Framework.Assert.That(items, Has.Length.EqualTo(3));
            NUnit.Framework.Assert.That(items.All(item => item.Text == "Polynomial.Evaluate(z,coefficients1,coefficients2, ...)"), Is.True);
            NUnit.Framework.Assert.That(items.All(item => item.ImageIndex >= 0 && !item.IsScripting), Is.True);
            NUnit.Framework.Assert.That(provider.ScriptingAutocompleteItems.Any(item => item.Text == "Zero"), Is.False);
        }

        [Test]
        public void EveryTypedCompletionHasItsAuthoredReference()
        {
            var settings = Settings.Default;
            var oldReturn = settings.ShowReturnTypeInScripting;
            var oldParameters = settings.ShowParametersTypeInScripting;
            try
            {
                settings.ShowReturnTypeInScripting = settings.ShowParametersTypeInScripting = true;
                var items = Provider().ScriptingAutocompleteItems.Where(item => item.Details.Type == "Polynomial").ToArray();
                foreach (var item in items)
                {
                    var fragment = XElement.Parse(item.Details.Description);
                    var captions = fragment.Elements("p").Where(p => (string)p.Attribute("class") == "reference-signature")
                        .Elements("code").Select(code => code.Value);
                    NUnit.Framework.Assert.That(captions, Does.Contain(item.MenuText), item.Text);
                    NUnit.Framework.Assert.That(item.Details.Category, Is.EqualTo("Polynomial operations"));
                    NUnit.Framework.Assert.That(item.Details.Signature, Is.EqualTo(item.Text));
                    NUnit.Framework.Assert.That(item.Details.Description, Does.Contain("reference-worked-example"));
                }
                NUnit.Framework.Assert.That(items.Single(item => item.Text == "Polynomial.DivideRemainder(a,b)").MenuText,
                    Is.EqualTo("(Polynomial, Polynomial) Polynomial.DivideRemainder(Polynomial a, Polynomial b)"));
            }
            finally
            {
                settings.ShowReturnTypeInScripting = oldReturn;
                settings.ShowParametersTypeInScripting = oldParameters;
            }
        }

        [Test]
        public void ConstructorCaptionHonorsParameterDisplayPreferences()
        {
            var settings = Settings.Default;
            var oldReturn = settings.ShowReturnTypeInScripting;
            var oldParameters = settings.ShowParametersTypeInScripting;
            try
            {
                var item = Provider().ScriptingAutocompleteItems.Single(entry => entry.Text == "new Polynomial(coefficients)");
                settings.ShowReturnTypeInScripting = settings.ShowParametersTypeInScripting = true;
                NUnit.Framework.Assert.That(item.MenuText, Is.EqualTo("new Polynomial(real[] coefficients)"));
                settings.ShowParametersTypeInScripting = false;
                NUnit.Framework.Assert.That(item.MenuText, Is.EqualTo("new Polynomial(coefficients)"));
            }
            finally
            {
                settings.ShowReturnTypeInScripting = oldReturn;
                settings.ShowParametersTypeInScripting = oldParameters;
            }
        }
    }
}
