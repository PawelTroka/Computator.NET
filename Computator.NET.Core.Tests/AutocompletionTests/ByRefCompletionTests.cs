using System.Linq;
using System.Runtime.InteropServices;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Properties;
using NUnit.Framework;

namespace Computator.NET.Core.Tests.AutocompletionTests
{
    [TestFixture, NonParallelizable]
    public class ByRefCompletionTests
    {
        private static class Signatures
        {
            public static void Produce(out long value) { value = 1; }
            public static void Change(ref long value) { value++; }
            public static void Observe(in long value) { }
            public static void OutputBuffer([Out] byte[] value) { }
        }

        [TestCase("Produce", "out")]
        [TestCase("Change", "ref")]
        [TestCase("Observe", "in")]
        public void ArgumentModifiersAreRetainedInInsertionAndTypedCaption(string method, string modifier)
        {
            var settings = Settings.Default;
            var oldReturn = settings.ShowReturnTypeInScripting;
            var oldParameters = settings.ShowParametersTypeInScripting;
            try
            {
                settings.ShowReturnTypeInScripting = settings.ShowParametersTypeInScripting = true;
                var item = new AutocompleteReflectionSource().GetFunctionsNamesWithDescription(typeof(Signatures))
                    .Single(candidate => candidate.Text.StartsWith(method + "("));
                item.IsScripting = true;
                NUnit.Framework.Assert.That(item.GetTextForReplace(), Is.EqualTo(method + "(" + modifier + " value)"));
                NUnit.Framework.Assert.That(item.MenuText, Is.EqualTo("void " + method + "(" + modifier + " integer value)"));
            }
            finally
            {
                settings.ShowReturnTypeInScripting = oldReturn;
                settings.ShowParametersTypeInScripting = oldParameters;
            }
        }

        [Test]
        public void InteropOutputAttributeOnAValueParameterDoesNotInsertAnOutKeyword()
        {
            var item = new AutocompleteReflectionSource().GetFunctionsNamesWithDescription(typeof(Signatures))
                .Single(candidate => candidate.Text.StartsWith("OutputBuffer("));
            NUnit.Framework.Assert.That(item.GetTextForReplace(), Is.EqualTo("OutputBuffer(value)"));
        }
    }
}
