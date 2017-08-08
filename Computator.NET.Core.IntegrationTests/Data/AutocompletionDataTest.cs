using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using NUnit.Framework;

namespace Computator.NET.Core.IntegrationTests.Data
{
    [TestFixture]
    public class AutocompletionDataTest
    {
        [Test]
        public void TestExpressions()
        {
            var content =
                (new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource()))
                .ExpressionAutocompleteItems;
            Assert.IsNotNull(content);
        }

        [Test]
        public void TestScripting()
        {
            var content = (new AutocompleteProvider(new AutocompleteReflectionSource(), new FunctionsDetailsFileSource()))
                .ScriptingAutocompleteItems;
            Assert.IsNotNull(content);
        }
    }
}