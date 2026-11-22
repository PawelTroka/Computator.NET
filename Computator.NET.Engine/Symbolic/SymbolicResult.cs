namespace Computator.NET.Engine.Symbolic;

/// <summary>Plain text and inert LaTeX for the same result, with its mathematical status.</summary>
public sealed class SymbolicResult
{
    public string Result { get; set; } = "";
    public string Latex { get; set; } = "";
    /// <summary>exact, approximate, unevaluated, or undefined.</summary>
    public string Status { get; set; } = "exact";
    public string[] Warnings { get; set; } = new string[0];
}
