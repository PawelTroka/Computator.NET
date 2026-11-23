using Computator.NET.Engine.Symbolic;
using NUnit.Framework;
using Check = NUnit.Framework.Assert;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public sealed class SymbolicInequalityNotationTests
{
    [TestCase("x != 0", "-1", "True")]
    [TestCase("x != 0", "0", "False")]
    [TestCase("x != 0", "1", "True")]
    [TestCase("x ≠ 0", "-1", "True")]
    [TestCase("x ≠ 0", "0", "False")]
    [TestCase("x ≠ 0", "1", "True")]
    [TestCase("x! = 0", "0", "False")]
    [TestCase("x! = 1", "0", "True")]
    [TestCase("piecewise(5 provided x=0,3 provided x!=0)", "-1", "3")]
    [TestCase("piecewise(5 provided x=0,3 provided x!=0)", "0", "5")]
    [TestCase("(x+1) != sin(0) and x != 0", "-1", "False")]
    [TestCase("(x+1) != sin(0) and x != 0", "2", "True")]
    [TestCase("sin²(piecewise(PI/2 provided x!=0,0))", "-1", "1")]
    [TestCase("cos²(piecewise(PI/2 provided x≠0,0))", "0", "1")]
    public void NativeInequalityRetainsItsMeaningWithoutChangingFactorial(string expression, string value, string expected)
    {
        var result = new SymbolicEngine().Execute(new SymbolicRequest
        {
            Operation = "substitute", Expression = expression,
            Substitutions = new Dictionary<string, string> { ["x"] = value }
        });
        Check.That(result.Status, Is.EqualTo("exact"));
        Check.That(result.Result, Is.EqualTo(expected));
    }
}
