// <copyright file="TslCompilerTest.cs" company="Pawel Troka">Copyright ©  2016 Pawel Troka</copyright>
using System;
using Computator.NET.Compilation;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Computator.NET.Compilation.Tests
{
    /// <summary>This class contains parameterized unit tests for TslCompiler</summary>
    [PexClass(typeof(TslCompiler))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class TslCompilerTest
    {
        /// <summary>Test stub for .ctor()</summary>
        [PexMethod]
        public TslCompiler ConstructorTest()
        {
            TslCompiler target = new TslCompiler();
            return target;
            // TODO: add assertions to method TslCompilerTest.ConstructorTest()
        }

        /// <summary>Test stub for TransformToCSharp(String)</summary>
        [PexMethod]
        public string TransformToCSharpTest([PexAssumeUnderTest]TslCompiler target, string tslCode)
        {
            string result = target.TransformToCSharp(tslCode);
            return result;
            // TODO: add assertions to method TslCompilerTest.TransformToCSharpTest(TslCompiler, String)
        }
    }
}
