using Computator.NET.Core.Compilation;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class ControlFlowMultiplicationTests
{
    [TestCase("lock(gate) write(1);")]
    [TestCase("lock(gate)\n    write(1);")]
    [TestCase("lock (gate) write(1);")]
    [TestCase("lock(gate) lock(other) write(1);")]
    [TestCase("lock(gate) if(ready) write(1);")]
    [TestCase("try { Run(); } catch(Exception error) when(IsExpected(error)) { Recover(); }")]
    [TestCase("try { Run(); } catch(Exception) when(ready) { Recover(); }")]
    [TestCase("try { Run(); } catch (Exception error) when (IsExpected(error)) { Recover(); }")]
    [TestCase("unsafe { fixed(int* pointer = values) Consume(pointer); }")]
    public void ControlHeadersDoNotAcquireMultiplicationOperators(string source)
    {
        NUnit.Framework.Assert.That(new TslCompiler().TransformToCSharp(source), Is.EqualTo(source));
    }

    [TestCase("lock(gate) write(System.Threading.Monitor.IsEntered(gate));")]
    [TestCase("lock(gate)\n    write(System.Threading.Monitor.IsEntered(gate));")]
    [TestCase("lock(gate) lock(gate) write(System.Threading.Monitor.IsEntered(gate));")]
    [TestCase("lock(gate) if(System.Threading.Monitor.IsEntered(gate)) write(true);")]
    public void EmbeddedLockBodiesExecuteUnderTheLockAndReleaseIt(string statement)
    {
        var source = "var gate = new object(); " + statement + " write(System.Threading.Monitor.IsEntered(gate));";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(source), Is.EqualTo("TrueFalse"));
    }

    [Test]
    public void LockExpressionAndBodyExecuteOnceAndRetainMathematicalNotation()
    {
        const string functions = """
            static object Gate = new object();
            static int Calls;
            static object GetGate() { Calls++; return Gate; }
            static real Probe(real x)
            {
                lock(GetGate()) return 2x+1;
            }
            """;
        const string script = "write(Probe(3)); write(Calls); write(System.Threading.Monitor.IsEntered(Gate));";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, functions), Is.EqualTo("71False"));
    }

    [TestCase("catch(InvalidOperationException error) when(error.Message == \"expected\")")]
    [TestCase("catch(InvalidOperationException) when(true)")]
    [TestCase("catch (InvalidOperationException error) when (error.Message == \"expected\")")]
    public void ExceptionFiltersRunAndSelectTheirHandler(string clause)
    {
        var script = "try { throw new InvalidOperationException(\"expected\"); } " + clause +
            " { write(\"handled\"); } finally { write(\":finally\"); }";
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script), Is.EqualTo("handled:finally"));
    }

    [Test]
    public void FiltersInCustomDefinitionsKeepTheirOrderAndSingleEvaluation()
    {
        const string functions = """
            static string Trace = "";
            static bool Filter(string marker, bool result) { Trace += marker; return result; }
            static real Probe()
            {
                try { throw new InvalidOperationException(); }
                catch(InvalidOperationException) when(Filter("A", false)) { return -1; }
                catch(InvalidOperationException) when(Filter("B", true)) { return 3²; }
                catch(InvalidOperationException) { return -2; }
            }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript("write(Probe()); write(Trace);", functions),
            Is.EqualTo("9AB"));
    }

    [Test]
    public void ThrowingFilterFallsThroughToTheNextCatch()
    {
        const string functions = "static bool Filter() { throw new Exception(); }";
        const string script = """
            try { throw new InvalidOperationException(); }
            catch(InvalidOperationException) when(Filter()) { write("wrong"); }
            catch(InvalidOperationException) { write("recovered"); }
            """;
        NUnit.Framework.Assert.That(new PortableEngine().ExecuteScript(script, functions), Is.EqualTo("recovered"));
    }

    [TestCase("for")]
    [TestCase("foreach")]
    [TestCase("while")]
    [TestCase("using")]
    [TestCase("if")]
    [TestCase("lock")]
    [TestCase("catch")]
    [TestCase("fixed")]
    public void EscapedKeywordFunctionsStillAllowImplicitMultiplication(string name)
    {
        var functions = "static real @" + name + "(real x) { return x+1; }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal("@" + name + "(2)3", customFunctions: functions),
            Is.EqualTo(9));
    }

    [TestCase("locking")]
    [TestCase("catcher")]
    [TestCase("fixedValue")]
    public void FunctionNamesBeginningWithAControlKeywordStillMultiply(string name)
    {
        var functions = "static real " + name + "(real x) { return x+1; }";
        NUnit.Framework.Assert.That(new PortableEngine().EvaluateReal(name + "(2)3", customFunctions: functions),
            Is.EqualTo(9));
    }
}
