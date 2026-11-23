using System.Collections.Generic;

namespace Computator.NET.Engine.Symbolic;

/// <summary>A mathematical request, independent of C# execution and application UI.</summary>
public sealed class SymbolicRequest
{
    public string Operation { get; set; } = "simplify";
    public string Expression { get; set; } = "";
    public string Variable { get; set; } = "x";
    public string Point { get; set; } = "0";
    public string Direction { get; set; } = "both";
    public string Domain { get; set; } = "complex";
    public int Order { get; set; } = 1;
    public int Precision { get; set; } = 30;
    public string LowerBound { get; set; } = "";
    public string UpperBound { get; set; } = "";
    public Dictionary<string, string> Substitutions { get; set; } = new Dictionary<string, string>();
    public string[] Equations { get; set; } = new string[0];
    public string[] Variables { get; set; } = new string[0];
}
