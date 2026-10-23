using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Computator.NET.Core.Configuration;
using Computator.NET.Core.Compatibility;
using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Autocompletion.DataSource;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Functions;
using Computator.NET.Core.Model;
using Computator.NET.Core.Natives;
using Computator.NET.Core.NumericalCalculations;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.SettingsTypes;
using Computator.NET.Desktop.Controls;
using Computator.NET.Desktop.Controls.CodeEditors;
using Computator.NET.Desktop.Views;
using Computator.NET.Engine.Compatibility;
using File = System.IO.File;

internal static partial class Program
{
    private static void RuntimeFactorySidecars()
    {
        // A unique hint matches only sidecars, making the old loader fail
        // deterministically rather than depending on directory enumeration order.
        var hint = "MonoChartingSidecarRegression-" + Guid.NewGuid().ToString("N");
        var paths = new[] { ".pdb", ".dll.config" }.Select(extension =>
            Path.Combine(Computator.NET.DataTypes.Configuration.AppInformation.Directory, hint + extension)).ToArray();
        try
        {
            foreach (var path in paths) File.WriteAllText(path, "Not a managed assembly.");
            var fallback = RuntimeObjectFactory.CreateInstance<Computator.NET.DataTypes.Charts.IChartFactory>(hint);
            Check(fallback != null, "Ignoring debug sidecars must retain the real chart-factory fallback.");
            Same(fallback.GetType().Assembly, typeof(Computator.NET.Charting.ChartFactory).Assembly,
                "Runtime discovery must select the production chart factory.");
            var hinted = RuntimeObjectFactory.CreateInstance<Computator.NET.DataTypes.Charts.IChartFactory>("Charting");
            Check(hinted is Computator.NET.Charting.ChartFactory, "The ordinary Charting assembly hint stopped working.");
        }
        finally { foreach (var path in paths) if (File.Exists(path)) File.Delete(path); }
    }

    private static void FrameworkAdjacentValues()
    {
        // Independently specified IEEE-754 bit patterns, including signed zero.
        Bits(FrameworkMath.BitIncrement(1), 0x3ff0000000000001L);
        Bits(FrameworkMath.BitDecrement(1), 0x3fefffffffffffffL);
        Bits(FrameworkMath.BitIncrement(-1), unchecked((long)0xbfefffffffffffffUL));
        Bits(FrameworkMath.BitDecrement(-1), unchecked((long)0xbff0000000000001UL));
        var negativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
        Bits(FrameworkMath.BitIncrement(0), 1);
        Bits(FrameworkMath.BitIncrement(negativeZero), 1);
        Bits(FrameworkMath.BitDecrement(0), unchecked((long)0x8000000000000001UL));
        Bits(FrameworkMath.BitDecrement(negativeZero), unchecked((long)0x8000000000000001UL));
        Bits(FrameworkMath.BitIncrement(-double.Epsilon), long.MinValue);
        Bits(FrameworkMath.BitDecrement(double.Epsilon), 0);
        Bits(FrameworkMath.BitIncrement(double.Epsilon), 2);
        Bits(FrameworkMath.BitDecrement(-double.Epsilon), unchecked((long)0x8000000000000002UL));
        Check(double.IsPositiveInfinity(FrameworkMath.BitIncrement(double.MaxValue)), "Incrementing max finite must overflow to infinity.");
        Check(double.IsNegativeInfinity(FrameworkMath.BitDecrement(-double.MaxValue)), "Decrementing min finite must overflow to negative infinity.");
        Check(FrameworkMath.BitIncrement(double.NegativeInfinity) == -double.MaxValue, "Incrementing negative infinity must produce min finite.");
        Check(FrameworkMath.BitDecrement(double.PositiveInfinity) == double.MaxValue, "Decrementing positive infinity must produce max finite.");
        Check(double.IsPositiveInfinity(FrameworkMath.BitIncrement(double.PositiveInfinity)), "Positive infinity is the increment endpoint.");
        Check(double.IsNegativeInfinity(FrameworkMath.BitDecrement(double.NegativeInfinity)), "Negative infinity is the decrement endpoint.");
        Check(double.IsNaN(FrameworkMath.BitIncrement(double.NaN)) && double.IsNaN(FrameworkMath.BitDecrement(double.NaN)), "NaN must remain NaN.");
    }

    private static void Bits(double actual, long expected)
    {
        var actualBits = BitConverter.DoubleToInt64Bits(actual);
        Check(actualBits == expected, "Expected IEEE-754 bits " + expected.ToString("x16") + ", got " + actualBits.ToString("x16") + ".");
    }

    private static void FrameworkElementaryValues()
    {
        GSLInitializer.Initialize();
        var evaluator = new ExpressionsEvaluator();
        var generated = evaluator.Evaluate("arsinh(x)", "", CalculationsMode.Real);
        foreach (var input in new[] { 1.0, -1.0, 1e-300, -1e-300, 1e300, -1e300 })
        {
            var expected = Math.Abs(input) < 1e-100 ? input : Math.Abs(input) > 1e100
                ? Math.Sign(input) * 691.4686750787737 : Math.Sign(input) * 0.88137358701954302523;
            var tolerance = Math.Abs(expected) * 2e-15;
            Near(ElementaryFunctions.arsinh(input), expected, tolerance);
            Near(generated.Evaluate(input), expected, tolerance);
        }
        var negativeZero = BitConverter.Int64BitsToDouble(long.MinValue);
        Bits(ElementaryFunctions.arsinh(negativeZero), long.MinValue);
        Bits(generated.Evaluate(negativeZero), long.MinValue);
        Bits(FrameworkElementaryMath.CopySign(3.0, negativeZero), unchecked((long)0xc008000000000000UL));
        Bits(FrameworkElementaryMath.CopySign(-3.0, 0.0), 0x4008000000000000L);
        var nanBits = unchecked((long)0x7ff8123456789abcUL);
        Bits(FrameworkElementaryMath.CopySign(BitConverter.Int64BitsToDouble(nanBits), -1), nanBits | long.MinValue);
        var complex = evaluator.Evaluate("arsinh(z)", "", CalculationsMode.Complex).Evaluate(new Complex(negativeZero, 0.5));
        Bits(complex.Real, long.MinValue);
        Near(complex.Imaginary, Math.PI / 6, 1e-15);
    }

    private static void NumericalMethodCatalog()
    {
        var evaluator = new ExpressionsEvaluator();
        // Constant integrands give an exact independent result even for Monte Carlo;
        // these delegate tables are the choices exposed in the desktop UI.
        var constant = evaluator.Evaluate("2", "", CalculationsMode.Real);
        foreach (var method in NumericalMethodsInfo.Instance.IntegrationMethods)
        {
            Run("desktop-integration-" + method.Value.Method.Name, () =>
            {
                var count = method.Value.Method.Name == "rombergMethod" ? 8 : 1024;
                var actual = method.Value(x => constant.Evaluate(x), 0, 1, count);
                if (method.Value.Method.Name == "monteCarloMethod" && actual == 1)
                    throw new KnownLimitationException("Existing Accord.Math 3.8.0 defect, reproduced on Mono/net462 and .NET 10/netstandard2.0: integral of constant 2 over [0,1] with 1024 samples returned 1; mathematically expected 2. The shared estimator is unchanged by the Mono restoration. This is not a successful numerical acceptance result.");
                Near(actual, 2, 1e-8);
            });
        }
        var linear = evaluator.Evaluate("x", "", CalculationsMode.Real);
        foreach (var method in NumericalMethodsInfo.Instance.DerrivationMethods)
        {
            Run("desktop-derivative-" + method.Value.Method.Name, () => Near(method.Value(x => linear.Evaluate(x), 2, 1, 1e-4), 1, 1e-5));
        }
        var root = evaluator.Evaluate("x-0.25", "", CalculationsMode.Real);
        foreach (var method in NumericalMethodsInfo.Instance.FunctionRootMethods)
        {
            Run("desktop-root-" + method.Value.Method.Name, () => Near(method.Value(x => root.Evaluate(x), 0, 1, 1e-8, 1024), .25, 1e-7));
        }
        Check(NumericalMethodsInfo.Instance.IntegrationMethods.Count == 8 && NumericalMethodsInfo.Instance.DerrivationMethods.Count == 5
            && NumericalMethodsInfo.Instance.FunctionRootMethods.Count == 5, "The expected 18 desktop numerical methods were not all exercised.");
    }

    private static void FunctionReference()
    {
        var renderer = typeof(TextEditorCodeEditor).Assembly.GetType("Computator.NET.Desktop.Controls.AutocompleteMenu.MonoReferenceText", true)
            .GetMethod("Render", BindingFlags.Static | BindingFlags.NonPublic);
        var catalog = new FunctionsDetailsFileSource().Details;
        Check(catalog.Count > 0, "The checked-in function reference was not copied into the runtime output.");
        foreach (var info in catalog.Values)
        {
            string rendered;
            try { rendered = (string)renderer.Invoke(null, new object[] { info }); }
            catch (Exception error) { throw new InvalidOperationException("Mono could not render checked-in reference '" + info.Signature + "'.", error); }
            Check(!string.IsNullOrWhiteSpace(rendered) && rendered.Contains(info.Title), "Reference lost its title: " + info.Signature);
            Check(!rendered.Contains("<div") && !rendered.Contains("<p>") && !rendered.Contains("<img"), "Reference exposed raw markup: " + info.Signature);
        }
        Console.WriteLine("  verified checked-in reference articles: " + catalog.Count);
        var type = typeof(TextEditorCodeEditor).Assembly.GetType("Computator.NET.Desktop.Controls.AutocompleteMenu.WebBrowserForm", true);
        using (var form = (System.Windows.Forms.Form)Activator.CreateInstance(type))
        {
            var info = new FunctionInfo
            {
                Title = "Sine reference",
                Signature = "sin(real x)",
                Description = "<p>Readable sine description.</p><p><img src=\"equation.png\" alt=\"sin(x)^2 + cos(x)^2 = 1\" /></p><p>x<sup>2</sup> + a<sub>1</sub></p>"
            };
            type.GetMethod("SetFunctionInfo", new[] { typeof(FunctionInfo) }).Invoke(form, new object[] { info });
            form.Show();
            System.Windows.Forms.Application.DoEvents();
            var text = form.Controls.OfType<System.Windows.Forms.RichTextBox>().Single();
            Check(text.ReadOnly && text.Text.StartsWith("Text reference", StringComparison.Ordinal), "Mono reference must disclose its readable text format.");
            Check(text.Text.Contains(info.Signature), "Reference must identify the function overload signature.");
            Check(text.Text.Contains("Readable sine description.") && text.Text.Contains("sin(x)^2 + cos(x)^2 = 1"), "Reference prose or equation alternative text was lost.");
            Check(text.Text.Contains("x^(2) + a_(1)"), "Reference superscripts/subscripts lost their mathematical meaning.");
            Check(!form.Controls.OfType<System.Windows.Forms.WebBrowser>().Any(), "Mono reference unexpectedly depends on the unrendered embedded browser.");
            Capture(form, "function-reference.png");
        }
    }

    private static void VerifyDesktopInputLayout(System.Windows.Forms.Form window, ISharedViewState state,
        Computator.NET.Desktop.Views.NumericalCalculationsView numerical)
    {
        // Exercise every parameter group through the real presenters. A visible
        // parent alone is insufficient: Mono can collapse its numeric children
        // to a few pixels, clip them, or cover them with another docked control.
        state.CurrentView = ViewName.Calculations;
        foreach (var mode in new[] { CalculationsMode.Real, CalculationsMode.Complex, CalculationsMode.Fxy })
        {
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
            System.Windows.Forms.Application.DoEvents();
            VerifyUsableInput(window, "valueForCalculationNumericUpDown");
            if (mode != CalculationsMode.Real) VerifyUsableInput(window, "calculationsImZnumericUpDown");
            VerifyUsableInput(window, "calculateButton");
            VerifyUsableInput(window, "calculationValueTextBox");
        }
        Capture(window, "calculator-inputs.png");
        EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
        state.CurrentView = ViewName.NumericalCalculations;
        foreach (var operation in new[] { Computator.NET.Localization.Strings.Integral,
            Computator.NET.Localization.Strings.Derivative, Computator.NET.Localization.Strings.Function_root })
        {
            numerical.SelectedOperation = operation;
            System.Windows.Forms.Application.DoEvents();
            var names = operation == Computator.NET.Localization.Strings.Derivative
                ? new[] { "nOrderDerivativeNumericUpDown", "xDerivativePointNumericUpDown", "epsTextBox" }
                : operation == Computator.NET.Localization.Strings.Integral
                    ? new[] { "aIntervalNumericUpDown", "bIntervalNumericUpDown", "nStepsNumericUpDown" }
                    : new[] { "aIntervalNumericUpDown", "bIntervalNumericUpDown", "nStepsNumericUpDown", "epsTextBox" };
            foreach (var name in names) VerifyUsableInput(window, name);
            VerifyUsableInput(window, "operationNumericalCalculationsComboBox");
            VerifyUsableInput(window, "methodNumericalCalculationsComboBox");
            VerifyUsableInput(window, "numericalOperationButton");
            VerifyUsableInput(window, "resultNumericalCalculationsTextBox");
        }
        Capture(window, "numerical-inputs.png");
    }

    private static void VerifyUsableInput(System.Windows.Forms.Form window, string name)
    {
        var control = Find(window, name);
        Check(control.Visible && control.Enabled && control.ClientSize.Width >= 60 && control.ClientSize.Height >= 18,
            "Desktop input is not usable: " + name + " " + control.Bounds);
        var rectangle = control.RectangleToScreen(control.ClientRectangle);
        Check(window.RectangleToScreen(window.ClientRectangle).Contains(rectangle), "Desktop input is outside MainView: " + name);
        var center = new System.Drawing.Point(rectangle.Left + rectangle.Width / 2, rectangle.Top + rectangle.Height / 2);
        for (var branch = control; branch.Parent != null; branch = branch.Parent)
        {
            var parent = branch.Parent;
            Check(parent.RectangleToScreen(parent.ClientRectangle).Contains(rectangle), "Desktop input is clipped by " + parent.GetType().Name + ": " + name);
            var hit = parent.GetChildAtPoint(parent.PointToClient(center), System.Windows.Forms.GetChildAtPointSkip.Invisible);
            Same(hit, branch, "Desktop input is covered by another control: " + name);
            if (parent == window) break;
        }
    }

    private static void LegacySettingsMigration()
    {
        var fixture = Environment.GetEnvironmentVariable("COMPUTATOR_MONO_LEGACY_FIXTURE");
        Check(!string.IsNullOrEmpty(fixture) && File.Exists(fixture), "The validation runner must generate the trusted historical-format NRBF fixture first.");
        var directory = Path.Combine(Path.GetTempPath(), "computator-mono-nrbf-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var legacyPath = Path.Combine(directory, "settings.dat");
            File.Copy(fixture, legacyPath);
            var original = File.ReadAllBytes(legacyPath);
            var imported = SettingsPersistence.Load(legacyPath);
            Equal(imported.Language, "pl-PL");
            Check(imported.CodeEditor == CodeEditorType.TextEditor, "Legacy unavailable editor preference was not normalized.");
            Check(imported.CalculationsErrors == CalculationsErrors.ShowError && imported.NumericalOutputNotation == NumericalOutputNotationType.EngineeringNotation,
                "Legacy calculation preferences were not imported.");
            Check(imported.FunctionsOrder == FunctionsOrder.Alphabetical && imported.TooltipType == TooltipType.Form, "Legacy reference preferences changed.");
            Near(imported.ExpressionFont.Size, 18.25, 0);
            Near(imported.ScriptingFont.Size, 14.5, 0);
            Equal(imported.ScriptingDirectory, @"C:\fixture\scripts");
            Equal(imported.WorkingDirectory, @"C:\fixture\workspace");
            Check(!imported.ShowParametersTypeInScripting, "Legacy Boolean preference changed.");
            SettingsPersistence.Save(legacyPath, imported);
            Check(File.ReadAllBytes(legacyPath).SequenceEqual(original), "Successful legacy import changed the original NRBF bytes.");
            Equal(SettingsPersistence.Load(legacyPath).Language, "pl-PL");
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void FrameworkRandomBounds()
    {
        foreach (var endpoint in new[] { int.MinValue, -1, 0, 1, int.MaxValue })
            Near(StatisticsFunctions.random(endpoint, endpoint), endpoint, 0);
        Check(double.IsNaN(StatisticsFunctions.random(-1)), "A negative single random bound must retain NaN behavior.");
        Expect<ArgumentOutOfRangeException>(() => StatisticsFunctions.random(1, 0));
        Parallel.For(0, 512, index =>
        {
            var value = StatisticsFunctions.random(int.MinValue, int.MaxValue);
            Check(value >= int.MinValue && value <= int.MaxValue && value == Math.Truncate(value), "Concurrent full-range random result was outside inclusive Int32 bounds.");
            value = StatisticsFunctions.random(int.MaxValue);
            Check(value >= 0 && value <= int.MaxValue && value == Math.Truncate(value), "Random upper endpoint overflowed.");
            value = StatisticsFunctions.random(int.MaxValue - 1, int.MaxValue);
            Check(value == int.MaxValue - 1.0 || value == int.MaxValue, "Two-value range at Int32.MaxValue overflowed.");
            value = StatisticsFunctions.random(1.0);
            Check(value >= 0 && value < 1, "Random double was outside [0,1).");
        });
        var evaluator = new ExpressionsEvaluator();
        Near(evaluator.Evaluate("random(2147483647,2147483647)", "", CalculationsMode.Real).Evaluate(0.0), int.MaxValue, 0);
        Near(evaluator.Evaluate("random(-2147483648,-2147483648)", "", CalculationsMode.Real).Evaluate(0.0), int.MinValue, 0);
    }

    private static void SettingsMigration()
    {
        var directory = Path.Combine(Path.GetTempPath(), "computator-mono-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var legacyPath = Path.Combine(directory, "settings.dat");
            var document = new SettingsDocument
            {
                Language = "pl-PL",
                ScriptingDirectory = Path.Combine(directory, "scripts"),
                CustomFunctionsDirectory = Path.Combine(directory, "functions"),
                WorkingDirectory = Path.Combine(directory, "workspace")
            };
            foreach (var oldEditorValue in new[] { 0, 1, 2 })
            {
                document.CodeEditor = (CodeEditorType)oldEditorValue;
                document.Validate();
                Check(document.CodeEditor == CodeEditorType.TextEditor, "Known Windows editor preference did not normalize to the available Mono editor.");
            }
            document.CodeEditor = (CodeEditorType)99;
            Expect<InvalidDataException>(() => document.Validate());
            document.CodeEditor = CodeEditorType.TextEditor;
            SettingsPersistence.Save(legacyPath, document);
            var loaded = SettingsPersistence.Load(legacyPath);
            Equal(loaded.Language, "pl-PL");
            Equal(loaded.WorkingDirectory, document.WorkingDirectory);
            Check(loaded.CodeEditor == CodeEditorType.TextEditor && (int)loaded.CodeEditor == 2, "Persisted TextEditor identity changed.");
            var firstJson = File.ReadAllBytes(SettingsPersistence.JsonPath(legacyPath));
            document.Language = "en";
            SettingsPersistence.Save(legacyPath, document);
            var backups = Directory.GetFiles(directory, "settings.json.backup-*");
            Check(backups.Length == 1 && File.ReadAllBytes(backups[0]).SequenceEqual(firstJson), "Replacing JSON did not preserve its exact previous contents.");
            Equal(SettingsPersistence.Load(legacyPath).Language, "en");
            File.WriteAllText(SettingsPersistence.JsonPath(legacyPath), "{ invalid JSON");
            Expect<Exception>(() => SettingsPersistence.Save(legacyPath, document));
            Equal(File.ReadAllText(SettingsPersistence.JsonPath(legacyPath)), "{ invalid JSON");
            File.Delete(SettingsPersistence.JsonPath(legacyPath));
            var damagedLegacy = new byte[] { 12, 34, 56, 78 };
            File.WriteAllBytes(legacyPath, damagedLegacy);
            Expect<Exception>(() => SettingsPersistence.Load(legacyPath));
            Check(File.ReadAllBytes(legacyPath).SequenceEqual(damagedLegacy), "Corrupt legacy settings were changed while parsing.");
            Check(!File.Exists(SettingsPersistence.JsonPath(legacyPath)), "A failed legacy parse must not write a replacement profile.");
        }
        finally { Directory.Delete(directory, true); }
    }

    private static void VerifyNestedScriptTree(ScriptingView scripting, string directory)
    {
        var nestedDirectory = Path.Combine(directory, "nested scripts");
        Directory.CreateDirectory(nestedDirectory);
        var path = Path.Combine(nestedDirectory, "zażółć.tsl");
        File.WriteAllText(path, "write(144);");
        var tree = (DirectoryTree)scripting.SolutionExplorerView.DirectoryTree;
        tree.RefreshDisplay();
        var nested = tree.Nodes[0].Nodes.Cast<System.Windows.Forms.TreeNode>().Single(node => node.Text == "nested scripts");
        nested.Expand();
        var script = nested.Nodes.Cast<System.Windows.Forms.TreeNode>().Single(node => node.Text == "zażółć.tsl");
        Equal(script.FullPath, path);
        tree.SelectedNode = script;
        System.Windows.Forms.Application.DoEvents();
        Check(scripting.CodeEditorView.ContainsDocument(path), "Selecting a nested Unix tree node did not open its file in the visible scripting editor.");
        Equal(scripting.CodeEditorView.Text, "write(144);");
    }

    private static void UnixDocumentSafety()
    {
        // These tests invoke the production internal helper, not a copied implementation.
        var safety = typeof(TextEditorCodeEditor).Assembly.GetType("Computator.NET.Desktop.Controls.CodeEditors.DocumentFileSafety", true);
        var capture = safety.GetMethod("Capture", BindingFlags.Static | BindingFlags.NonPublic);
        var write = safety.GetMethod("Write", BindingFlags.Static | BindingFlags.NonPublic);
        var sameFile = safety.GetMethod("AreSameFile", BindingFlags.Static | BindingFlags.NonPublic);
        var directory = Path.Combine(Path.GetTempPath(), "computator-mono-file-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "document.tsl");
            var caseVariant = Path.Combine(directory, "DOCUMENT.tsl");
            File.WriteAllText(path, "original");
            File.WriteAllText(caseVariant, "different file");
            var error = Expect<TargetInvocationException>(() => sameFile.Invoke(null, new object[] { path, caseVariant }));
            Check(error.InnerException is IOException, "Case-only aliases must fail visibly on Unix rather than sharing one buffer.");
            var baseline = capture.Invoke(null, new object[] { path });
            baseline = write.Invoke(null, new object[] { path, "saved π", baseline, null, null, null });
            Equal(File.ReadAllText(path), "saved π");
            Equal(File.ReadAllText(caseVariant), "different file");
            File.WriteAllText(path, "external");
            error = Expect<TargetInvocationException>(() => write.Invoke(null, new object[] { path, "must not overwrite", baseline, null, null, null }));
            Check(error.InnerException is IOException, "An externally edited document must fail its save baseline check.");
            Equal(File.ReadAllText(path), "external");
            var link = Path.Combine(directory, "symbolic-link.tsl");
            if (symlink(path, link) != 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "Could not create the isolated symbolic-link test fixture.");
            baseline = capture.Invoke(null, new object[] { link });
            error = Expect<TargetInvocationException>(() => write.Invoke(null, new object[] { link, "must not replace a link", baseline, null, null, null }));
            Check(error.InnerException is IOException, "Writing through a symbolic link must be rejected.");
            Equal(File.ReadAllText(path), "external");
            Check((File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0, "The symbolic link was unexpectedly replaced.");
            Check(Directory.GetFiles(directory, ".computator-save-*.tmp").Length == 0, "Failed save left staged temporary files.");
        }
        finally { Directory.Delete(directory, true); }
    }

    [DllImport("libc", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern int symlink(string target, string linkPath);
}
