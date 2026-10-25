using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Computator.NET.Charting.Chart3D.UI;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Charting.RealCharting;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Abstract.Views;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Menu.Commands.FileCommands;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.Core.Services.ErrorHandling;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Configuration;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using Computator.NET.Engine;
using Settings = Computator.NET.Core.Properties.Settings;
using File = System.IO.File;

internal static partial class Program
{
    private sealed class Result
    {
        public string Name;
        public double Seconds;
        public Exception Failure;
        public string SkippedReason;
    }

    private sealed class KnownLimitationException : Exception
    {
        public KnownLimitationException(string message) : base(message) { }
    }

    private sealed class RecordingErrors : IErrorHandler
    {
        public readonly List<string> Messages = new List<string>();
        public void DisplayError(string message, string title) { Messages.Add(title + ": " + message); }
        public void LogError(string message, ErrorType errorType, Exception ex) { }
        public void AssertEmpty()
        {
            Check(Messages.Count == 0, "Desktop displayed errors: " + string.Join(Environment.NewLine, Messages));
        }
    }

    private static readonly List<Result> Results = new List<Result>();
    private static string outputDirectory;

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: mono Computator.NET.Mono.SmokeTests.exe /absolute/path/results");
            return 2;
        }
        outputDirectory = Path.GetFullPath(args[0]);
        Directory.CreateDirectory(outputDirectory);
        Environment.SetEnvironmentVariable("MONO_WINFORMS_XIM_STYLE", "disabled");
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
        Application.SetCompatibleTextRenderingDefault(false);
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

        Run("mono-runtime-and-shared-source-graph", RuntimeAndSharedGraph);
        Run("runtime-factory-ignores-debug-sidecars", RuntimeFactorySidecars);
        Run("roslyn-dynamic-and-numerical-packages", ModernCompilerAndPackages);
        Run("portable-real-notation-and-complex-evaluation", PortableExpressions);
        Run("portable-script-and-custom-function-isolation", PortableScriptsAndDefinitions);
        Run("portable-diagnostics-and-recovery", PortableDiagnostics);
        Run("shared-numerical-integration-derivative-and-root", SharedNumericalMethods);
        Run("framework-ieee-adjacent-values", FrameworkAdjacentValues);
        Run("framework-asinh-and-copy-sign", FrameworkElementaryValues);
        Run("framework-random-inclusive-bounds-and-concurrency", FrameworkRandomBounds);
        Run("settings-json-and-windows-editor-migration", SettingsMigration);
        Run("legacy-nrbf-import-on-mono", LegacySettingsMigration);
        Run("desktop-real-and-complex-expression-evaluation", DesktopExpressions);
        Run("desktop-script-and-custom-functions", DesktopScripts);
        Run("desktop-numerical-method-catalog", NumericalMethodCatalog);
        Run("native-gsl-generated-import-and-error-recovery", NativeGsl);
        Run("mono-function-reference-readable-text", FunctionReference);
        Run("unicode-editor-documents-and-renaming", EditorDocuments);
        Run("unshown-editor-tabs-have-backed-documents", UnshownEditorDocuments);
        Run("desktop-visible-custom-functions-file-and-script-workflow", DesktopWorkflow);
        Run("unix-document-save-and-alias-protection", UnixDocumentSafety);
        Run("real-chart-samples-and-png-rendering", RealChart);
        Run("complex-chart-png-and-domain-colors", ComplexChart);
        Run("unsupported-3d-is-explicit", Unsupported3D);
        WriteResults();
        var failed = Results.Count(result => result.Failure != null);
        var skipped = Results.Count(result => result.SkippedReason != null);
        Console.WriteLine("Mono acceptance: {0} passed, {1} failed, {2} skipped (documented limitations).", Results.Count - failed - skipped, failed, skipped);
        return failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Action test)
    {
        var result = new Result { Name = name };
        var watch = Stopwatch.StartNew();
        try { test(); }
        catch (KnownLimitationException known) { result.SkippedReason = known.Message; }
        catch (Exception error) { result.Failure = error; }
        result.Seconds = watch.Elapsed.TotalSeconds;
        Results.Add(result);
        Console.WriteLine("{0} {1} ({2:F2}s)", result.SkippedReason != null ? "SKIP" : result.Failure == null ? "PASS" : "FAIL", name, result.Seconds);
        if (result.Failure != null) Console.Error.WriteLine(result.Failure);
        if (result.SkippedReason != null) Console.WriteLine("  " + result.SkippedReason);
    }

    private static void RuntimeAndSharedGraph()
    {
        Check(Type.GetType("Mono.Runtime") != null, "Acceptance must run on Mono, not dotnet or Windows Framework.");
        Same(typeof(TslCompiler).Assembly, typeof(PortableEngine).Assembly, "TSL compiler must be the shared Engine implementation.");
        Same(typeof(Computator.NET.Core.NumericalCalculations.Integral).Assembly, typeof(PortableEngine).Assembly,
            "Desktop and portable numerical methods must share one implementation.");
        foreach (var assembly in new[] { typeof(MainView).Assembly, typeof(Chart2D).Assembly, typeof(ExpressionsEvaluator).Assembly, typeof(PortableEngine).Assembly })
        {
            var forbidden = assembly.GetReferencedAssemblies().Where(reference => new[]
                { "PresentationFramework", "PresentationCore", "WindowsFormsIntegration", "Scintilla5.NET", "ICSharpCode.AvalonEdit" }.Contains(reference.Name));
            Check(!forbidden.Any(), assembly.GetName().Name + " references unsupported Windows UI assemblies.");
        }
        Check(typeof(Chart2D).BaseType.Assembly.GetName().Name == "DataVisualizationX", "2D charts must use the recovered Unix chart implementation.");
    }

    private static void ModernCompilerAndPackages()
    {
        using (var compiler = new NativeCompiler())
        {
            var assembly = compiler.Compile(@"public class Probe {
                public int Value { get; set => field = value < 0 ? 0 : value; }
                public static double Run() {
                    dynamic item = new Probe { Value = 39 };
                    return item.Value + MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.CreateIdentity(2).Determinant()
                        + Meta.Numerics.Functions.AdvancedMath.Gamma(3.0);
                }
            }");
            Near(Convert.ToDouble(assembly.GetType("Probe").GetMethod("Run").Invoke(null, null)), 42, 1e-12);
        }
    }

    private static void PortableExpressions()
    {
        var engine = new PortableEngine();
        Near(engine.EvaluateReal("2x+3", 7), 17, 1e-12);
        Near(engine.EvaluateReal("x²+y²", 3, 4), 25, 1e-12);
        Near(engine.EvaluateReal("sqrt(9)+|x|", -2), 5, 1e-12);
        Near(engine.EvaluateReal("2·cos(0)"), 2, 1e-12);
        Near(engine.EvaluateReal("sin(PI/2)"), 1, 1e-12);
        Near((engine.EvaluateComplex("z²", new Complex(1, 1)) - new Complex(0, 2)).Magnitude, 0, 1e-12);
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pl-PL");
            Near(engine.EvaluateReal("1.5/2"), .75, 1e-12);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    private static void PortableScriptsAndDefinitions()
    {
        var engine = new PortableEngine();
        Equal(engine.ExecuteScript("var f(real x)=x²; write(f(3)); write(Probe(1));", "static real Probe(real x) { return x+1; }"), "92");
        Near(engine.EvaluateReal("Probe(2)", customFunctions: "static real Probe(real x) { return x/2; }"), 1, 1e-12);
        Near(engine.EvaluateReal("Probe(2)", customFunctions: "static real Probe(real x) { return x+41; }"), 43, 1e-12);
        Expect<EngineCompilationException>(() => engine.EvaluateReal("Probe(2)"));
    }

    private static void PortableDiagnostics()
    {
        var engine = new PortableEngine();
        var error = Expect<EngineCompilationException>(() => engine.EvaluateReal("missing(1)"));
        Check(error.Diagnostics.Any(item => item.File == "expression.tsl" && item.Line == 1), "Expression error must retain its source line.");
        error = Expect<EngineCompilationException>(() => engine.EvaluateReal("Probe(1)", customFunctions: "static real Probe(real x) { return missing; }"));
        Check(error.Diagnostics.Any(item => item.File == "custom-functions.tslf"), "Custom-function diagnostic must retain its document.");
        Near(engine.EvaluateReal("2+2"), 4, 1e-12);
    }

    private static void SharedNumericalMethods()
    {
        var engine = new PortableEngine();
        Near(engine.Integrate("x²", 0, 1, 1000), 1.0 / 3, 1e-10);
        Near(engine.Differentiate("sin(x)", 0), 1, 1e-6);
        Near(engine.FindRoot("x²-2", 0, 2), Math.Sqrt(2), 1e-7);
        Near(engine.Integrate("Probe(x)", 0, 1, 1000, "static real Probe(real x) { return 3*x²; }"), 1, 1e-10);
    }

    private static void DesktopExpressions()
    {
        GSLInitializer.Initialize();
        var evaluator = new ExpressionsEvaluator();
        Near(evaluator.Evaluate("2x+3", "", CalculationsMode.Real).Evaluate(7.0), 17, 1e-12);
        Near(evaluator.Evaluate("sqrt(9)+|x|", "", CalculationsMode.Real).Evaluate(-2.0), 5, 1e-12);
        Near(evaluator.Evaluate("Probe(x)", "static real Probe(real x) { return x²+1; }", CalculationsMode.Real).Evaluate(3.0), 10, 1e-12);
        Near((evaluator.Evaluate("z²", "", CalculationsMode.Complex).Evaluate(new Complex(1, 1)) - new Complex(0, 2)).Magnitude, 0, 1e-12);
        // Scalar f(x,y) calculations remain useful even though their WPF chart cannot render.
        Near(evaluator.Evaluate("x²+y²", "", CalculationsMode.Fxy).Evaluate(3.0, 4.0), 25, 1e-12);
        Expect<CompilationException>(() => evaluator.Evaluate("missing(1)", "", CalculationsMode.Real));
        Near(evaluator.Evaluate("2+2", "", CalculationsMode.Real).Evaluate(0.0), 4, 1e-12);
    }

    private static void DesktopScripts()
    {
        GSLInitializer.Initialize();
        var output = new StringBuilder();
        var script = new ScriptEvaluator().Evaluate("var f(real x)=x²; write(f(3)); write(Probe(1));", "static real Probe(real x) { return x+1; }");
        script.Evaluate(text => output.Append(text));
        Equal(output.ToString(), "92");
    }

    private static void NativeGsl()
    {
        GSLInitializer.Initialize();
        var native = typeof(SpecialFunctions).GetMethod("gsl_sf_gamma_inc_P", BindingFlags.Static | BindingFlags.NonPublic);
        Check(native != null, "GSL incomplete-gamma wrapper is missing.");
        var generated = new ExpressionsEvaluator().Evaluate("gsl_sf_gamma_inc_P(x, 1)", "", CalculationsMode.Real);
        var previous = GslErrorBoundary.ThrowOnError;
        try
        {
            GslErrorBoundary.ThrowOnError = false;
            Check(double.IsNaN((double)native.Invoke(null, new object[] { -1.0, 1.0 })), "Native domain error must return NaN without aborting Mono.");
            Check(double.IsNaN(generated.Evaluate(-1.0)), "Generated GSL domain error must return NaN.");
            Near(generated.Evaluate(1.0), 1 - Math.Exp(-1), 1e-12);
            GslErrorBoundary.ThrowOnError = true;
            var error = Expect<TargetInvocationException>(() => native.Invoke(null, new object[] { -1.0, 1.0 }));
            Check(error.InnerException is ArithmeticException, "GSL error must cross the managed boundary safely.");
            Near((double)native.Invoke(null, new object[] { 1.0, 1.0 }), 1 - Math.Exp(-1), 1e-12);
            Near(generated.Evaluate(1.0), 1 - Math.Exp(-1), 1e-12);
        }
        finally { GslErrorBoundary.ThrowOnError = previous; }
    }

    private static void EditorDocuments()
    {
        var bootstrapper = new WinFormsBootstrapper();
        using (bootstrapper.Container)
        using (var editor = bootstrapper.Create<TextEditorCodeEditor>())
        {
            Check(editor.Handle != IntPtr.Zero, "TextEditor requires a real native control.");
            var documentEditor = (ICodeEditorControl)editor;
            documentEditor.NewDocument("first.tsl");
            documentEditor.Text = "var α = π + 2·3; // zażółć gęślą jaźń";
            documentEditor.NewDocument("second.tsl");
            documentEditor.Text = "write(42);";
            documentEditor.SwitchDocument("first.tsl");
            Equal(documentEditor.Text, "var α = π + 2·3; // zażółć gęślą jaźń");
            documentEditor.RenameDocument("first.tsl", "renamed.tsl");
            Check(documentEditor.ContainsDocument("renamed.tsl") && !documentEditor.ContainsDocument("first.tsl"), "Editor document rename was not preserved.");
            documentEditor.SwitchDocument("second.tsl");
            Equal(documentEditor.Text, "write(42);");
        }
    }

    private static void UnshownEditorDocuments()
    {
        var bootstrapper = new WinFormsBootstrapper();
        using (bootstrapper.Container)
        using (var editor = new CodeEditorControlWrapper(bootstrapper.Create<TextEditorCodeEditor>()))
        {
            // Mono can select an inserted tab before its parent is visible
            // without raising SelectedIndexChanged. Every displayed tab must
            // still own a buffer, including blank tabs left in the background.
            var first = editor.CurrentFileName;
            Check(editor.ContainsDocument(first), "The initial untitled tab has no backing document before Show.");
            editor.NewDocument();
            var second = editor.CurrentFileName;
            Check(second != first && editor.ContainsDocument(second), "A new untitled tab has no backing document before Show.");
            Check(editor.ConfirmCloseDocuments(), "Clean untitled tabs unexpectedly prevented close approval.");
            editor.Text = "write(144);";
            editor.SwitchTab(first);
            Equal(editor.Text, string.Empty);
            editor.SwitchTab(second);
            Equal(editor.Text, "write(144);");
        }
    }

    private static void DesktopWorkflow()
    {
        GSLInitializer.Initialize();
        var previousEditor = Settings.Default.CodeEditor;
        var previousScripts = Settings.Default.ScriptingDirectory;
        var previousFunctions = Settings.Default.CustomFunctionsDirectory;
        var directory = Path.Combine(outputDirectory, "documents");
        Directory.CreateDirectory(directory);
        // Simulate a profile copied from Windows, whose selected WPF editor is unavailable.
        Settings.Default.CodeEditor = (CodeEditorType)1;
        Settings.Default.ScriptingDirectory = directory;
        Settings.Default.CustomFunctionsDirectory = directory;
        var errors = new RecordingErrors();
        var bootstrapper = new WinFormsBootstrapper();
        try
        {
            using (bootstrapper.Container)
            {
                bootstrapper.RegisterInstance<IErrorHandler>(errors);
                using (var window = bootstrapper.Create<MainView>())
                {
                    window.Show();
                    Application.DoEvents();
                    var custom = bootstrapper.Create<CustomFunctionsView>();
                    var scripting = bootstrapper.Create<ScriptingView>();
                    var state = bootstrapper.Create<ISharedViewState>();
                    VerifyDesktopInputLayout(window, state, bootstrapper.Create<NumericalCalculationsView>());
                    Same(custom.FindForm(), window, "Custom-function editor must be the displayed editor.");
                    Same(scripting.FindForm(), window, "Scripting editor must be displayed in MainView.");
                    VerifyNestedScriptTree(scripting, directory);
                    var path = Path.Combine(directory, "visible-functions.tslf");
                    File.WriteAllText(path, "static real Probe(real x) { return x+42; }");
                    custom.CustomFunctionsEditor.NewDocument(path);
                    custom.CustomFunctionsEditor.SwitchTab(path);
                    var expression = bootstrapper.Create<ITextProvider>();
                    expression.Text = "Probe(0)";
                    state.CurrentView = ViewName.Calculations;
                    state.DefaultActions[ViewName.Calculations](window, EventArgs.Empty);
                    errors.AssertEmpty();
                    Equal(Find(window, "calculationValueTextBox").Text, "42");
                    custom.CustomFunctionsEditor.Text = "static real Probe(real x) { return x+73; }";
                    state.DefaultActions[ViewName.Calculations](window, EventArgs.Empty);
                    errors.AssertEmpty();
                    Equal(Find(window, "calculationValueTextBox").Text, "73");
                    scripting.CodeEditorView.Text = "write(Probe(0));";
                    state.DefaultActions[ViewName.Scripting](window, EventArgs.Empty);
                    errors.AssertEmpty();
                    Check(Find(window, "consoleOutputTextBox").Text.EndsWith("73", StringComparison.Ordinal), "Visible script did not receive unsaved custom-function edits.");
                    state.CurrentView = ViewName.CustomFunctions;
                    bootstrapper.Create<SaveCommand>().Execute();
                    Equal(File.ReadAllText(path), custom.CustomFunctionsEditor.Text);
                    custom.CustomFunctionsEditor.RemoveTab(path);
                    custom.CustomFunctionsEditor.NewDocument(path);
                    custom.CustomFunctionsEditor.SwitchTab(path);
                    Equal(custom.CustomFunctionsEditor.Text, "static real Probe(real x) { return x+73; }");
                    // This acceptance workflow intentionally edited the named
                    // script above. Save it explicitly before normal shutdown;
                    // an unattended test must not discard a real dirty buffer.
                    state.CurrentView = ViewName.Scripting;
                    bootstrapper.Create<SaveCommand>().Execute();
                    Equal(File.ReadAllText(scripting.CodeEditorView.CurrentFileName), scripting.CodeEditorView.Text);
                    Capture(window, "desktop-workflow.png");
                    window.Close();
                    Application.DoEvents();
                    Check(window.IsDisposed, "The saved desktop workflow did not close normally.");
                }
            }
        }
        finally
        {
            Settings.Default.CodeEditor = previousEditor;
            Settings.Default.ScriptingDirectory = previousScripts;
            Settings.Default.CustomFunctionsDirectory = previousFunctions;
        }
    }

    private static void RealChart()
    {
        using (var chart = new Chart2D { Width = 640, Height = 400 })
        {
            chart.SetChartAreaValues(-2, 2, -1, 5);
            chart.AddFunction(new ExpressionsEvaluator().Evaluate("x²", "", CalculationsMode.Real));
            Check(chart.Series.Count == 1 && chart.Series[0].Points.Count > 20, "Real function did not produce a sampled series.");
            foreach (var point in chart.Series[0].Points)
                Near(point.YValues[0], point.XValue * point.XValue, 1e-12);
            using (var image = chart.GetImage(640, 400))
            {
                Check(image.Width == 640 && image.Height == 400, "2D export size is wrong.");
                AssertImageHasVariation(image, 5);
                image.Save(Path.Combine(outputDirectory, "real-chart.png"), ImageFormat.Png);
            }
        }
    }

    private static void ComplexChart()
    {
        using (var chart = new Computator.NET.Charting.ComplexCharting.ComplexChart { Width = 400, Height = 320, Quality = 50 })
        {
            chart.SetChartAreaValues(-2, 2, -2, 2);
            chart.AddFunction(new ExpressionsEvaluator().Evaluate("z²", "", CalculationsMode.Complex));
            using (var image = chart.GetImage(400, 320))
            {
                Check(image.Width == 400 && image.Height == 320, "Complex export size is wrong.");
                AssertImageHasVariation(image, 40);
                image.Save(Path.Combine(outputDirectory, "complex-chart.png"), ImageFormat.Png);
            }
            chart.ClearAll();
        }
    }

    private static void Unsupported3D()
    {
        using (var chart = new Chart3DControl { Width = 600, Height = 200 })
        {
            Check(chart is Control, "Mono 3D placeholder must be WinForms.");
            var function = new ExpressionsEvaluator().Evaluate("x+y", "", CalculationsMode.Fxy);
            Expect<PlatformNotSupportedException>(() => chart.AddFunction(function));
            Expect<PlatformNotSupportedException>(() => chart.GetImage(640, 400));
            var exportPath = Path.Combine(outputDirectory, "unsupported-3d-must-not-exist.png");
            Expect<PlatformNotSupportedException>(() => chart.SaveImage(exportPath, ImageFormat.Png));
            Check(!File.Exists(exportPath), "Unsupported 3D must not report a successful blank export.");
            Capture(chart, "unsupported-3d.png");
        }
    }

    private static void Capture(Control control, string name)
    {
        using (var image = new Bitmap(control.Width, control.Height))
        {
            if (control is Form && control.Visible)
            {
                control.Refresh();
                Application.DoEvents();
                using (var graphics = Graphics.FromImage(image))
                    graphics.CopyFromScreen(control.Location, Point.Empty, image.Size);
                AssertImageHasVariation(image, 16);
            }
            else control.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(outputDirectory, name), ImageFormat.Png);
        }
    }

    private static void AssertImageHasVariation(Image image, int minimumColors)
    {
        using (var bitmap = new Bitmap(image))
        {
            var colors = new HashSet<int>();
            for (var x = 0; x < bitmap.Width; x += 7)
                for (var y = 0; y < bitmap.Height; y += 7)
                    colors.Add(bitmap.GetPixel(x, y).ToArgb());
            Check(colors.Count >= minimumColors, "Rendered image has only " + colors.Count + " colors; expected plotted content.");
        }
    }

    private static Control Find(Control root, string name) { return root.Controls.Find(name, true).Single(); }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Same(object actual, object expected, string message) { Check(ReferenceEquals(actual, expected), message); }
    private static void Equal(string actual, string expected) { Check(actual == expected, "Expected '" + expected + "', got '" + actual + "'."); }
    private static void Near(double actual, double expected, double tolerance)
    {
        Check(!double.IsNaN(actual) && !double.IsInfinity(actual) && Math.Abs(actual - expected) <= tolerance,
            string.Format(CultureInfo.InvariantCulture, "Expected {0:R} ± {1:R}, got {2:R}.", expected, tolerance, actual));
    }

    private static T Expect<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T expected) { return expected; }
        throw new InvalidOperationException("Expected " + typeof(T).FullName + ".");
    }

    private static void WriteResults()
    {
        using (var writer = XmlWriter.Create(Path.Combine(outputDirectory, "mono-results.xml"), new XmlWriterSettings { Indent = true }))
        {
            writer.WriteStartElement("testsuite");
            writer.WriteAttributeString("name", "Computator.NET.Mono.Acceptance");
            writer.WriteAttributeString("tests", Results.Count.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("failures", Results.Count(result => result.Failure != null).ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("skipped", Results.Count(result => result.SkippedReason != null).ToString(CultureInfo.InvariantCulture));
            foreach (var result in Results)
            {
                writer.WriteStartElement("testcase");
                writer.WriteAttributeString("name", result.Name);
                writer.WriteAttributeString("time", result.Seconds.ToString("F3", CultureInfo.InvariantCulture));
                if (result.SkippedReason != null)
                {
                    writer.WriteStartElement("skipped");
                    writer.WriteAttributeString("message", result.SkippedReason);
                    writer.WriteEndElement();
                }
                if (result.Failure != null)
                {
                    writer.WriteStartElement("failure");
                    writer.WriteAttributeString("message", result.Failure.Message);
                    writer.WriteString(result.Failure.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
