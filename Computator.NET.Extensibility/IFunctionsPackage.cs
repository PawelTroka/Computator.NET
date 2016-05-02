using System.Collections.Generic;

namespace Computator.NET.Extensibility
{
    public interface IFunctionsPackage
    {
        string ToCode { get; }
        IEnumerable<FunctionInfo> FunctionsInfo { get; }
    }
}