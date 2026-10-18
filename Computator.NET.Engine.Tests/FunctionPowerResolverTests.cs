using System.Reflection;
using System.Runtime.Loader;
using Computator.NET.Core.Compilation.TslHandlers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Computator.NET.Engine.Tests;

[TestFixture]
public class FunctionPowerResolverTests
{
    private static readonly Lazy<MetadataReference[]> RuntimeReferences = new(() =>
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToArray());

    [Test]
    public void AnInheritedExactCallableFromAnotherAssemblyWinsOverShorthand()
    {
        var dependency = Emit(CreateCompilation("""
            public class ExistingFunctions
            {
                protected static double cosʸ(double x) => 99;
            }
            """));
        const string source = """
            public class Entry : ExistingFunctions
            {
                static double cos(double x) => System.Math.Cos(x);
                static double pow(double x, double y) => System.Math.Pow(x, y);
                static double y = 2;
                public static double Run() => cosʸ(0.4);
            }
            """;

        NUnit.Framework.Assert.That(Evaluate(source, dependency), Is.EqualTo(99));
    }

    [Test]
    public void AConstructedTypeDoesNotMaskAFunctionPowerBesideIt()
    {
        const string source = """
            public class Entry
            {
                record Fⁿ(double Value);
                static double F(double x) => x + 1;
                static double pow(double x, double n) => System.Math.Pow(x, n);
                static double n = 2;
                public static double Run() => new Fⁿ(7).Value + Fⁿ(2);
            }
            """;

        NUnit.Framework.Assert.That(Evaluate(source), Is.EqualTo(16));
    }

    [TestCase("static double Fⁿ(double a, double b) => a + b;")]
    [TestCase("static double Fⁿ(string a) => 99;")]
    public void AnInvalidCompetingOverloadDoesNotMakeAValidSplitAmbiguous(string competingMethod)
    {
        var source = $$"""
            public class Entry
            {
                static double F(double x) => x + 1;
                {{competingMethod}}
                static double pow(double x, double n) => System.Math.Pow(x, n);
                static double nm = 2, m = 3;
                public static double Run() => Fⁿᵐ(2);
            }
            """;

        NUnit.Framework.Assert.That(Evaluate(source), Is.EqualTo(9));
    }

    [Test]
    public void TwoValidSplitsRemainACompilationError()
    {
        const string source = """
            public class Entry
            {
                static double F(double x) => x + 1;
                static double Fⁿ(double x) => x + 2;
                static double pow(double x, double n) => System.Math.Pow(x, n);
                static double nm = 2, m = 3;
                public static double Run() => Fⁿᵐ(2);
            }
            """;

        var errors = FunctionPowerResolver.Resolve(CreateCompilation(source)).GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        NUnit.Framework.Assert.That(errors.Any(diagnostic => diagnostic.Id == "CS0103" &&
            diagnostic.GetMessage().Contains("Fⁿᵐ", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void ACallToAnExactNameWithTheWrongArityKeepsItsOwnDiagnostic()
    {
        const string source = """
            public class Entry
            {
                static double F(double x) => x + 1;
                static double Fⁿ(double a, double b) => a + b;
                static double pow(double x, double n) => System.Math.Pow(x, n);
                static double n = 2;
                public static double Run() => Fⁿ(2);
            }
            """;

        var errors = FunctionPowerResolver.Resolve(CreateCompilation(source)).GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        NUnit.Framework.Assert.That(errors.Any(diagnostic => diagnostic.Id == "CS7036" &&
            diagnostic.GetMessage().Contains("Fⁿ", StringComparison.Ordinal)), Is.True);
    }

    [Test]
    public void AnExactMemberWinsAfterItsReceiverHasBeenRewritten()
    {
        const string source = """
            public class Entry
            {
                class Receiver
                {
                    public double F(double x) => x;
                    public double Fᵐ(double x) => 99;
                }
                static Receiver Make() => new Receiver();
                static Receiver pow(Receiver value, double exponent) => value;
                static double pow(double x, double n) => System.Math.Pow(x, n);
                static double n = 2, m = 3;
                public static double Run() => Makeⁿ().Fᵐ(2);
            }
            """;

        NUnit.Framework.Assert.That(Evaluate(source), Is.EqualTo(99));
    }

    [Test]
    public void ADynamicReceiverRetainsItsMemberNameAfterRewriting()
    {
        const string source = """
            public class Entry
            {
                class Receiver
                {
                    public double F(double x) => x;
                    public double Fᵐ(double x) => 99;
                }
                static Receiver Make() => new Receiver();
                static dynamic pow(Receiver value, double exponent) => value;
                static double pow(double x, double n) => System.Math.Pow(x, n);
                static double n = 2, m = 3;
                public static double Run() => Makeⁿ().Fᵐ(2);
            }
            """;

        NUnit.Framework.Assert.That(Evaluate(source), Is.EqualTo(99));
    }

    [Test]
    public void AReceiverInferredFromOnePowerCanBeUsedByASubsequentPower()
    {
        const string source = """
            public class Entry
            {
                static double sqrt(double x) => System.Math.Sqrt(x);
                static double pow(double x, double y) => System.Math.Pow(x, y);
                static double y = 2;
                public static double Run()
                {
                    var value = sqrtʸ(4);
                    return value.CompareToʸ(1);
                }
            }
            """;

        NUnit.Framework.Assert.That(Evaluate(source), Is.EqualTo(1));
    }

    [Test]
    public void AnInferredExponentCannotHideAnotherValidSplit()
    {
        const string source = """
            public class Entry
            {
                static double sqrt(double x) => System.Math.Sqrt(x);
                static double F(double x) => x + 1;
                static double Fⁿ(double x) => x + 2;
                static double pow(double x, double n) => System.Math.Pow(x, n);
                static double y = 2, m = 3;
                public static double Run()
                {
                    var nm = sqrtʸ(4);
                    return Fⁿᵐ(2);
                }
            }
            """;

        var errors = FunctionPowerResolver.Resolve(CreateCompilation(source)).GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        NUnit.Framework.Assert.That(errors.Any(diagnostic => diagnostic.Id == "CS0103" &&
            diagnostic.GetMessage().Contains("Fⁿᵐ", StringComparison.Ordinal)), Is.True);
    }

    [TestCase("sqrtʸ(4)", 4)]
    [TestCase("sqrtʸ(-4).Real", -4)]
    [TestCase("cosʸ(sqrtʸ(4))", 0.42724998309569323)]
    [TestCase("cosʸ(sqrt(4))", 0.17317818956819406)]
    [TestCase("sqrtʸ(value)", 4)]
    public void KnownMethodGroupsSupportPowersOfDynamicValues(string expression, double expected)
    {
        // The desktop sqrt(double) returns dynamic so a negative real input
        // can produce Complex. Its surrounding calls dispatch at runtime.
        var source = $$"""
            public class Entry
            {
                static dynamic sqrt(double x)
                {
                    if (x < 0) return System.Numerics.Complex.Sqrt(x);
                    return System.Math.Sqrt(x);
                }
                static System.Numerics.Complex sqrt(System.Numerics.Complex x) =>
                    System.Numerics.Complex.Sqrt(x);
                static double cos(double x) => System.Math.Cos(x);
                static System.Numerics.Complex cos(System.Numerics.Complex x) =>
                    System.Numerics.Complex.Cos(x);
                static double pow(double x, double y) => System.Math.Pow(x, y);
                static System.Numerics.Complex pow(System.Numerics.Complex x, System.Numerics.Complex y) =>
                    System.Numerics.Complex.Pow(x, y);
                static double y = 2;
                public static double Run()
                {
                    dynamic value = 4.0;
                    return {{expression}};
                }
            }
            """;

        NUnit.Framework.Assert.That(Evaluate(source), Is.EqualTo(expected).Within(1e-12));
    }

    [Test]
    public void AnExactCallableWithADynamicArgumentWinsOverShorthand()
    {
        const string source = """
            public class Entry
            {
                static double F(double x) => x + 1;
                static double Fʸ(double x) => 99;
                static double pow(double x, double y) => System.Math.Pow(x, y);
                static double y = 2;
                public static double Run()
                {
                    dynamic value = 2.0;
                    return Fʸ(value);
                }
            }
            """;

        NUnit.Framework.Assert.That(Evaluate(source), Is.EqualTo(99));
    }

    private static CSharpCompilation CreateCompilation(string source, byte[]? dependency = null) =>
        CSharpCompilation.Create("FunctionPowerTest_" + Guid.NewGuid().ToString("N"),
            new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp14)) },
            dependency == null ? RuntimeReferences.Value : RuntimeReferences.Value.Concat(new[]
            {
                MetadataReference.CreateFromImage(dependency)
            }), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static byte[] Emit(CSharpCompilation compilation)
    {
        using var image = new MemoryStream();
        var result = compilation.Emit(image);
        NUnit.Framework.Assert.That(result.Success, Is.True, string.Join(Environment.NewLine,
            result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
        return image.ToArray();
    }

    private static double Evaluate(string source, byte[]? dependency = null)
    {
        var image = Emit(FunctionPowerResolver.Resolve(CreateCompilation(source, dependency)));
        var context = new AssemblyLoadContext("FunctionPowerTest_" + Guid.NewGuid().ToString("N"), isCollectible: true);
        try
        {
            if (dependency != null)
            {
                using var dependencyStream = new MemoryStream(dependency);
                context.LoadFromStream(dependencyStream);
            }
            using var stream = new MemoryStream(image);
            var assembly = context.LoadFromStream(stream);
            return (double)assembly.GetType("Entry")!.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)!
                .Invoke(null, null)!;
        }
        finally
        {
            context.Unload();
        }
    }
}
