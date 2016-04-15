using Computator.NET.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class AutocompletionDataTest
    {
        [TestMethod]
        public void TestScripting()
        {
            var content = AutocompletionData.GetAutocompleteItemsForScripting();
            Assert.IsNotNull(content);
        }

        [TestMethod]
        public void TestExpressions()
        {
            var content = AutocompletionData.GetAutocompleteItemsForExpressions();
            Assert.IsNotNull(content);
        }
    }
}