using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Computator.NET.Charting.ComplexCharting;
using Computator.NET.Core.Abstract.Controls;
using Computator.NET.Core.Model;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Events;
using Computator.NET.DataTypes.Functions;
using Computator.NET.Desktop.Bootstrapping;
using Computator.NET.Desktop.Views;

internal static partial class Program
{
    // Run inside the *actual* bootstrapped desktop window, not a detached chart
    // created for GetImage(). Capture the X11 screen itself, not DrawToBitmap.
    private static void VerifyMonoComplexChartVisible(MainView window,
        WinFormsBootstrapper bootstrapper, ISharedViewState state, RecordingErrors errors)
    {
        state.CurrentView = ViewName.Charting;
        EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Complex));
        var view = bootstrapper.Create<ChartingView>();
        var chart = (ComplexChart)view.ComplexChart;
        chart.Quality = 50; // Reproduce the production default sampling quality.
        Application.DoEvents();
        Console.WriteLine("Complex UI initial: " + DescribeComplexLayout(chart));
        Check(chart.Parent.Controls.GetChildIndex(chart) == 0,
            "Active complex chart is underneath another chart window in Mono/X11.");
        Check(window.Visible && chart.Visible && chart.IsHandleCreated && chart.Width > 200 && chart.Height > 150,
            "Complex plot does not have a visible, sized, native X11 control: " + DescribeComplexLayout(chart));

        using (var blank = ScreenshotComplexControl(chart, "complex-visible-before.png"))
            Console.WriteLine("Complex UI before draw colored pixels: " + CountDomainColorPixels(blank));

        bootstrapper.Create<ITextProvider>().Text = "z²";
        var draw = Find(window, "addToChartButton") as Button;
        Check(draw != null && draw.Visible && draw.Enabled, "Real desktop Draw button is not available.");
        var paintCalls = 0;
        chart.Paint += (sender, args) => paintCalls++;
        for (var attempt = 0; attempt < 5; attempt++)
            ClickActualX11Button(draw); // Real pointer/button events, as in production.
        Console.WriteLine("X11 Draw clicks delivered (five attempted), initial chart paints: " + paintCalls);

        // The production chart uses async sampling. Pump the real message loop
        // until its background calculation and native invalidation have completed.
        var timer = Stopwatch.StartNew();
        Bitmap last = null;
        int colored = 0;
        while (timer.Elapsed < TimeSpan.FromSeconds(25))
        {
            Application.DoEvents();
            if (last != null) last.Dispose();
            last = ScreenshotComplexControl(chart, null);
            colored = CountDomainColorPixels(last);
            if (colored > 200) break;
            Thread.Sleep(60);
        }
        try
        {
            last.Save(System.IO.Path.Combine(outputDirectory, "complex-visible-after.png"), ImageFormat.Png);
            Console.WriteLine("Complex UI after draw: " + DescribeComplexLayout(chart) +
                "; colored pixels: " + colored + "; elapsed: " + timer.Elapsed + "; chart paint calls: " + paintCalls);
            Check(colored > 200,
                "z² was not painted in the actual Linux/X11 window despite clicking Draw; " +
                "on-screen domain color pixels: " + colored);
        }
        finally { if (last != null) last.Dispose(); }

        using (var export = chart.GetImage(320, 220))
            AssertImageHasVariation(export, 40);

        // Switch expression and require the live native screen to change,
        // rather than merely inspecting GetImage() again.
        using (var zSquared = ScreenshotComplexControl(chart, null))
        {
            bootstrapper.Create<ITextProvider>().Text = "z³";
            ClickActualX11Button(draw);
            using (var zCubed = WaitForComplexPixels(chart, "complex-visible-z-cubed.png",
                image => DifferentComplexPixels(zSquared, image) > 600, 25))
                Check(CountDomainColorPixels(zCubed) > 200, "Changing expression hid the complex domain colors.");
        }

        // Real -> 3D -> Complex must make the selected native child frontmost,
        // including the separate Mono 3D implementation introduced by #296.
        foreach (var mode in new[] { CalculationsMode.Real, CalculationsMode.Fxy, CalculationsMode.Complex })
        {
            EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(mode));
            Application.DoEvents();
            Check(chart.Visible == (mode == CalculationsMode.Complex),
                "Complex chart visibility did not follow calculation mode " + mode);
            if (mode == CalculationsMode.Complex)
                Check(chart.Parent.Controls.GetChildIndex(chart) == 0,
                    "Switching back to Complex left it behind an X11 sibling.");
        }
        chart.Refresh();
        Application.DoEvents();
        using (var returning = ScreenshotComplexControl(chart, null))
            Check(CountDomainColorPixels(returning) > 200, "Complex chart disappeared after mode switching.");

        var normalSize = window.ClientSize;
        try
        {
            window.ClientSize = new Size(normalSize.Width - 100, normalSize.Height - 75);
            Application.DoEvents();
            Check(chart.Width > 200 && chart.Height > 150 && draw.Visible,
                "Resizing clipped the active complex plot or its Draw button.");
            using (var resized = WaitForComplexPixels(chart, "complex-visible-resized.png",
                image => CountDomainColorPixels(image) > 200, 15))
                AssertImageHasVariation(resized, 50);
        }
        finally { window.ClientSize = normalSize; Application.DoEvents(); }

        var clear = Find(window, "clearChartButton") as Button;
        Check(clear != null && clear.Visible, "Desktop Clear button is not available.");
        clear.PerformClick();
        Application.DoEvents();
        using (var cleared = ScreenshotComplexControl(chart, "complex-visible-cleared.png"))
            Check(CountDomainColorPixels(cleared) == 0, "Clear left stale colors on the visible chart.");

        // A compilation error must be reported through the normal error handler,
        // must not repaint old colors, and must not prevent a later valid Draw.
        var errorsBefore = errors.Messages.Count;
        bootstrapper.Create<ITextProvider>().Text = "MissingMonoFunction(z)";
        ClickActualX11Button(draw);
        Check(errors.Messages.Count > errorsBefore, "Invalid complex expression was not reported.");
        using (var invalid = ScreenshotComplexControl(chart, null))
            Check(CountDomainColorPixels(invalid) == 0, "Invalid Draw repainted stale domain colors.");
        errors.Messages.Clear();

        // Clear while an asynchronous drawing is being scheduled: obsolete
        // samples must never appear later, including during rapid user input.
        bootstrapper.Create<ITextProvider>().Text = "z⁴";
        ClickActualX11Button(draw);
        clear.PerformClick();
        PumpComplexUiFor(TimeSpan.FromMilliseconds(600));
        using (var stale = ScreenshotComplexControl(chart, null))
            Check(CountDomainColorPixels(stale) == 0, "An obsolete async render repainted after Clear.");

        bootstrapper.Create<ITextProvider>().Text = "z²";
        ClickActualX11Button(draw);
        using (var recovered = WaitForComplexPixels(chart, "complex-visible-recovered.png",
            image => CountDomainColorPixels(image) > 200, 25))
            AssertImageHasVariation(recovered, 50);

        // The plotting delegate itself may throw only on the worker task,
        // after the Draw handler returned. Confirm the async-void continuation
        // cannot terminate the running desktop or permanently poison redraws.
        var failingFunction = new Function(
            new Func<Complex, Complex>(z =>
            {
                throw new InvalidOperationException("Intentional complex rendering failure");
            }),
            FunctionType.Complex) { Name = "Intentional complex rendering failure" };
        chart.AddFunction(failingFunction);
        var failureWatch = Stopwatch.StartNew();
        while (failureWatch.Elapsed < TimeSpan.FromSeconds(10) &&
               string.IsNullOrWhiteSpace(chart.LastRenderError))
        {
            Application.DoEvents();
            Thread.Sleep(40);
        }
        Check(chart.LastRenderError != null &&
            chart.LastRenderError.Contains("Intentional complex rendering failure"),
            "Asynchronous evaluation did not deliver an observable UI error.");
        Application.DoEvents();
        using (var failure = ScreenshotComplexControl(chart, "complex-visible-render-failure.png"))
            Check(failure.Width > 200 && failure.Height > 150,
                "Failed complex render destroyed the native window.");

        chart.ClearAll();
        bootstrapper.Create<ITextProvider>().Text = "z²";
        ClickActualX11Button(draw);
        using (var repaired = WaitForComplexPixels(chart, "complex-visible-after-render-failure.png",
            image => CountDomainColorPixels(image) > 200, 25))
            AssertImageHasVariation(repaired, 50);
        Check(string.IsNullOrEmpty(chart.LastRenderError),
            "A successful redraw did not clear the earlier asynchronous error.");

        // Restore the original application mode for existing desktop workflow checks.
        EventAggregator.Instance.Publish(new CalculationsModeChangedEvent(CalculationsMode.Real));
    }


    private static Bitmap WaitForComplexPixels(Control control, string filename,
        Func<Bitmap, bool> accepted, int timeoutSeconds)
    {
        var watch = Stopwatch.StartNew();
        Bitmap latest = null;
        while (watch.Elapsed < TimeSpan.FromSeconds(timeoutSeconds))
        {
            Application.DoEvents();
            if (latest != null) latest.Dispose();
            latest = ScreenshotComplexControl(control, null);
            if (accepted(latest)) break;
            Thread.Sleep(75);
        }
        try
        {
            latest.Save(System.IO.Path.Combine(outputDirectory, filename), ImageFormat.Png);
            Check(accepted(latest),
                "Visible X11 complex chart did not reach " + filename + " after " + watch.Elapsed);
            var result = latest;
            latest = null;
            return result;
        }
        finally { if (latest != null) latest.Dispose(); }
    }

    private static int DifferentComplexPixels(Bitmap first, Bitmap second)
    {
        Check(first.Size == second.Size, "Expression switch unexpectedly resized the chart.");
        var changed = 0;
        for (var x = 10; x < first.Width - 10; x += 5)
            for (var y = 10; y < first.Height - 10; y += 5)
                if (first.GetPixel(x, y).ToArgb() != second.GetPixel(x, y).ToArgb())
                    changed++;
        return changed;
    }

    private static void PumpComplexUiFor(TimeSpan duration)
    {
        var watch = Stopwatch.StartNew();
        while (watch.Elapsed < duration)
        {
            Application.DoEvents();
            Thread.Sleep(35);
        }
    }

    private static string DescribeComplexLayout(ComplexChart chart)
    {
        if (chart.Parent == null) return "unparented";
        return string.Format("bounds={0} visible={1} handle={2} childIndex={3} siblings=[{4}]",
            chart.Bounds, chart.Visible, chart.IsHandleCreated,
            chart.Parent.Controls.GetChildIndex(chart),
            string.Join("; ", chart.Parent.Controls.Cast<Control>().Select(c =>
                c.GetType().Name + ":" + c.Visible + ":" + c.Bounds).ToArray()));
    }

    private static Bitmap ScreenshotComplexControl(Control chart, string name)
    {
        var rect = chart.RectangleToScreen(chart.ClientRectangle);
        Check(rect.Width > 0 && rect.Height > 0,
            "Complex chart has no X11 rectangle.");
        var bitmap = new Bitmap(rect.Width, rect.Height);
        using (var graphics = Graphics.FromImage(bitmap))
            graphics.CopyFromScreen(rect.Location, Point.Empty, rect.Size);
        if (name != null)
            bitmap.Save(System.IO.Path.Combine(outputDirectory, name), ImageFormat.Png);
        return bitmap;
    }

    private static int CountDomainColorPixels(Bitmap bitmap)
    {
        int count = 0;
        for (int x = 10; x < bitmap.Width - 10; x += 5)
        for (int y = 10; y < bitmap.Height - 10; y += 5)
        {
            var p = bitmap.GetPixel(x,y);
            if (Math.Max(p.R,Math.Max(p.G,p.B)) - Math.Min(p.R,Math.Min(p.G,p.B)) > 45 &&
                Math.Max(p.R,Math.Max(p.G,p.B)) > 85)
                count++;
        }
        return count;
    }
    [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    private static extern IntPtr ComplexXOpenDisplay(IntPtr displayName);
    [DllImport("libX11.so.6", EntryPoint = "XCloseDisplay")]
    private static extern int ComplexXCloseDisplay(IntPtr display);
    [DllImport("libX11.so.6", EntryPoint = "XSync")]
    private static extern int ComplexXSync(IntPtr display, int discard);
    [DllImport("libXtst.so.6", EntryPoint = "XTestFakeMotionEvent")]
    private static extern int ComplexXTestFakeMotionEvent(IntPtr display, int screen, int x, int y, ulong delay);
    [DllImport("libXtst.so.6", EntryPoint = "XTestFakeButtonEvent")]
    private static extern int ComplexXTestFakeButtonEvent(IntPtr display, uint button, int press, ulong delay);

    private static void ClickActualX11Button(Control button)
    {
        var center = button.PointToScreen(new Point(button.Width / 2, button.Height / 2));
        var display = ComplexXOpenDisplay(IntPtr.Zero);
        Check(display != IntPtr.Zero, "Cannot open native X11 display for Draw mouse input.");
        try
        {
            Check(ComplexXTestFakeMotionEvent(display, 0, center.X, center.Y, 0) != 0,
                "Native mouse motion to Draw button failed.");
            ComplexXSync(display, 0);
            Application.DoEvents();
            Check(ComplexXTestFakeButtonEvent(display, 1, 1, 0) != 0, "X11 Draw mouse press failed.");
            ComplexXSync(display, 0);
            Application.DoEvents();
            Check(ComplexXTestFakeButtonEvent(display, 1, 0, 0) != 0, "X11 Draw mouse release failed.");
            ComplexXSync(display, 0);
            Application.DoEvents();
        }
        finally { ComplexXCloseDisplay(display); }
    }

}
