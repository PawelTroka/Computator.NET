using System.Reflection;
using Computator.NET.Compilation;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class NativeCompilerUnitTest
    {
        private NativeCompiler nativeCompiler;

        [SetUp]
        public void Init()
        {
            nativeCompiler = new NativeCompiler();
        }

        [Test]
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