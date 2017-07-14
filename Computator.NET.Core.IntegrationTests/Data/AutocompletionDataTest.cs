using Computator.NET.Core.Autocompletion;
using NUnit.Framework;

namespace Computator.NET.Core.IntegrationTests.Data
{
    [TestFixture]
    public class AutocompletionDataTest
    {
        [Test]
        public void TestExpressions()
        {
            var content = AutocompletionData.GetAutocompleteItemsForExpressions(new FunctionsDetails());
            Assert.IsNotNull(content);
        }

        [Test]
        public void TestScripting()
        {
            var content = AutocompletionData.GetAutocompleteItemsForScripting(new FunctionsDetails());
            Assert.IsNotNull(content);
        }
    }
}