using Computator.NET.Core.Autocompletion;
using NUnit.Framework;

namespace Computator.NET.IntegrationTests.Data
{
    [TestFixture]
    public class FunctionDetailsTests
    {
        [Test]
        public void TestLoading()
        {
            var array = (new FunctionsDetails()).ToArray();
            foreach (var keyValuePair in array)
            {
                Assert.IsNotNull(keyValuePair.Value);
                Assert.IsNotNull(keyValuePair.Value.Signature);
                Assert.IsNotNull(keyValuePair.Value.Type);
                Assert.IsNotNull(keyValuePair.Value.Category);
                Assert.IsNotNull(keyValuePair.Value.Description);
                Assert.IsNotNull(keyValuePair.Value.Title);
                Assert.IsNotNull(keyValuePair.Value.Url);

                if(!string.IsNullOrWhiteSpace(keyValuePair.Value.Signature))
                    Assert.AreEqual(keyValuePair.Key, keyValuePair.Value.Signature);
            }
        }

        [Test]
        public void TestSavingWithEmpties()
        {
            (new FunctionsDetails()).SaveEmptyFunctionDetailsToXmlFile();
        }
    }
}