using Computator.NET.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class FunctionDetailsUnitTests
    {
        [TestMethod]
        [DeploymentItem("Data", "Data")]
        public void TestLoading()
        {
            var array = FunctionsDetails.Details.ToArray();
            foreach (var keyValuePair in array)
            {
                Assert.IsNotNull(keyValuePair.Value);
                Assert.IsNotNull(keyValuePair.Value.Signature);
                Assert.IsNotNull(keyValuePair.Value.Type);
                Assert.IsNotNull(keyValuePair.Value.Category);
                Assert.IsNotNull(keyValuePair.Value.Description);
                Assert.IsNotNull(keyValuePair.Value.Title);

                Assert.IsNotNull(keyValuePair.Value.Url);

                Assert.AreEqual(keyValuePair.Key, keyValuePair.Value.Signature);
            }
        }

        [TestMethod]
        public void TestSavingWithEmpties()
        {
            FunctionsDetails.Details.SaveEmptyFunctionDetailsToXmlFile();
        }
    }

    [TestClass]
    public class EvaluationUnitTest
    {
    }
}