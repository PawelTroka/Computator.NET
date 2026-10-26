using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 2 || Type.GetType("Mono.Runtime") == null)
        {
            Console.Error.WriteLine("Usage: mono Computator.NET.Mono.StartupProbe.exe /absolute/path/Computator.NET.exe /absolute/path/screenshots");
            return 2;
        }

        var applicationPath = Path.GetFullPath(args[0]);
        var applicationDirectory = Path.GetDirectoryName(applicationPath);
        var domain = AppDomain.CreateDomain("Computator.NET.Mono.ProductionStartup", null, new AppDomainSetup
        {
            ApplicationBase = applicationDirectory,
            ConfigurationFile = applicationPath + ".config"
        });
        try
        {
            var runner = (StartupRunner)domain.CreateInstanceFromAndUnwrap(
                Assembly.GetExecutingAssembly().Location, typeof(StartupRunner).FullName);
            return runner.Run(applicationPath, Path.GetFullPath(args[1]));
        }
        finally { AppDomain.Unload(domain); }
    }
}

public sealed class StartupRunner : MarshalByRefObject
{
    public override object InitializeLifetimeService() { return null; }

    public int Run(string applicationPath, string outputDirectory)
    {
        var applicationDirectory = Path.GetDirectoryName(applicationPath);
        Directory.CreateDirectory(outputDirectory);
        Directory.SetCurrentDirectory(applicationDirectory);
        // The application owns the startup path: DPI/text configuration, logging,
        // GSL, profile migration, examples, splash, composition and message loop.
        // Reflection only provides a deterministic close after the real window paints.
        var watch = Stopwatch.StartNew();
        var shown = false;
        var complexRequested = false;
        double? complexStartedAt = null;
        double? firstVisibleAt = null;
        Exception failure = null;
        using (var timer = new Timer { Interval = 200 })
        {
            timer.Tick += (sender, eventArgs) =>
            {
                try
                {
                    var form = Application.OpenForms.Cast<Form>().SingleOrDefault(candidate =>
                        candidate.GetType().FullName == "Computator.NET.Desktop.Views.MainView" && candidate.Visible);
                    if (form == null)
                    {
                        if (watch.Elapsed.TotalSeconds > 45)
                            throw new TimeoutException("Production startup did not show MainView within 45 seconds.");
                        return;
                    }
                    if (form.Handle == IntPtr.Zero || form.Controls.Count == 0)
                        throw new InvalidOperationException("MainView has no initialized native window/control tree.");
                    if (!firstVisibleAt.HasValue)
                    {
                        firstVisibleAt = watch.Elapsed.TotalSeconds;
                        form.Activate();
                        form.Refresh();
                        return;
                    }
                    if (watch.Elapsed.TotalSeconds - firstVisibleAt.Value < 1) return;
                    if (complexRequested)
                    {
                        var chart = Descendants(form).Single(control => control.GetType().Name == "ComplexChart");
                        using (var visible = CaptureX11Pixels(chart))
                        {
                            var colorPixels = CountDomainColorPixels(visible);
                            if (colorPixels > 200 || watch.Elapsed.TotalSeconds - complexStartedAt.Value > 30)
                            {
                                visible.Save(Path.Combine(outputDirectory, "production-complex-after.png"), ImageFormat.Png);
                                Console.WriteLine("Production complex chart: visible=" + chart.Visible +
                                    " bounds=" + chart.Bounds + " colored-pixels=" + colorPixels);
                                if (colorPixels <= 200)
                                    throw new InvalidOperationException("Production executable did not paint z² after its real Draw action.");
                                shown = true;
                                timer.Stop();
                                form.Close();
                            }
                        }
                        return;
                    }
                    AssertUsableLayout(form);
                    foreach (var name in new[] { "Computator.NET.Desktop", "Computator.NET.Core", "Computator.NET.Charting", "Computator.NET.Engine" })
                    {
                        var loaded = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == name);
                        if (!string.Equals(Path.GetDirectoryName(loaded.Location), applicationDirectory, StringComparison.Ordinal))
                            throw new InvalidOperationException(name + " was loaded outside the executable's own directory: " + loaded.Location);
                    }
                    using (var bitmap = new Bitmap(form.Width, form.Height))
                    {
                        // Mono's Form.DrawToBitmap can omit all native children.
                        // Read the real X11 pixels after a complete paint cycle.
                        using (var graphics = Graphics.FromImage(bitmap))
                            graphics.CopyFromScreen(form.Location, Point.Empty, bitmap.Size);
                        var colors = new System.Collections.Generic.HashSet<int>();
                        for (var x = 0; x < bitmap.Width; x += 7)
                            for (var y = 0; y < bitmap.Height; y += 7)
                                colors.Add(bitmap.GetPixel(x, y).ToArgb());
                        if (colors.Count < 16)
                            throw new InvalidOperationException("The real application window did not paint its control tree (" + colors.Count + " sampled colors).");
                        bitmap.Save(Path.Combine(outputDirectory, "startup.png"), ImageFormat.Png);
                    }
                    BeginProductionComplexPlot(form, outputDirectory);
                    complexRequested = true;
                    complexStartedAt = watch.Elapsed.TotalSeconds;
                }
                catch (Exception error)
                {
                    failure = error;
                    timer.Stop();
                    Application.Exit();
                }
            };
            try
            {
                timer.Start();
                var application = Assembly.LoadFrom(applicationPath);
                application.EntryPoint.Invoke(null, application.EntryPoint.GetParameters().Length == 0 ? null : new object[] { new string[0] });
                if (failure != null) throw failure;
                if (!shown) throw new InvalidOperationException("Production entry point returned before MainView was shown.");
                Console.WriteLine("PASS production executable startup, dependency loading, rendering and graceful close ({0:F2}s)", watch.Elapsed.TotalSeconds);
                return 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine("FAIL production executable startup: " + error);
                return 1;
            }
        }
    }


    private static void BeginProductionComplexPlot(Form form, string outputDirectory)
    {
        // Exercise mode selection and Draw in the *production* executable's
        // Application.Run loop, including its real injected presenters and views.
        var menuField = form.GetType().GetField("fdsfdsToolStripMenuItem",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (menuField == null)
            throw new InvalidOperationException("Missing production complex-mode menu.");
        var mode = menuField.GetValue(form) as ToolStripMenuItem;
        if (mode == null)
            throw new InvalidOperationException("Complex-mode menu has unexpected type.");
        mode.PerformClick();
        var children = Descendants(form).ToArray();
        var chart = children.Single(control => control.GetType().Name == "ComplexChart");
        var expression = children.Single(control => control.GetType().Name == "ExpressionTextBox");
        var draw = children.Single(control => control.Name == "addToChartButton") as Button;
        if (draw == null || !draw.Visible || !chart.Visible || !chart.IsHandleCreated ||
            chart.ClientSize.Width < 200 || chart.ClientSize.Height < 150)
            throw new InvalidOperationException("Production complex chart or Draw is not actually visible: " +
                "chart=" + chart.Bounds + " visible=" + chart.Visible + " draw=" + draw?.Visible);
        using (var before = CaptureX11Pixels(chart))
            before.Save(Path.Combine(outputDirectory, "production-complex-before.png"), ImageFormat.Png);
        expression.Text = "z²";
        draw.PerformClick();
        Console.WriteLine("Production executable complex UI Draw was clicked; bounds=" + chart.Bounds);
    }

    private static Bitmap CaptureX11Pixels(Control control)
    {
        var rectangle = control.RectangleToScreen(control.ClientRectangle);
        if (rectangle.Width < 1 || rectangle.Height < 1)
            throw new InvalidOperationException("Invalid native screen rectangle for " + control.Name);
        var image = new Bitmap(rectangle.Width, rectangle.Height);
        using (var graphics = Graphics.FromImage(image))
            graphics.CopyFromScreen(rectangle.Location, Point.Empty, rectangle.Size);
        return image;
    }

    private static int CountDomainColorPixels(Bitmap bitmap)
    {
        var pixels = 0;
        for (var x = 10; x < bitmap.Width - 10; x += 5)
            for (var y = 10; y < bitmap.Height - 10; y += 5)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (Math.Max(pixel.R, Math.Max(pixel.G, pixel.B)) -
                    Math.Min(pixel.R, Math.Min(pixel.G, pixel.B)) > 45 &&
                    Math.Max(pixel.R, Math.Max(pixel.G, pixel.B)) > 85)
                    pixels++;
            }
        return pixels;
    }
    private static System.Collections.Generic.IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }

    private static void AssertUsableLayout(Form form)
    {
        var descendants = Descendants(form).ToArray();
        foreach (var type in new[] { "MenuStripView", "ToolBarView" })
            AssertVisibleControl(form, descendants.Single(control => control.GetType().Name == type), 150, 18);
        AssertVisibleControl(form, descendants.Single(control => control.GetType().Name == "ExpressionTextBox"), 120, 20);
        foreach (var name in new[] { "x0NumericUpDown", "xnNumericUpDown", "y0NumericUpDown", "yNNumericUpDown" })
        {
            var input = descendants.Single(control => control.Name == name);
            // NumericUpDown's client area excludes its border. Require enough
            // actual text height for its font, including smaller chart inputs.
            AssertVisibleControl(form, input, 60, input.Font.Height);
        }
    }

    private static void AssertVisibleControl(Form form, Control control, int minimumWidth, int minimumHeight)
    {
        if (!control.Visible || !control.Enabled || control.ClientSize.Width < minimumWidth || control.ClientSize.Height < minimumHeight)
            throw new InvalidOperationException("Required startup control is not usable: " + control.GetType().Name + " / " + control.Name
                + " bounds=" + control.Bounds + " client=" + control.ClientSize + " fontHeight=" + control.Font.Height
                + " visible=" + control.Visible + " enabled=" + control.Enabled);
        var rectangle = control.RectangleToScreen(control.ClientRectangle);
        if (!form.RectangleToScreen(form.ClientRectangle).Contains(rectangle))
            throw new InvalidOperationException("Required startup control is clipped outside MainView: " + control.Name + " " + rectangle);
        for (var ancestor = control.Parent; ancestor != null && ancestor != form; ancestor = ancestor.Parent)
            if (!ancestor.RectangleToScreen(ancestor.ClientRectangle).Contains(rectangle))
                throw new InvalidOperationException("Required startup control is clipped by " + ancestor.GetType().Name + ": " + control.Name);
        var center = new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top + rectangle.Height / 2);
        for (var branch = control; branch.Parent != null; branch = branch.Parent)
        {
            var parent = branch.Parent;
            var hit = parent.GetChildAtPoint(parent.PointToClient(center), GetChildAtPointSkip.Invisible);
            if (hit != branch)
                throw new InvalidOperationException("Required startup control is covered by another control: " + control.Name
                    + "; expected " + branch.GetType().Name + ", hit " + (hit == null ? "nothing" : hit.GetType().Name));
            if (parent == form) break;
        }
    }
}
