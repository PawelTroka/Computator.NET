// <copyright file="TslCompilerTest.cs" company="Pawel Troka">Copyright ©  2016 Pawel Troka</copyright>

using System;
using Computator.NET.Compilation;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Computator.NET.Compilation.Tests
{
    [TestClass]
    [PexClass(typeof(TslCompiler))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class TslCompilerTest
    {

        [PexMethod(MaxConstraintSolverTime = 2)]
        public string TransformToCSharp([PexAssumeUnderTest]TslCompiler target, string tslCode)
        {
            string result = target.TransformToCSharp(tslCode);
            return result;
            // TODO: add assertions to method TslCompilerTest.TransformToCSharp(TslCompiler, String)
        }
    }
}
