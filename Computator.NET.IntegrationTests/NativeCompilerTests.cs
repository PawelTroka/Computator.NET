using System.Reflection;
using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.IntegrationTests
{
	[TestFixture]
	public class NativeCompilerTests
	{
		[SetUp]
		public void Init()
		{
			nativeCompiler = new NativeCompiler();
		}

		private NativeCompiler nativeCompiler;

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