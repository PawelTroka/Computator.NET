using System.Reflection;
using Computator.NET.Compilation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class NativeCompilerUnitTest
    {
        private NativeCompiler nativeCompiler;

        [TestInitialize]
        public void Init()
        {
            nativeCompiler = new NativeCompiler();
        }

        [TestMethod]
        public void Test1()
        {
            var assembly = nativeCompiler.Compile(@"using System;

namespace Testing
{
	public static class TestCase
	{
		public static int TestFunction()
		{
			return Math.Abs(-2)*2*2;
		}
	}
}");
            Assert.AreEqual(8,
                assembly.GetType("Testing.TestCase")
                    .GetMethod("TestFunction", BindingFlags.Public | BindingFlags.Static)
                    .Invoke(null, null));
        }
    }
}