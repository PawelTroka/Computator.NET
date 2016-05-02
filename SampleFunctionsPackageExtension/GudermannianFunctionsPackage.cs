using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Computator.NET.Extensibility;

namespace SampleFunctionsPackages
{
    [Export(typeof(IFunctionsPackage))]
    [ExportMetadata("IsScriptingOnly", false)]
    public class GudermannianFunctionsPackage : IFunctionsPackage
    {
        public string ToCode { get; } = @"
                    public static double GudermannianFunction(double x)//https://en.wikipedia.org/wiki/Gudermannian_function
                    {
                        return System.Math.Asin(System.Math.Tanh(x));
                    }";

        public IEnumerable<FunctionInfo> FunctionsInfo { get; } = new List<FunctionInfo>
        {
            new FunctionInfo
            {
                Category = "SampleExtension",
                Description =
                    "The Gudermannian function, named after Christoph Gudermann (1798–1852), relates the circular functions and hyperbolic functions without using complex numbers.",
                Signature = "GudermannianFunction(x)",
                Title = "Gudermannian function",
                Type = "SpecialFunctions",
                Url = "https://en.wikipedia.org/wiki/Gudermannian_function"
            }
        };

        public static double GudermannianFunction(double x) //https://en.wikipedia.org/wiki/Gudermannian_function
        {
            return Math.Asin(Math.Tanh(x));
        }
    }
}