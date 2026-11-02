using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.DataTypes.Charts;

internal static partial class Program
{
    private static void Native3DScriptPlots()
    {
        // Check the host API independently of Roslyn's generated source: their
        // preprocessor symbols are separate and formerly hid these overloads.
        var methods = typeof(ScriptingFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static);
        Check(methods.Any(m => m.Name == "plot" && m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType == typeof(Func<double,double,double>[]) &&
            m.GetParameters()[0].IsDefined(typeof(ParamArrayAttribute), false)), "Mono's reflected surface-array plot API is missing.");
        Check(typeof(ScriptingFunctions).GetMethod("plot", new[] { typeof(IEnumerable<double>), typeof(IEnumerable<double>), typeof(IEnumerable<double>) }) != null,
            "Mono's reflected x/y/z point plot API is missing.");
        var source = new FunctionsDetailsFileSource();
        var catalog = new AutocompleteProvider(new AutocompleteReflectionSource(), source).ScriptingAutocompleteItems;
        foreach (var signature in new[] { "plot(fxys1,fxys2, ...)", "plot(x,y,z)" })
        {
            var item = catalog.Single(i => i.Text == signature);
            Check(Object.ReferenceEquals(item.Details, source.Details[signature]), "Restored plot completion lost its existing reference.");
        }

        WithCompiledScriptPlot("plot(new System.Func<double,double,double>[] { (x,y) => x²+y² });", "mono-script-surface", chart =>
        {
            Check(chart.Mode == Chart3DMode.Surface, "One function must retain the surface mode.");
            VerifyScriptSurface(chart, 0, (x,y) => x*x+y*y);
        });
        WithCompiledScriptPlot("plot((x,y) => x+y, (x,y) => x-y);", "mono-script-multiple-functions", chart =>
        {
            Check(chart.Mode == Chart3DMode.Points, "Multiple functions must retain the existing points-mode convention.");
            VerifyScriptSurface(chart, 0, (x,y) => x+y);
            VerifyScriptSurface(chart, 1, (x,y) => x-y);
        });
        var range = "System.Linq.Enumerable.Range(0,81)";
        var xValues = "System.Linq.Enumerable.Select(" + range + ", i => (double)(i%9-4))";
        var yValues = "System.Linq.Enumerable.Select(" + range + ", i => (double)(i/9-4))";
        var zValues = "System.Linq.Enumerable.Select(" + range + ", i => (double)((i%9-4)*(i%9-4)+(i/9-4)*(i/9-4)))";
        WithCompiledScriptPlot("plot(" + xValues + "," + yValues + "," + zValues + ");", "mono-script-xyz-points", chart =>
        {
            Check(chart.Mode == Chart3DMode.Points && chart.FiniteSurfaceSamples == 0, "Point-only script output must be a real point plot.");
            var series = (List<List<Point3D>>)typeof(Chart3DControl).GetField("_points", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(chart);
            Check(series.Count == 1 && series[0].Count == 81, "Point plot lost coordinates.");
            for (var index = 0; index < 81; index++)
            {
                var x = index % 9 - 4; var y = index / 9 - 4;
                Near(series[0][index].x, x, 0); Near(series[0][index].y, y, 0); Near(series[0][index].z, x*x+y*y, 0);
            }
        });

        var output = new StringBuilder();
        var definitions = "static int Reads=0; static System.Collections.Generic.IEnumerable<double> Unbounded() { while(true) { Reads++; yield return double.NaN; } }";
        var bounded = new ScriptEvaluator().Evaluate("try { plot(Unbounded(),Unbounded(),Unbounded()); } catch(System.ArgumentOutOfRangeException) { write(Reads); }", definitions);
        bounded.Evaluate(text => output.Append(text));
        Equal(output.ToString(), "60003"); // three iterators, exactly 20,001 attempted triples
        Check(!Application.OpenForms.Cast<Form>().Any(f => f.Controls.Find("chart3D", true).Length != 0), "Rejected inputs opened a plot window.");
        output.Clear();
        var invalid = "try { plot((System.Func<double,double,double>[])null); } catch(System.ArgumentNullException) { write(1); }" +
            "try { plot(new System.Func<double,double,double>[0]); } catch(System.ArgumentException) { write(2); }" +
            "try { plot((System.Collections.Generic.IEnumerable<double>)null,new double[0],new double[0]); } catch(System.ArgumentNullException) { write(3); }";
        new ScriptEvaluator().Evaluate(invalid, "").Evaluate(text => output.Append(text)); Equal(output.ToString(), "123");
        Console.WriteLine("  compiled 3D scripts: surface, multiple-functions points, xyz points, bounded infinite input and invalid arguments passed");
    }

    private static void VerifyScriptSurface(Chart3DControl chart, int index, Func<double,double,double> expected)
    {
        var surfaces = ((IEnumerable)typeof(Chart3DControl).GetField("_surfaces", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(chart)).Cast<object>().ToArray();
        Check(surfaces.Length > index, "A compiled script did not create the expected sampled surface.");
        var grid = surfaces[index];
        var x = (double[])grid.GetType().GetField("X", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(grid);
        var y = (double[])grid.GetType().GetField("Y", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(grid);
        var z = (double[])grid.GetType().GetField("Z", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(grid);
        Check(x.Length >= 17 && x.Length <= 97 && y.Length == x.Length && z.Length == x.Length*y.Length, "The renderer's bounded surface grid was not retained.");
        for (var row = 0; row < y.Length; row++)
            for (var column = 0; column < x.Length; column++) Near(z[row*x.Length+column], expected(x[column],y[row]), 1e-10);
    }

    private static void WithCompiledScriptPlot(string code, string name, Action<Chart3DControl> inspect)
    {
        var previous = new HashSet<Form>(Application.OpenForms.Cast<Form>());
        try
        {
            var script = new ScriptEvaluator().Evaluate(code, "");
            script.Evaluate(_ => { });
            Application.DoEvents();
            var windows = Application.OpenForms.Cast<Form>().Where(f => !previous.Contains(f)).ToArray();
            Check(windows.Length == 1, "A compiled plotting script must open exactly one real plot window.");
            var window = windows[0]; window.Size = new Size(760,560); window.Location = new Point(100,100);
            var chart = window.Controls.Find("chart3D", true).OfType<Chart3DControl>().Single();
            inspect(chart);
            using (var export = new Bitmap(chart.GetImage(700,480)))
            {
                Check(CountScriptPlotColors(export) > 150, "A script plot exported blank/axes-only geometry.");
                export.Save(Path.Combine(outputDirectory, name + "-export.png"), ImageFormat.Png);
            }
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var painted = false;
            while (!painted && timer.ElapsedMilliseconds < 5000)
            {
                chart.Refresh(); Application.DoEvents(); Thread.Sleep(20);
                using (var visible = CaptureVisibleChart(chart, name + "-visible.png")) painted = CountScriptPlotColors(visible) > 150;
            }
            Check(painted, "A compiled plotting script did not paint real nonblank X11 pixels.");
        }
        finally
        {
            foreach (var window in Application.OpenForms.Cast<Form>().Where(f => !previous.Contains(f)).ToArray()) { window.Close(); window.Dispose(); }
            Application.DoEvents();
        }
    }

    private static int CountScriptPlotColors(Bitmap bitmap)
    {
        var count = 0;
        for (var y = 0; y < bitmap.Height; y += 2)
            for (var x = 0; x < bitmap.Width; x += 2)
            {
                var c = bitmap.GetPixel(x,y);
                if (Math.Max(c.R,Math.Max(c.G,c.B))-Math.Min(c.R,Math.Min(c.G,c.B)) > 60) count++;
            }
        return count;
    }
}
