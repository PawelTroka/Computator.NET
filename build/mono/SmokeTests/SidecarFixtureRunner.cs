using System;
using System.IO;
using System.Reflection;

public sealed class SidecarFixtureRunner : MarshalByRefObject
{
    public override object InitializeLifetimeService() { return null; }

    public void Verify(string applicationDirectory)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var candidate = Path.Combine(applicationDirectory, new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
        };
        VerifyFactory(applicationDirectory);
    }

    private static void VerifyFactory(string applicationDirectory)
    {
        var directory = AppDomain.CurrentDomain.BaseDirectory;
        var name = "MonoChartingSidecarRegression-" + Guid.NewGuid().ToString("N");
        File.Copy(Path.Combine(applicationDirectory, "Computator.NET.Charting.dll"), Path.Combine(directory, "Computator.NET.Charting.dll"));
        foreach (var suffix in new[] { ".pdb", ".dll.config" }) File.WriteAllText(Path.Combine(directory, name + suffix), "Not a managed assembly.");
        var fallback = Computator.NET.DataTypes.RuntimeObjectFactory.CreateInstance<Computator.NET.DataTypes.Charts.IChartFactory>(name);
        if (!(fallback is Computator.NET.Charting.ChartFactory))
            throw new InvalidOperationException("Ignoring debug sidecars did not retain the production chart-factory fallback.");
        var hinted = Computator.NET.DataTypes.RuntimeObjectFactory.CreateInstance<Computator.NET.DataTypes.Charts.IChartFactory>("Charting");
        if (!(hinted is Computator.NET.Charting.ChartFactory))
            throw new InvalidOperationException("The ordinary charting assembly hint stopped working.");
    }
}
