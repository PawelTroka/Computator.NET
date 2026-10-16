using System;
using System.Linq;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Constants;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture, NonParallelizable]
    public class FunctionReferenceCategoryTests
    {
        [Test]
        public void EveryReferenceHasASubjectRatherThanAnImplementationTypeName()
        {
            foreach (var pair in new FunctionsDetailsFileSource().Details)
            {
                var category = pair.Value.Category;
                NUnit.Framework.Assert.That(string.IsNullOrWhiteSpace(category), Is.False, pair.Key);
                NUnit.Framework.Assert.That(category, Is.EqualTo(category.Trim()), pair.Key);
                NUnit.Framework.Assert.That(category.Replace(" ", ""),
                    Is.Not.EqualTo(pair.Value.Type).IgnoreCase, pair.Key);
            }
        }

        [TestCase(typeof(PhysicalConstants))]
        [TestCase(typeof(MathematicalConstants))]
        public void ConstantSubjectsPreserveTheIndependentRuntimeAttributeMetadata(Type type)
        {
            // This source reads CategoryAttribute from the actual engine members,
            // before the generated XML replaces their completion metadata.
            var runtime = new AutocompleteReflectionSource().GetFunctionsNamesWithDescription(type, true);
            var reference = new FunctionsDetailsFileSource().Details;
            NUnit.Framework.Assert.That(runtime, Is.Not.Empty);
            foreach (var item in runtime)
            {
                NUnit.Framework.Assert.That(string.IsNullOrWhiteSpace(item.Details.Category), Is.False, item.Text);
                NUnit.Framework.Assert.That(reference[item.Text].Category,
                    Is.EqualTo(item.Details.Category), item.Text);
            }
        }

        [TestCase("sin(x)", "Trigonometric functions")]
        [TestCase("ln(x)", "Logarithms")]
        [TestCase("gamma(x)", "Gamma and related functions")]
        [TestCase("Γ(z)", "Gamma and related functions")]
        [TestCase("BesselJ0(x)", "Bessel functions")]
        [TestCase("c", "Universal")]
        [TestCase("ElementaryCharge", "Electromagnetic constants")]
        [TestCase("PI", "Basic math constants")]
        [TestCase("i", "Complex mathematical constants")]
        public void BothCompletionModesExposeTheRestoredSubject(string signature, string category)
        {
            var provider = new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource());
            foreach (var items in new[] { provider.ExpressionAutocompleteItems, provider.ScriptingAutocompleteItems })
            {
                var matches = items.Where(item => item.Text == signature).ToArray();
                NUnit.Framework.Assert.That(matches, Is.Not.Empty, signature);
                foreach (var item in matches)
                    NUnit.Framework.Assert.That(item.Details.Category, Is.EqualTo(category), signature);
            }
        }
    }
}
