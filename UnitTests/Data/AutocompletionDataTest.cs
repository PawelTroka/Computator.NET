using Computator.NET.Data;
using NUnit.Framework;

namespace UnitTests.Data
{
    [TestFixture]
    public class AutocompletionDataTest
    {
        [Test]
        public void TestExpressions()
        {
            var content = AutocompletionData.GetAutocompleteItemsForExpressions();
            Assert.IsNotNull(content);
        }

        [Test]
        public void TestScripting()
        {
            var content = AutocompletionData.GetAutocompleteItemsForScripting();
            Assert.IsNotNull(content);
        }
    }
}